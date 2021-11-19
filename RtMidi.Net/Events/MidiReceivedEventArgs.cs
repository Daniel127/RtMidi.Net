namespace RtMidi.Net.Events;

public class MidiMessageReceivedEventArgs : EventArgs
{
    public MidiMessage Message { get; }
    public TimeSpan Timestamp { get; }

    public MidiMessageReceivedEventArgs(MidiMessage message, TimeSpan timestamp)
    {
        Message = message;
        Timestamp = timestamp;
    }
}