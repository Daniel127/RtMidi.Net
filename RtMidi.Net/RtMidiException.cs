namespace RtMidi.Net;

/// <summary>
/// Error reported by the native RtMidi library, for example when the MIDI
/// system can't be initialised or a port can't be opened.
/// </summary>
public class RtMidiException : Exception
{
    public RtMidiException(string message) : base(message)
    {
    }
}
