using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using UnityEngine.Serialization;


public class BlockSpawner : MonoBehaviour
{
    public static BlockSpawner Instance { get; private set; }
    
    [Header("Spawn Settings")]
    [SerializeField] private Material blockMaterial;
    [SerializeField] private string midiFile;
    [SerializeField] private GameObject block;
    [SerializeField] private float blockSpeed;
    [SerializeField] private GameObject p1Goals;
    [SerializeField] private GameObject p2Goals;
    [SerializeField] private GameObject sharedGoals;
    
    private MidiPlayer _midiPlayer;
    private List<Transform> _spawnPoints;
    
    private void Awake()
    {
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

        var p1Notes = _midiPlayer.GetNotesOfChannel(1);
        var p2Notes = _midiPlayer.GetNotesOfChannel(5);
        var sharedNotes = _midiPlayer.GetNotesOfChannel(6);
        
        var p1Goals = this.p1Goals.GetComponentsInChildren<GoalScript>(true)
            .Select(t => t.gameObject)
            .ToList();
        var p2Goals = this.p2Goals.GetComponentsInChildren<GoalScript>(true)
            .Select(t => t.gameObject)
            .ToList();
        
        var sharedGoals = this.sharedGoals.GetComponentsInChildren<GoalScript>(true)
            .Select(t => t.gameObject)
            .ToList();

        
        var startDspTime = AudioSettings.dspTime;
        StartCoroutine(WaitAndPlay(fallTime));
        StartCoroutine(SpawnNotesWithTiming(p1Notes, p1Goals,startDspTime, fallTime));
        StartCoroutine(SpawnNotesWithTiming(p2Notes, p2Goals,startDspTime, fallTime));
        StartCoroutine(SpawnNotesWithTiming(sharedNotes, sharedGoals,startDspTime, fallTime));

    }
    
    private IEnumerator SpawnNotesWithTiming(List<Note> notes, List<GameObject> goals, double startTime, float fallTime)
    {
        MidiFile file = _midiPlayer.MidiFile;
        var tempoMap = file.GetTempoMap();

        foreach (var note in notes)
        {
            var noteTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap);
            var noteTimeInSeconds = noteTimeSpan.TotalSeconds;
            var noteSpawnTime = startTime + noteTimeInSeconds;

            while (AudioSettings.dspTime < noteSpawnTime)
                yield return null;


            var dist = EstimateRequiredDistance(note, fallTime);
            SpawnMidiNote(block, notes, note, goals, dist);
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

    
    void SpawnMidiNote(GameObject block, List<Note> notes, Note note, List<GameObject> goals, float height)
    {
        var goal = GetClosestGoal(note, notes, goals);

        var closest = goal.spawnPoint;
        
        var spawnPosition = new Vector3(closest.position.x, goal.transform.position.y + height, closest.position.z);
        
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

    private GoalScript GetClosestGoal(Note note, List<Note> notes, List<GameObject> goals)
    {
        var minNote = _midiPlayer.GetMinNote(notes).NoteNumber;
        var maxNote = _midiPlayer.GetMaxNote(notes).NoteNumber;
        var t = (float)(note.NoteNumber - minNote) / (maxNote - minNote);
        
        var left = goals.OrderBy(g => g.transform.position.x).First();
        var right = goals.OrderBy(g => g.transform.position.x).Last();
        var interpolated = Vector3.Lerp(left.transform.position, right.transform.position, t);
        var closestSpawn = goals.OrderBy(p => Vector3.Distance(p.transform.position, interpolated)).First();
        return closestSpawn.GetComponent<GoalScript>();
    }

}
