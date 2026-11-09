using RtMidi.Net.Enums;

namespace RtMidi.Net.Tests;

public class MidiNoteTests
{
    [Theory]
    [InlineData(0, MusicNote.C, MusicOctave.OctaveMinus1)]
    [InlineData(11, MusicNote.B, MusicOctave.OctaveMinus1)]
    [InlineData(12, MusicNote.C, MusicOctave.Octave0)]
    [InlineData(21, MusicNote.A, MusicOctave.Octave0)]   // lowest piano key, A0
    [InlineData(59, MusicNote.B, MusicOctave.Octave3)]
    [InlineData(60, MusicNote.C, MusicOctave.Octave4)]   // middle C, C4
    [InlineData(61, MusicNote.CSharp, MusicOctave.Octave4)]
    [InlineData(69, MusicNote.A, MusicOctave.Octave4)]   // A440, A4
    [InlineData(108, MusicNote.C, MusicOctave.Octave8)]  // highest piano key, C8
    [InlineData(127, MusicNote.G, MusicOctave.Octave9)]
    public void FromByte_SplitsNoteAndOctave(byte number, MusicNote note, MusicOctave octave)
    {
        var midiNote = new MidiNote(number);

        Assert.Equal(note, midiNote.Note);
        Assert.Equal(octave, midiNote.Octave);
    }

    [Fact]
    public void AllNoteNumbers_RoundTrip()
    {
        for (byte number = 0; number <= 127; number++)
        {
            var midiNote = new MidiNote(number);
            Assert.Equal(number, midiNote.GetByteRepresentation());
            Assert.Equal(number, new MidiNote(midiNote.Note, midiNote.Octave).GetByteRepresentation());
        }
    }

