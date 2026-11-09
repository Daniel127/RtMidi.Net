namespace RtMidi.Net;

/// <summary>
/// The 16 instrument families of General MIDI Level 1. Each family has 8 programs of <see cref="GeneralMidiProgram"/>,
/// in this order; see <see cref="GeneralMidi.GetFamily"/>.
/// </summary>
public enum GeneralMidiFamily : byte
{
    /// <summary>Piano: programs 1 to 8 in the GM 1 table (values 0 to 7).</summary>
    Piano = 0,
    /// <summary>Chromatic Percussion: programs 9 to 16 in the GM 1 table (values 8 to 15).</summary>
    ChromaticPercussion = 1,
    /// <summary>Organ: programs 17 to 24 in the GM 1 table (values 16 to 23).</summary>
    Organ = 2,
    /// <summary>Guitar: programs 25 to 32 in the GM 1 table (values 24 to 31).</summary>
    Guitar = 3,
    /// <summary>Bass: programs 33 to 40 in the GM 1 table (values 32 to 39).</summary>
    Bass = 4,
    /// <summary>Strings: programs 41 to 48 in the GM 1 table (values 40 to 47).</summary>
    Strings = 5,
    /// <summary>Ensemble: programs 49 to 56 in the GM 1 table (values 48 to 55).</summary>
    Ensemble = 6,
    /// <summary>Brass: programs 57 to 64 in the GM 1 table (values 56 to 63).</summary>
    Brass = 7,
    /// <summary>Reed: programs 65 to 72 in the GM 1 table (values 64 to 71).</summary>
    Reed = 8,
    /// <summary>Pipe: programs 73 to 80 in the GM 1 table (values 72 to 79).</summary>
    Pipe = 9,
    /// <summary>Synth Lead: programs 81 to 88 in the GM 1 table (values 80 to 87).</summary>
    SynthLead = 10,
    /// <summary>Synth Pad: programs 89 to 96 in the GM 1 table (values 88 to 95).</summary>
    SynthPad = 11,
    /// <summary>Synth Effects: programs 97 to 104 in the GM 1 table (values 96 to 103).</summary>
    SynthEffects = 12,
    /// <summary>Ethnic: programs 105 to 112 in the GM 1 table (values 104 to 111).</summary>
    Ethnic = 13,
    /// <summary>Percussive: programs 113 to 120 in the GM 1 table (values 112 to 119).</summary>
    Percussive = 14,
    /// <summary>Sound Effects: programs 121 to 128 in the GM 1 table (values 120 to 127).</summary>
    SoundEffects = 15,
}
