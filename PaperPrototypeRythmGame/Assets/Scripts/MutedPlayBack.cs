using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;

public class MutedPlayback : Playback
{
    public MutedPlayback(IEnumerable<ITimedObject> timedObjects, TempoMap tempoMap)
        : base(timedObjects, tempoMap)
    {
    }

    protected override bool TryPlayEvent(MidiEvent midiEvent, object metadata)
    {
        // Prevent the event from being sent to the output device
        return false;
    }
}