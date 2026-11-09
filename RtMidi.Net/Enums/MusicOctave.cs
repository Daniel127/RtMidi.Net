namespace RtMidi.Net.Enums;

/// <summary>
/// Octave of a MIDI note in scientific pitch notation (ISO): middle C (MIDI note 60) is C in
/// <see cref="Octave4"/>, A440 (MIDI note 69) is A in <see cref="Octave4"/>, MIDI note 0 is C in
/// <see cref="OctaveMinus1"/> and MIDI note 127 is G in <see cref="Octave9"/>.
/// </summary>
/// <remarks>
/// The underlying value is <c>MIDI note / 12</c>, so it is the scientific octave number plus one.
/// To show names with another numbering (middle C as C3 or C5) use
/// <see cref="MidiNote.GetName(bool, int)"/> with its <c>middleCOctave</c> parameter.
/// </remarks>
public enum MusicOctave : byte
{
    /// <summary>Octave -1: MIDI notes 0 (C-1) to 11 (B-1).</summary>
    OctaveMinus1 = 0,
    /// <summary>Octave 0: MIDI notes 12 (C0) to 23 (B0).</summary>
    Octave0 = 1,
    /// <summary>Octave 1: MIDI notes 24 (C1) to 35 (B1).</summary>
    Octave1 = 2,
    /// <summary>Octave 2: MIDI notes 36 (C2) to 47 (B2).</summary>
    Octave2 = 3,
    /// <summary>Octave 3: MIDI notes 48 (C3) to 59 (B3).</summary>
    Octave3 = 4,
    /// <summary>Octave 4: MIDI notes 60 (C4) to 71 (B4). Contains middle C (60) and A440 (69).</summary>
    Octave4 = 5,
    /// <summary>Octave 5: MIDI notes 72 (C5) to 83 (B5).</summary>
    Octave5 = 6,
    /// <summary>Octave 6: MIDI notes 84 (C6) to 95 (B6).</summary>
    Octave6 = 7,
    /// <summary>Octave 7: MIDI notes 96 (C7) to 107 (B7).</summary>
    Octave7 = 8,
    /// <summary>Octave 8: MIDI notes 108 (C8) to 119 (B8).</summary>
    Octave8 = 9,
    /// <summary>Octave 9: MIDI notes 120 (C9) to 127 (G9).</summary>
    Octave9 = 10
}
