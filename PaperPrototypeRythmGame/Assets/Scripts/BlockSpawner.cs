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
using Random = UnityEngine.Random;


public class BlockSpawner : MonoBehaviour
{
    public static BlockSpawner Instance { get; private set; }
    
    [Header("Spawn Settings")]
    [SerializeField] private string midiFile;
    [SerializeField] private GameObject block;
    [SerializeField] public float blockSpeed;
    [SerializeField] private float prepTime;
    
    [HideInInspector] public MidiPlayer _midiPlayer;
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
        var p1Notes = _midiPlayer.GetNotesOfChannel(1);
        SpawnAllNotes(p1Notes);
        //_midiPlayer.StartPlayback();
    }
    


    private void SpawnAllNotes(List<Note> notes)
    {
        var tempoMap = _midiPlayer.MidiFile.GetTempoMap();
        foreach (var note in notes)
        {
            var metricTime = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap).TotalSeconds;
            SpawnMidiNote(block, note, notes, (float)metricTime * blockSpeed + 20);
        }
    }

    


    public void DropMyNeedle()
    {
        StartCoroutine(_midiPlayer.StartPlayback());
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
    


    
    void SpawnMidiNote(GameObject noteObject, Note note, List<Note> notes, float distance)
    {
        var closest = GetClosestSpawnPoint(note, notes);
        
        var spawnPosition = new Vector3(distance, closest.transform.position.y, closest.position.z);
        
        var spawnedNote = Instantiate(noteObject, spawnPosition, Quaternion.identity);
        spawnedNote.transform.SetParent(closest.transform, true);        
        spawnedNote.GetComponent<MovingBlock>().speed = blockSpeed;
        spawnedNote.GetComponent<MovingBlock>().goal = closest.position;
        spawnedNote.GetComponent<MovingBlock>().Key = Random.Range(1,4);
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

    private float GetDistance(float seconds)
    {
        return seconds * blockSpeed;
    }
}
