using RtMidi.Net.Enums;

namespace RtMidi.Net.Tests;

public class MidiNoteTests
{
    [Theory]
    [InlineData(0, MusicNote.C, MusicOctave.Octave0)]
    [InlineData(21, MusicNote.A, MusicOctave.Octave1)]
    [InlineData(60, MusicNote.C, MusicOctave.Octave5)]
    [InlineData(61, MusicNote.CSharp, MusicOctave.Octave5)]
    [InlineData(127, MusicNote.G, MusicOctave.Octave10)]
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
    [InlineData(MusicNote.GSharp, MusicOctave.Octave10)]
    [InlineData(MusicNote.B, MusicOctave.Octave10)]
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

    [Fact]
    public void GetName_HighestOctave()
    {
        Assert.Equal("G10", new MidiNote(127).GetName());
    }
}
