namespace RtMidi.Net.Enums;

/// <summary>
/// Types of messages indicated in
/// https://www.midi.org/specifications-old/item/table-1-summary-of-midi-message
/// and
/// https://www.midi.org/specifications-old/item/table-2-expanded-messages-list-status-bytes
/// </summary>
public enum MidiMessageType : byte
{
    Unknown = 0,

    NoteOff = 0b_1000_0000,
    NoteOn = 0b_1001_0000,
    PolyphonicKeyPressure = 0b_1010_0000,
    ControlChange = 0b_1011_0000,
    ProgramChange = 0b_1100_0000,
    ChannelPressure = 0b_1101_0000,
    PitchBendChange = 0b_1110_0000,

    //System Common Messages 
    SystemExclusive = 0b_1111_0000,
    TimeCodeQuarterFrame = 0b_1111_0001,
    SongPositionPointer = 0b_1111_0010,
    SongSelect = 0b_1111_0011,
    UndefinedReserved1 = 0b_1111_0100,
    UndefinedReserved2 = 0b_1111_0101,
    TuneRequest = 0b_1111_0110,
    EndOfSysEx = 0b_11110111,

    //Real time
    TimingClock = 0b_1111_1000,
    UndefinedReserved3 = 0b_1111_1001,
    Start = 0b_1111_1010,
    Continue = 0b_1111_1011,
    Stop = 0b_1111_1100,
    UndefinedReserved4 = 0b_1111_1101,
    ActiveSensing = 0b_1111_1110,
    SystemReset = 0b_1111_1111,
}