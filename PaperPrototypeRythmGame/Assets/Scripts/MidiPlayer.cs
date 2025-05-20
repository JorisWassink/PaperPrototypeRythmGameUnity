using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine;
using Note = Melanchall.DryWetMidi.Interaction.Note;

public class MidiPlayer : MonoBehaviour
{
    private const string OutputDeviceName = "Microsoft GS Wavetable Synth";

    private OutputDevice _outputDevice;
    
    public MidiFile MidiFile { get; private set; }
    public Playback Playback;
    public List<Note> Notes;
    public Note MinNote;
    public Note MaxNote;

    public event EventHandler<NotesEventArgs> NotesPlaybackStartedEvent;
    public event EventHandler<Event> StartPlaying; 
    public event EventHandler<NotesEventArgs> NotesPlaybackFinishedEvent;
    
    public void InitializePlayer(string midiFile)
    {
        MidiFile= MidiFile.Read(midiFile);
        Notes = MidiFile.GetNotes().ToList();

        List<Note> delete = new List<Note>();
        MinNote = Notes[0];
        MaxNote = Notes[Notes.Count - 1];
        foreach (var note in Notes)
        {
            if (note.Channel != 3)
                delete.Add(note);
            else
            {
                if (note.NoteNumber > MaxNote.NoteNumber)
                    MaxNote = note;
                if (note.NoteNumber < MinNote.NoteNumber)
                    MinNote = note;
            }
        }

        foreach (var note in delete)
            Notes.Remove(note);
        
        InitializeOutputDevice();
        InitializeFilePlayback(MidiFile);
    }

    private void Start()
    {
        StartPlaying?.Invoke(this, new Event());
        
    }

    public void OnQuit()
    {
        Debug.Log("Releasing playback and device...");

        if (Playback != null)
        {
            Playback.NotesPlaybackStarted -= OnNotesPlaybackStarted;
            Playback.NotesPlaybackFinished -= OnNotesPlaybackFinished;
            Playback.Dispose();
        }
        

        if (_outputDevice != null)
            _outputDevice.Dispose();

        Debug.Log("Playback and device released.");
    }


    private void InitializeOutputDevice()
    {
        Debug.Log($"Initializing output device [{OutputDeviceName}]...");

        var allOutputDevices = OutputDevice.GetAll();
        if (!allOutputDevices.Any(d => d.Name == OutputDeviceName))
        {
            var allDevicesList = string.Join(Environment.NewLine, allOutputDevices.Select(d => $"  {d.Name}"));
            Debug.Log($"There is no [{OutputDeviceName}] device presented in the system. Here the list of all device:{Environment.NewLine}{allDevicesList}");
            return;
        }

        _outputDevice = OutputDevice.GetByName(OutputDeviceName);
        Debug.Log($"Output device [{OutputDeviceName}] initialized.");
    }

    private void InitializeFilePlayback(MidiFile midiFile)
    {
        var tempoMap = midiFile.GetTempoMap();
        var timedEvents = midiFile.GetTimedEvents();
        
        Playback = new Playback(timedEvents, tempoMap, _outputDevice);
        Playback.NotesPlaybackStarted += OnNotesPlaybackStarted;
        Playback.NotesPlaybackFinished += OnNotesPlaybackFinished;
        Debug.Log("Initializing file playback (muted but active)...");
    }


    public void StartPlayback()
    {
        Playback.Start();
    }
    
    
    private void OnNotesPlaybackFinished(object sender, NotesEventArgs e)
    {
        NotesPlaybackFinishedEvent?.Invoke(sender, e);
    }

    private void OnNotesPlaybackStarted(object sender, NotesEventArgs e)
    {
        NotesPlaybackStartedEvent?.Invoke(sender, e);
    }

    private void LogNotes(string title, NotesEventArgs e)
    {
        var message = new StringBuilder()
            .AppendLine(title)
            .AppendLine(string.Join(Environment.NewLine, e.Notes.Select(n => $"  {n}")))
            .ToString();
        Debug.Log(message.Trim());
    }

}
