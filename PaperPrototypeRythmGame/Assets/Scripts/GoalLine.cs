using System;
using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

public class GoalLine : MonoBehaviour
{
    [SerializeField] private GoalScript goalScript;

    private void Update()
    {
        if (goalScript.isGoing)
            return;

        var cubeThingie = goalScript.currentBlock;
        if (cubeThingie == null)
            return;
        
        var midiPlayer = BlockSpawner.Instance._midiPlayer;
        var tempoMap = midiPlayer.MidiFile.GetTempoMap();
        var notes = midiPlayer.Notes;
        var firstNote = notes[0];
        var buffer = 5f;
        
        var noteInSeconds = (float)TimeConverter.ConvertTo<MetricTimeSpan>(firstNote.Time, tempoMap).TotalSeconds + buffer;
        float d;
        if (noteInSeconds != 0)
            d = BlockSpawner.Instance.blockSpeed / noteInSeconds;
        else
            d = 0;
        
        if ((transform.position.x - cubeThingie.transform.position.x) <= d)
            BlockSpawner.Instance.DropMyNeedle();
        
    }

    private void OnTriggerExit(Collider other)
    {
        goalScript.blocks.Remove(other.gameObject);
        Destroy(other.gameObject);
        goalScript.UpdateBlock();
        goalScript.isGoing = true;
    }
}
