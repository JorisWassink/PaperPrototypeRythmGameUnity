using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;



public class BlockSpawner : MonoBehaviour
{
    public static BlockSpawner Instance { get; private set; }
    
    [Header("Spawn Settings")]
    [SerializeField] private Material blockMaterial;
    [SerializeField] private string midiFile;
    [SerializeField] private GameObject block;
    [SerializeField] private float blockSpeed;
    private MidiPlayer _midiPlayer;
    private List<Transform> _spawnPoints;

    public List<Transform> allGoals; // Assign all goal points (unsorted) in Inspector
    private List<Transform> goalPoints;

    private void Awake()
    {
        allGoals = new List<Transform>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    void Start()
    {
        InitializeMidiPlayer();
        _spawnPoints = new List<Transform>();
        foreach (var trans in GetComponentsInChildren<Transform>())
        {
            if (trans.gameObject.CompareTag("Spawner"))
            {
                _spawnPoints.Add(trans.transform);
            }
        }
        
        goalPoints = new List<Transform>();

        foreach (var spawner in _spawnPoints)
        {
            Transform closestGoal = allGoals
                .OrderBy(g => Vector3.Distance(spawner.position, g.position))
                .First();

            goalPoints.Add(closestGoal);
            allGoals.Remove(closestGoal); // Prevent duplicates
        }

    }

    void InitializeMidiPlayer()
    {
        try
        {
            _midiPlayer = gameObject.AddComponent<MidiPlayer>();
            _midiPlayer.InitializePlayer(midiFile);
            _midiPlayer.NotesPlaybackStartedEvent += OnNotesPlaybackStarted;
            _midiPlayer.NotesPlaybackFinishedEvent += OnNotesPlaybackFinished;
            _midiPlayer.StartPlaying += StartPlaying;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }


    private void StartPlaying(object sender, Event e)
    {
        var startTime = AudioSettings.dspTime;

        var note = _midiPlayer.Notes[0];
        var time = EstimateFallTime(note, startTime);
        //_midiPlayer.StartPlayback();
        StartCoroutine(WaitAndPlay(time));
        StartCoroutine(SpawnNotesWithTiming(startTime));
    }
    
    private IEnumerator SpawnNotesWithTiming(double startTime)
    {
        MidiFile file = _midiPlayer.MidiFile;
        var tempoMap = file.GetTempoMap();

        foreach (var note in _midiPlayer.Notes)
        {
            var noteTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap);
            var noteTimeInSeconds = noteTimeSpan.TotalSeconds;
            var noteSpawnTime = startTime + noteTimeInSeconds;
            var waitTime = noteSpawnTime - AudioSettings.dspTime;

            if (waitTime > 0)
                yield return new WaitForSecondsRealtime((float)waitTime);

            SpawnMidiNote(block, note);
        }
    }
    
    private IEnumerator WaitAndPlay(float waitTime)
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - start < waitTime)
            yield return null;

        _midiPlayer.StartPlayback();
    }

    private void OnApplicationQuit()
    {
        _midiPlayer.NotesPlaybackStartedEvent -= OnNotesPlaybackStarted;
        _midiPlayer.NotesPlaybackFinishedEvent -= OnNotesPlaybackFinished;
        _midiPlayer.OnQuit();
    }

    private void OnDestroy()
    {
        OnApplicationQuit();
    }

    private void OnNotesPlaybackFinished(object sender, NotesEventArgs e) { }
    
    private void OnNotesPlaybackStarted(object sender, NotesEventArgs e) { }

    float EstimateFallTime(Note note, double startTime)
    {
        var closestSpawn = GetClosestSpawnPoint(note);

        float DistanceIgnoreY(Vector3 a, Vector3 b) =>
            Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));

        // Find closest goal ignoring Y
        var goal = goalPoints.OrderBy(g => DistanceIgnoreY(g.position, closestSpawn.position)).First();

        // Calculate full 3D distance including Y for fall time
        var distance = Vector3.Distance(closestSpawn.position, goal.position);

        var speed = note.Velocity / 100f * blockSpeed;
        
        
        // Load MIDI file and tempo map once
        MidiFile file = _midiPlayer.MidiFile;
        var tempoMap = file.GetTempoMap();
        
        var noteTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap);
        var noteTimeInSeconds = (float)noteTimeSpan.TotalSeconds;

        // Calculate when the note should spawn
        var noteSpawnTime = startTime + noteTimeInSeconds;
        var waitTime = noteSpawnTime - (float)AudioSettings.dspTime;
        
        
        return (distance / Mathf.Max(0.01f, speed)) - noteTimeInSeconds; // prevent div by 0
    }
    
    void SpawnMidiNote(GameObject block, Note note)
    {
        var closest = GetClosestSpawnPoint(note);

        var spawnedNote = Instantiate(block, closest.position, Quaternion.identity, closest);
        spawnedNote.GetComponent<Renderer>().material = closest.GetComponent<Renderer>().material;
        spawnedNote.GetComponent<MovingBlock>().speed = note.Velocity / 100f * blockSpeed;
        Debug.Log($"spawnedNote speed: {spawnedNote.GetComponent<MovingBlock>().speed}");
        if (spawnedNote.GetComponent<LongBlock>() != null)
        {
            spawnedNote.GetComponent<LongBlock>().length = note.Length;
        }
    }

    private Transform GetClosestSpawnPoint(Note note)
    {
        var minNote = _midiPlayer.MinNote.NoteNumber;
        var maxNote = _midiPlayer.MaxNote.NoteNumber;
        var t = (float)(note.NoteNumber - minNote) / (maxNote - minNote);

        var left = _spawnPoints.First();
        var right = _spawnPoints.Last();
        var interpolated = Vector3.Lerp(left.position, right.position, t);
        var closestSpawn = _spawnPoints.OrderBy(p => Vector3.Distance(p.position, interpolated)).First();
        return closestSpawn;
    }
}
