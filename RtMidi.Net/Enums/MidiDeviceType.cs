namespace RtMidi.Net.Enums;

/// <summary>
/// Direction of a MIDI port.
/// </summary>
public enum MidiDeviceType : byte
{
    /// <summary>A port that receives messages.</summary>
    Input,
    /// <summary>A port that sends messages.</summary>
    Output,
}