    [Theory]
    [InlineData(128)]
    [InlineData(255)]
    public void FromByte_AboveRange_Throws(byte number)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MidiNote(number));
    }

    [Theory]
    [InlineData(MusicNote.GSharp, MusicOctave.Octave9)]
    [InlineData(MusicNote.B, MusicOctave.Octave9)]
    [InlineData((MusicNote)12, MusicOctave.Octave0)]
    public void FromNoteAndOctave_OutOfRange_Throws(MusicNote note, MusicOctave octave)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MidiNote(note, octave));
    }

    [Fact]
    public void LatinNotes_AreTheSameAsEnglishNotes()
    {
        Assert.Equal(new MidiNote(MusicNote.C, MusicOctave.Octave4), new MidiNote(MusicNote.Do, MusicOctave.Octave4));
        Assert.Equal(new MidiNote(MusicNote.ASharp, MusicOctave.Octave2), new MidiNote(MusicNote.LaSharp, MusicOctave.Octave2));
    }

    [Theory]
    [InlineData(MusicNote.C, "C", "Do")]
    [InlineData(MusicNote.CSharp, "C#", "Do#")]
    [InlineData(MusicNote.D, "D", "Re")]
    [InlineData(MusicNote.DSharp, "D#", "Re#")]
    [InlineData(MusicNote.E, "E", "Mi")]
    [InlineData(MusicNote.F, "F", "Fa")]
    [InlineData(MusicNote.FSharp, "F#", "Fa#")]
    [InlineData(MusicNote.G, "G", "Sol")]
    [InlineData(MusicNote.GSharp, "G#", "Sol#")]
    [InlineData(MusicNote.A, "A", "La")]
    [InlineData(MusicNote.ASharp, "A#", "La#")]
    [InlineData(MusicNote.B, "B", "Si")]
    public void GetName_ReturnsEnglishOrLatinNameWithOctave(MusicNote note, string english, string latin)
    {
        var midiNote = new MidiNote(note, MusicOctave.Octave4);

        Assert.Equal($"{english}4", midiNote.GetName());
        Assert.Equal($"{latin}4", midiNote.GetName(useLatinName: true));
        Assert.Equal($"{english}4", midiNote.ToString());
    }

    [Theory]
    [InlineData(0, "C-1")]
    [InlineData(11, "B-1")]
    [InlineData(12, "C0")]
    [InlineData(21, "A0")]
    [InlineData(59, "B3")]
    [InlineData(60, "C4")]
    [InlineData(69, "A4")]
    [InlineData(108, "C8")]
    [InlineData(127, "G9")]
    public void GetName_UsesScientificPitchNotation(byte number, string name)
    {
        Assert.Equal(name, new MidiNote(number).GetName());
    }

    [Fact]
    public void MusicOctave_IsTheScientificOctave()
    {
        Assert.Equal(new MidiNote(60), new MidiNote(MusicNote.C, MusicOctave.Octave4));
        Assert.Equal(new MidiNote(69), new MidiNote(MusicNote.A, MusicOctave.Octave4));
        Assert.Equal(new MidiNote(0), new MidiNote(MusicNote.C, MusicOctave.OctaveMinus1));
        Assert.Equal(new MidiNote(127), new MidiNote(MusicNote.G, MusicOctave.Octave9));
    }

    /// <summary>
    /// Independent check of the octave numbering: in scientific pitch notation an octave starts at
    /// C and A4 is 440 Hz; MIDI note n sounds at 440 * 2^((n - 69) / 12) Hz (MIDI Tuning Standard).
    /// So every C must be 261.63 Hz * 2^(octave - 4), and every A must be 440 Hz * 2^(octave - 4).
    /// </summary>
    [Fact]
    public void ScientificOctave_MatchesFrequencies()
    {
        for (byte number = 0; number <= 127; number++)
        {
            var note = new MidiNote(number);
            var frequency = 440.0 * Math.Pow(2, (number - 69) / 12.0);
            var name = note.GetName();
            var octave = int.Parse(name[name.TrimEnd("-0123456789".ToCharArray()).Length..]);
            var expectedFrequency = 440.0 * Math.Pow(2, octave - 4 + ((int)note.Note - 9) / 12.0);

            Assert.Equal(expectedFrequency, frequency, 6);
        }
    }

    [Fact]
    public void PianoKeyboard_IsA0ToC8()
    {
        // An 88-key piano spans MIDI notes 21 to 108.
        Assert.Equal("A0", new MidiNote(21).GetName());
        Assert.Equal("C8", new MidiNote(108).GetName());
        Assert.Equal(88, 108 - 21 + 1);
    }

    [Theory]
    [InlineData(60, 4, "C4", "Do4")]
    [InlineData(60, 3, "C3", "Do3")]
    [InlineData(60, 5, "C5", "Do5")]
    [InlineData(0, 3, "C-2", "Do-2")]
    [InlineData(0, 5, "C0", "Do0")]
    [InlineData(127, 3, "G8", "Sol8")]
    [InlineData(127, 5, "G10", "Sol10")]
    public void GetName_MiddleCOctave_OnlyShiftsTheNumber(byte number, int middleCOctave, string english, string latin)
    {
        var note = new MidiNote(number);

        Assert.Equal(english, note.GetName(middleCOctave: middleCOctave));
        Assert.Equal(latin, note.GetName(useLatinName: true, middleCOctave: middleCOctave));
        Assert.Equal(number, note.Number);
    }

    [Fact]
    public void Number_IsTheMidiNote()
    {
        for (byte number = 0; number <= 127; number++)
            Assert.Equal(number, new MidiNote(number).Number);
    }

    [Theory]
    [InlineData("C4", 60)]
    [InlineData("c4", 60)]
    [InlineData(" C4 ", 60)]
    [InlineData("C#4", 61)]
    [InlineData("C♯4", 61)]
    [InlineData("Db4", 61)]
    [InlineData("D♭4", 61)]
    [InlineData("db4", 61)]
    [InlineData("A4", 69)]
    [InlineData("Bb3", 58)]
    [InlineData("bb3", 58)]
    [InlineData("B3", 59)]
    [InlineData("Cb4", 59)]   // a flat crosses down to the previous octave
    [InlineData("B#3", 60)]   // a sharp crosses up to the next octave
    [InlineData("E#4", 65)]
    [InlineData("C##4", 62)]
    [InlineData("C-1", 0)]
    [InlineData("C#-1", 1)]
    [InlineData("G9", 127)]
    [InlineData("A0", 21)]
    [InlineData("C8", 108)]
    [InlineData("Do4", 60)]
    [InlineData("do4", 60)]
    [InlineData("Do#4", 61)]
    [InlineData("Reb4", 61)]
    [InlineData("Re4", 62)]
    [InlineData("Mi4", 64)]
    [InlineData("Fa4", 65)]
    [InlineData("Fa#4", 66)]
    [InlineData("Sol4", 67)]
    [InlineData("SOL4", 67)]
    [InlineData("Sol#4", 68)]
    [InlineData("La4", 69)]
    [InlineData("Sib3", 70 - 12)]
    [InlineData("Si3", 59)]
    public void Parse_ScientificNames(string name, byte number)
    {
        Assert.Equal(number, MidiNote.Parse(name).Number);
        Assert.True(MidiNote.TryParse(name, out var note));
        Assert.Equal(number, note.Number);
    }

    [Theory]
    [InlineData("C3", 3, 60)]
    [InlineData("Do3", 3, 60)]
    [InlineData("C5", 5, 60)]
    [InlineData("C-2", 3, 0)]
    [InlineData("G8", 3, 127)]
    [InlineData("C0", 5, 0)]
    [InlineData("G10", 5, 127)]
    public void Parse_MiddleCOctave(string name, int middleCOctave, byte number)
    {
        Assert.Equal(number, MidiNote.Parse(name, middleCOctave).Number);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void GetName_Parse_RoundTrip(int middleCOctave)
    {
        for (byte number = 0; number <= 127; number++)
        {
            var note = new MidiNote(number);
            Assert.Equal(note, MidiNote.Parse(note.GetName(middleCOctave: middleCOctave), middleCOctave));
            Assert.Equal(note, MidiNote.Parse(note.GetName(useLatinName: true, middleCOctave: middleCOctave), middleCOctave));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("C")]
    [InlineData("4")]
    [InlineData("H4")]
    [InlineData("C 4")]
    [InlineData("C+4")]
    [InlineData("C--1")]
    [InlineData("C4.5")]
    [InlineData("C4x")]
    [InlineData("C-")]
    [InlineData("Cb-1")]   // below MIDI note 0
    [InlineData("C-2")]
    [InlineData("G#9")]    // above MIDI note 127
    [InlineData("C10")]
    [InlineData("C99999999999")]   // too large for an int
    [InlineData("C-99999999999")]
    [InlineData("C2147483647")]    // int.MaxValue: no overflow, just out of range
    [InlineData("Ut4")]
    public void Parse_Invalid_Throws(string name)
    {
        Assert.Throws<FormatException>(() => MidiNote.Parse(name));
        Assert.False(MidiNote.TryParse(name, out _));
    }

    [Fact]
    public void Parse_Null()
    {
        Assert.Throws<ArgumentNullException>(() => MidiNote.Parse(null!));
        Assert.False(MidiNote.TryParse(null, out _));
    }

    [Theory]
    [InlineData(MidiNote.MinMiddleCOctave - 1)]
    [InlineData(MidiNote.MaxMiddleCOctave + 1)]
    [InlineData(1000)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void MiddleCOctave_OutOfRange_Throws(int middleCOctave)
    {
        var note = new MidiNote(60);
        Assert.Throws<ArgumentOutOfRangeException>(() => note.GetName(middleCOctave: middleCOctave));
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiNote.Parse("C4", middleCOctave));
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiNote.TryParse("C4", out _, middleCOctave));
    }

    [Fact]
    public void Parse_LeadingZeros()
    {
        Assert.Equal(60, MidiNote.Parse("C04").Number);
        Assert.Equal(0, MidiNote.Parse("C-01").Number);
        Assert.Equal(60, MidiNote.Parse("C0004").Number);
        Assert.Equal(0, MidiNote.Parse("C-0001").Number);
        Assert.Equal(60, MidiNote.Parse("C" + new string('0', 50) + "4").Number);
    }

    [Fact]
    public void GetName_Parse_RoundTrip_EveryAllowedMiddleCOctave()
    {
        for (var middleCOctave = MidiNote.MinMiddleCOctave; middleCOctave <= MidiNote.MaxMiddleCOctave; middleCOctave++)
        {
            for (byte number = 0; number <= 127; number++)
            {
                var note = new MidiNote(number);
                Assert.Equal(note, MidiNote.Parse(note.GetName(middleCOctave: middleCOctave), middleCOctave));
                Assert.Equal(note, MidiNote.Parse(note.GetName(useLatinName: true, middleCOctave: middleCOctave), middleCOctave));
            }
        }
    }
}
