using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Unity.VisualScripting;
using UnityEngine.Serialization;


public class BlockSpawner : MonoBehaviour
{
    public static BlockSpawner Instance { get; private set; }
    
    [Header("Spawn Settings")]
    [SerializeField] private Material blockMaterial;
    [SerializeField] private string midiFile;
    [SerializeField] private GameObject block;
    [SerializeField] private float blockSpeed;
    [SerializeField] private GameObject _goal;
    
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
        _spawnPoints = NoteLines.Instance.SpawnPoints;
        _spawnPoints = _spawnPoints.OrderBy(p => p.position.y).ToList();
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
        
        var startDspTime = AudioSettings.dspTime;
        StartCoroutine(WaitAndPlay(fallTime));
        StartCoroutine(SpawnNotesWithTiming(p1Notes, _goal,startDspTime, fallTime));


    }
    
    private IEnumerator SpawnNotesWithTiming(List<Note> notes, GameObject goal, double startTime, float fallTime)
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
            SpawnMidiNote(block, note, notes, goal, dist);
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

    
    void SpawnMidiNote(GameObject noteObject, Note note, List<Note> notes, GameObject goal, float distance)
    {
        var closest = GetClosestSpawnPoint(note, notes);
        
        var spawnPosition = new Vector3(closest.position.x + distance, closest.transform.position.y, closest.position.z);
        
        var spawnedNote = Instantiate(noteObject, spawnPosition, Quaternion.identity);
        spawnedNote.transform.SetParent(closest.transform, true);        
        spawnedNote.GetComponent<Renderer>().material = goal.GetComponent<Renderer>().material;
        spawnedNote.GetComponent<MovingBlock>().speed = blockSpeed;
        spawnedNote.GetComponent<MovingBlock>().goal = closest.position;
        spawnedNote.GetComponent<MovingBlock>().Key = 1;
        if (spawnedNote.GetComponent<LongBlock>() != null)
        {
            spawnedNote.GetComponent<LongBlock>().length = note.Length;
        }
    }

    private Transform GetClosestSpawnPoint(Note note, List<Note> notes)
    {
        var minNote = _midiPlayer.GetMinNote(notes).NoteNumber;
        var maxNote = _midiPlayer.GetMaxNote(notes).NoteNumber;
        var t = (float)(note.NoteNumber - minNote) / (maxNote - minNote);
        var index = Mathf.Clamp(Mathf.RoundToInt(t * (_spawnPoints.Count - 1)), 0, _spawnPoints.Count - 1);
        return _spawnPoints[index];

    }
}
