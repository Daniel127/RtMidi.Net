namespace RtMidi.Net;

/// <summary>
/// The 47 percussion sounds of General MIDI Level 1 (GM 1 Percussion Key Map): the MIDI note number that plays
/// each one on <see cref="GeneralMidi.PercussionChannel"/> (channel 10).
/// </summary>
/// <remarks>
/// Each name is the official GM 1 Sound Set name. A note is not always a drum, so <see cref="MidiNote"/> stays a
/// number: <c>new MidiNote((byte)GeneralMidiPercussion.AcousticSnare)</c> or <c>(GeneralMidiPercussion)note.Number</c>.
/// GM 1 devices only support these notes, 35 to 81, on the percussion channel.
/// </remarks>
public enum GeneralMidiPercussion : byte
{
    /// <summary>Note 35: Acoustic Bass Drum.</summary>
    AcousticBassDrum = 35,
    /// <summary>Note 36: Bass Drum 1.</summary>
    BassDrum1 = 36,
    /// <summary>Note 37: Side Stick.</summary>
    SideStick = 37,
    /// <summary>Note 38: Acoustic Snare.</summary>
    AcousticSnare = 38,
    /// <summary>Note 39: Hand Clap.</summary>
    HandClap = 39,
    /// <summary>Note 40: Electric Snare.</summary>
    ElectricSnare = 40,
    /// <summary>Note 41: Low Floor Tom.</summary>
    LowFloorTom = 41,
    /// <summary>Note 42: Closed Hi Hat.</summary>
    ClosedHiHat = 42,
    /// <summary>Note 43: High Floor Tom.</summary>
    HighFloorTom = 43,
    /// <summary>Note 44: Pedal Hi-Hat.</summary>
    PedalHiHat = 44,
    /// <summary>Note 45: Low Tom.</summary>
    LowTom = 45,
    /// <summary>Note 46: Open Hi-Hat.</summary>
    OpenHiHat = 46,
    /// <summary>Note 47: Low-Mid Tom.</summary>
    LowMidTom = 47,
    /// <summary>Note 48: Hi-Mid Tom.</summary>
    HiMidTom = 48,
    /// <summary>Note 49: Crash Cymbal 1.</summary>
    CrashCymbal1 = 49,
    /// <summary>Note 50: High Tom.</summary>
    HighTom = 50,
    /// <summary>Note 51: Ride Cymbal 1.</summary>
    RideCymbal1 = 51,
    /// <summary>Note 52: Chinese Cymbal.</summary>
    ChineseCymbal = 52,
    /// <summary>Note 53: Ride Bell.</summary>
    RideBell = 53,
    /// <summary>Note 54: Tambourine.</summary>
    Tambourine = 54,
    /// <summary>Note 55: Splash Cymbal.</summary>
    SplashCymbal = 55,
    /// <summary>Note 56: Cowbell.</summary>
    Cowbell = 56,
    /// <summary>Note 57: Crash Cymbal 2.</summary>
    CrashCymbal2 = 57,
    /// <summary>Note 58: Vibraslap.</summary>
    Vibraslap = 58,
    /// <summary>Note 59: Ride Cymbal 2.</summary>
    RideCymbal2 = 59,
    /// <summary>Note 60: Hi Bongo.</summary>
    HiBongo = 60,
    /// <summary>Note 61: Low Bongo.</summary>
    LowBongo = 61,
    /// <summary>Note 62: Mute Hi Conga.</summary>
    MuteHiConga = 62,
    /// <summary>Note 63: Open Hi Conga.</summary>
    OpenHiConga = 63,
    /// <summary>Note 64: Low Conga.</summary>
    LowConga = 64,
    /// <summary>Note 65: High Timbale.</summary>
    HighTimbale = 65,
    /// <summary>Note 66: Low Timbale.</summary>
    LowTimbale = 66,
    /// <summary>Note 67: High Agogo.</summary>
    HighAgogo = 67,
    /// <summary>Note 68: Low Agogo.</summary>
    LowAgogo = 68,
    /// <summary>Note 69: Cabasa.</summary>
    Cabasa = 69,
    /// <summary>Note 70: Maracas.</summary>
    Maracas = 70,
    /// <summary>Note 71: Short Whistle.</summary>
    ShortWhistle = 71,
    /// <summary>Note 72: Long Whistle.</summary>
    LongWhistle = 72,
    /// <summary>Note 73: Short Guiro.</summary>
    ShortGuiro = 73,
    /// <summary>Note 74: Long Guiro.</summary>
    LongGuiro = 74,
    /// <summary>Note 75: Claves.</summary>
    Claves = 75,
    /// <summary>Note 76: Hi Wood Block.</summary>
    HiWoodBlock = 76,
    /// <summary>Note 77: Low Wood Block.</summary>
    LowWoodBlock = 77,
    /// <summary>Note 78: Mute Cuica.</summary>
    MuteCuica = 78,
    /// <summary>Note 79: Open Cuica.</summary>
    OpenCuica = 79,
    /// <summary>Note 80: Mute Triangle.</summary>
    MuteTriangle = 80,
    /// <summary>Note 81: Open Triangle.</summary>
    OpenTriangle = 81,
}
