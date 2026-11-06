using RtMidi.Net.Enums;

namespace RtMidi.Net;

/// <summary>
/// A MIDI note number (0-127) as a note and an octave: 0 is C in octave 0, 60 is C in octave 5, 127 is G in octave 10.
/// </summary>
public readonly struct MidiNote
{
    private const byte MaxNoteNumber = 127;
    private const int NotesPerOctave = 12;

    private static readonly string[] EnglishNames = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];
    private static readonly string[] LatinNames = ["Do", "Do#", "Re", "Re#", "Mi", "Fa", "Fa#", "Sol", "Sol#", "La", "La#", "Si"];

    /// <summary>
    /// The note
    /// </summary>
    public MusicNote Note { get; }

    /// <summary>
    /// The octave
    /// </summary>
    public MusicOctave Octave { get; }

    /// <summary>
    /// Create a MidiNote
    /// </summary>
    /// <param name="note">Number in range 0 - 127</param>
    /// <exception cref="ArgumentOutOfRangeException">The number is above 127.</exception>
    public MidiNote(byte note)
    {
        if (note > MaxNoteNumber) throw new ArgumentOutOfRangeException(nameof(note), note, "The note must be 0 to 127");
        Note = (MusicNote)(note % NotesPerOctave);
        Octave = (MusicOctave)(note / NotesPerOctave);
    }

    /// <summary>
    /// Create a MidiNote
    /// </summary>
    /// <param name="note">The note</param>
    /// <param name="octave">The octave</param>
    /// <exception cref="ArgumentOutOfRangeException">The note is not a valid <see cref="MusicNote"/>,
    /// or the result is above MIDI note 127 (G in octave 10).</exception>
    public MidiNote(MusicNote note, MusicOctave octave)
    {
        if ((byte)note >= NotesPerOctave)
            throw new ArgumentOutOfRangeException(nameof(note), note, "Unknown note.");
        if ((byte)octave * NotesPerOctave + (byte)note > MaxNoteNumber)
            throw new ArgumentOutOfRangeException(nameof(octave), octave, $"{note} in {octave} is above MIDI note 127 (G in Octave10).");
        Note = note;
        Octave = octave;
    }

    /// <summary>
    /// Get Byte for MIDI message
    /// </summary>
    /// <returns>The MIDI byte representation</returns>
    public byte GetByteRepresentation()
    {
        return (byte)((byte)Note + (byte)Octave * NotesPerOctave);
    }

    /// <summary>
    /// Returns the note name, useful for Debug purposes
    /// </summary>
    /// <param name="useLatinName">Use the latin representation (Do, Re, Mi...)</param>
    /// <returns>The note name, e.g. <c>C#4</c> or <c>Do#4</c></returns>
    public string GetName(bool useLatinName = false)
    {
        var names = useLatinName ? LatinNames : EnglishNames;
        return $"{names[(int)Note]}{(int)Octave}";
    }

    /// <inheritdoc />
    public override string ToString() => GetName();
}
