namespace RtMidi.Net;

/// <summary>
/// Types of messages indicated in
/// https://www.midi.org/specifications-old/item/table-1-summary-of-midi-message
/// and
/// https://www.midi.org/specifications-old/item/table-2-expanded-messages-list-status-bytes
/// </summary>
public enum MidiMessageType : byte
{
    /// <summary>Not a known message.</summary>
    Unknown = 0,

    /// <summary>Note Off.</summary>

    NoteOff = 0b_1000_0000,
    /// <summary>Note On.</summary>
    NoteOn = 0b_1001_0000,
    /// <summary>Polyphonic Key Pressure (aftertouch).</summary>
    PolyphonicKeyPressure = 0b_1010_0000,
    /// <summary>Control Change.</summary>
    ControlChange = 0b_1011_0000,
    /// <summary>Program Change.</summary>
    ProgramChange = 0b_1100_0000,
    /// <summary>Channel Pressure (aftertouch).</summary>
    ChannelPressure = 0b_1101_0000,
    /// <summary>Pitch Bend Change.</summary>
    PitchBendChange = 0b_1110_0000,

    //System Common Messages 
    /// <summary>System Exclusive (SysEx).</summary>
    SystemExclusive = 0b_1111_0000,
    /// <summary>MIDI Time Code Quarter Frame.</summary>
    TimeCodeQuarterFrame = 0b_1111_0001,
    /// <summary>Song Position Pointer.</summary>
    SongPositionPointer = 0b_1111_0010,
    /// <summary>Song Select.</summary>
    SongSelect = 0b_1111_0011,
    /// <summary>Undefined (0xF4).</summary>
    UndefinedReserved1 = 0b_1111_0100,
    /// <summary>Undefined (0xF5).</summary>
    UndefinedReserved2 = 0b_1111_0101,
    /// <summary>Tune Request.</summary>
    TuneRequest = 0b_1111_0110,
    /// <summary>End of System Exclusive.</summary>
    EndOfSysEx = 0b_11110111,

    //Real time
    /// <summary>Timing Clock.</summary>
    TimingClock = 0b_1111_1000,
    /// <summary>Undefined (0xF9).</summary>
    UndefinedReserved3 = 0b_1111_1001,
    /// <summary>Start.</summary>
    Start = 0b_1111_1010,
    /// <summary>Continue.</summary>
    Continue = 0b_1111_1011,
    /// <summary>Stop.</summary>
    Stop = 0b_1111_1100,
    /// <summary>Undefined (0xFD).</summary>
    UndefinedReserved4 = 0b_1111_1101,
    /// <summary>Active Sensing.</summary>
    ActiveSensing = 0b_1111_1110,
    /// <summary>System Reset.</summary>
    SystemReset = 0b_1111_1111,
}