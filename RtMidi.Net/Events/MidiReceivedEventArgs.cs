namespace RtMidi.Net.Events;

/// <summary>
/// Data of <see cref="Clients.MidiInputClient.OnMessageReceived"/>.
/// </summary>
public class MidiMessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// The decoded message.
    /// </summary>
    public MidiMessage Message { get; }

    /// <summary>
    /// Delta-time since the previous message, as reported by RtMidi.
    /// </summary>
    public TimeSpan Timestamp { get; }

    /// <summary>
    /// Creates the event data.
    /// </summary>
    public MidiMessageReceivedEventArgs(MidiMessage message, TimeSpan timestamp)
    {
        Message = message;
        Timestamp = timestamp;
    }
}
