using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using RtMidi.Net.Enums;

namespace RtMidi.Net;

/// <summary>
/// A MIDI note number (0-127) as a note and an octave, in scientific pitch notation (ISO):
/// 0 is C-1, 60 (middle C) is C4, 69 (A440) is A4 and 127 is G9.
/// </summary>
public readonly struct MidiNote
{
    /// <summary>
    /// The octave number of middle C (MIDI note 60) in scientific pitch notation, the default for
    /// <see cref="GetName"/> and <see cref="Parse"/>.
    /// </summary>
    public const int DefaultMiddleCOctave = 4;

    private const byte MaxNoteNumber = 127;
    private const int NotesPerOctave = 12;

    /// <summary>
    /// <see cref="MusicOctave"/> value of the octave that contains middle C (60 / 12).
    /// </summary>
    private const int MiddleCOctaveValue = 5;

    private static readonly string[] EnglishNames = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];
    private static readonly string[] LatinNames = ["Do", "Do#", "Re", "Re#", "Mi", "Fa", "Fa#", "Sol", "Sol#", "La", "La#", "Si"];

    // Natural notes accepted by Parse, Latin first: "Fa" and "Do" also start with an English letter.
    private static readonly (string Name, int PitchClass)[] ParseNames =
    [
        ("Sol", 7), ("Do", 0), ("Re", 2), ("Mi", 4), ("Fa", 5), ("La", 9), ("Si", 11),
        ("C", 0), ("D", 2), ("E", 4), ("F", 5), ("G", 7), ("A", 9), ("B", 11),
    ];

    /// <summary>
    /// The note
    /// </summary>
    public MusicNote Note { get; }

    /// <summary>
    /// The octave
    /// </summary>
    public MusicOctave Octave { get; }

    /// <summary>
    /// The MIDI note number (0-127).
    /// </summary>
    public byte Number => GetByteRepresentation();

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
    /// <param name="octave">The octave, in scientific pitch notation (middle C is C in <see cref="MusicOctave.Octave4"/>)</param>
    /// <exception cref="ArgumentOutOfRangeException">The note is not a valid <see cref="MusicNote"/>,
    /// or the result is above MIDI note 127 (G9).</exception>
    public MidiNote(MusicNote note, MusicOctave octave)
    {
        if ((byte)note >= NotesPerOctave)
            throw new ArgumentOutOfRangeException(nameof(note), note, "Unknown note.");
        if ((byte)octave * NotesPerOctave + (byte)note > MaxNoteNumber)
            throw new ArgumentOutOfRangeException(nameof(octave), octave, $"{note} in {octave} is above MIDI note 127 (G9).");
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
    /// Returns the note name, e.g. <c>C4</c> or <c>Do4</c> for middle C (MIDI note 60).
    /// </summary>
    /// <param name="useLatinName">Use the latin representation (Do, Re, Mi...)</param>
    /// <param name="middleCOctave">The octave number given to middle C. The default, 4, is
    /// scientific pitch notation (ISO); some software uses 3 (e.g. Yamaha, Ableton) or 5.
    /// It only changes the text, never the MIDI note.</param>
    /// <returns>The note name, e.g. <c>C#4</c>, <c>Do#4</c> or <c>C-1</c></returns>
    public string GetName(bool useLatinName = false, int middleCOctave = DefaultMiddleCOctave)
    {
        var names = useLatinName ? LatinNames : EnglishNames;
        var octave = (long)Octave - MiddleCOctaveValue + middleCOctave;
        return string.Create(CultureInfo.InvariantCulture, $"{names[(int)Note]}{octave}");
    }

    /// <summary>
    /// Reads a note name such as <c>C4</c>, <c>c#4</c>, <c>Db4</c>, <c>Do4</c>, <c>Sib3</c> or <c>C-1</c>.
    /// </summary>
    /// <param name="name">English (C to B) or Latin (Do to Si) note, case-insensitive, then any
    /// number of sharps (<c>#</c>, <c>♯</c>) or flats (<c>b</c>, <c>♭</c>), then the octave number.
    /// Accidentals can cross an octave: <c>Cb4</c> is B3 and <c>B#3</c> is C4.</param>
    /// <param name="middleCOctave">The octave number that <paramref name="name"/> uses for middle C;
    /// 4 (default) is scientific pitch notation.</param>
    /// <returns>The note.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
    /// <exception cref="FormatException">The name is not a note, or it is outside MIDI notes 0 to 127.</exception>
    public static MidiNote Parse(string name, int middleCOctave = DefaultMiddleCOctave)
    {
        ArgumentNullException.ThrowIfNull(name);
        return TryParse(name, out var note, middleCOctave)
            ? note
            : throw new FormatException($"'{name}' is not a note between MIDI notes 0 and 127 (e.g. C4, Do#4, Bb3).");
    }

    /// <summary>
    /// Reads a note name like <see cref="Parse"/>, without throwing.
    /// </summary>
    /// <param name="name">The note name, see <see cref="Parse"/>.</param>
    /// <param name="note">The note, when the name is valid.</param>
    /// <param name="middleCOctave">The octave number that <paramref name="name"/> uses for middle C.</param>
    /// <returns><see langword="true"/> if the name is a note between MIDI notes 0 and 127.</returns>
    public static bool TryParse([NotNullWhen(true)] string? name, out MidiNote note, int middleCOctave = DefaultMiddleCOctave)
    {
        note = default;
        if (string.IsNullOrWhiteSpace(name)) return false;
        var text = name.AsSpan().Trim();

        var pitchClass = -1;
        foreach (var (noteName, value) in ParseNames)
        {
            if (!text.StartsWith(noteName, StringComparison.OrdinalIgnoreCase)) continue;
            pitchClass = value;
            text = text[noteName.Length..];
            break;
        }
        if (pitchClass < 0) return false;

        long accidental = 0;
        while (!text.IsEmpty && text[0] is '#' or '♯' or 'b' or '♭')
        {
            accidental += text[0] is '#' or '♯' ? 1 : -1;
            text = text[1..];
        }

        // Only an optional '-' and digits: no '+', spaces or other signs.
        var digits = text.StartsWith("-") ? text[1..] : text;
        if (digits.IsEmpty || digits.Length > 3) return false;
        foreach (var c in digits)
        {
            if (c is < '0' or > '9') return false;
        }
        var octave = int.Parse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);

        // long: neither a large middleCOctave nor many accidentals can overflow.
        var number = ((long)octave - middleCOctave + MiddleCOctaveValue) * NotesPerOctave + pitchClass + accidental;
        if (number is < 0 or > MaxNoteNumber) return false;

        note = new MidiNote((byte)number);
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => GetName();
}
