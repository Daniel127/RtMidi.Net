namespace RtMidi.Net.Events;

/// <summary>
/// Data of <see cref="Clients.MidiInputClient.MessageReceived"/>.
/// </summary>
public sealed class MidiMessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// The decoded message.
    /// </summary>
    public MidiMessage Message { get; }

    /// <summary>
    /// Delta-time since the previous message, as reported by RtMidi (zero for the first message).
    /// </summary>
    public TimeSpan Timestamp { get; }

    /// <summary>
    /// Creates the event data.
    /// </summary>
    /// <param name="message">The decoded message.</param>
    /// <param name="timestamp">Delta-time since the previous message.</param>
    public MidiMessageReceivedEventArgs(MidiMessage message, TimeSpan timestamp)
    {
        Message = message;
        Timestamp = timestamp;
    }
}
