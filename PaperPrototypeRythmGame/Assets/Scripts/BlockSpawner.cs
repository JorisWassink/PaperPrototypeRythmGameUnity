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

    [HideInInspector] public List<Transform> allGoals; // Assign all goal points (unsorted) in Inspector
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
        var fallTime = 3f;
        var startDspTime = AudioSettings.dspTime;
        StartCoroutine(WaitAndPlay(fallTime));
        StartCoroutine(SpawnNotesWithTiming(startDspTime, fallTime));

    }
    
    private IEnumerator SpawnNotesWithTiming(double startTime, float fallTime)
    {
        MidiFile file = _midiPlayer.MidiFile;
        var tempoMap = file.GetTempoMap();

        foreach (var note in _midiPlayer.Notes)
        {
            var noteTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap);
            var noteTimeInSeconds = noteTimeSpan.TotalSeconds;
            var noteSpawnTime = startTime + noteTimeInSeconds;

            while (AudioSettings.dspTime < noteSpawnTime)
                yield return null;


            var dist = EstimateRequiredDistance(note, fallTime);
            SpawnMidiNote(block, note, dist);
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
    
    
    float EstimateRequiredDistance(Note note,  float desiredFallTime)
    {
        var speed = blockSpeed;
        var distance = desiredFallTime * speed;
        return distance;
    }

    
    void SpawnMidiNote(GameObject block, Note note, float height)
    {
        var closest = GetClosestSpawnPoint(note);
        var goal = GetGoalForSpawner(closest);
        
        var spawnPosition = new Vector3(closest.position.x, goal.position.y + height, closest.position.z);
        
        var spawnedNote = Instantiate(block, spawnPosition, Quaternion.identity);
        spawnedNote.transform.SetParent(closest.transform, true);        
        spawnedNote.GetComponent<Renderer>().material = closest.GetComponent<Renderer>().material;
        spawnedNote.GetComponent<MovingBlock>().speed = blockSpeed;
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
    
    public Transform GetGoalForSpawner(Transform spawner)
    {
        int index = _spawnPoints.IndexOf(spawner);
        if (index >= 0 && index < goalPoints.Count)
        {
            return goalPoints[index];
        }
        Debug.LogWarning("Spawner not found or goal not assigned.");
        return null;
    }

}
