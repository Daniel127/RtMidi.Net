using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace RtMidi.Net;

/// <summary>
/// A MIDI note number (0-127) as a note and an octave, in scientific pitch notation (ISO):
/// 0 is C-1, 60 (middle C) is C4, 69 (A440) is A4 and 127 is G9.
/// </summary>
/// <remarks>
/// The octave numbering only changes names: <see cref="Number"/> is the same whatever numbering is used
/// to show or read the note. <c>default(MidiNote)</c> is MIDI note 0 (C-1).
/// </remarks>
/// <example>
/// <code>
/// var middleC = new MidiNote(60);
/// string name = middleC.GetName();                          // "C4"
/// string latin = middleC.GetName(useLatinName: true);       // "Do4"
/// var a440 = new MidiNote(MusicNote.A, MusicOctave.Octave4); // MIDI note 69
/// byte bFlat = MidiNote.Parse("Bb3").Number;                 // 58
/// </code>
/// </example>
public readonly struct MidiNote : IEquatable<MidiNote>, IComparable<MidiNote>
{
    /// <summary>
    /// The octave number of middle C (MIDI note 60) in scientific pitch notation, the default for
    /// <see cref="GetName"/> and <see cref="Parse"/>.
    /// </summary>
    public const int DefaultMiddleCOctave = 4;

    /// <summary>
    /// Lowest octave number accepted for middle C by <see cref="GetName"/>, <see cref="Parse"/> and <see cref="TryParse"/>.
    /// </summary>
    public const int MinMiddleCOctave = -10;

    /// <summary>
    /// Highest octave number accepted for middle C by <see cref="GetName"/>, <see cref="Parse"/> and <see cref="TryParse"/>.
    /// </summary>
    public const int MaxMiddleCOctave = 10;

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
    /// The note inside the octave (C to B), always one of the English names; the Latin names of
    /// <see cref="MusicNote"/> are aliases with the same values.
    /// </summary>
    public MusicNote Note { get; }

    /// <summary>
    /// The octave, in scientific pitch notation (middle C is in <see cref="MusicOctave.Octave4"/>).
    /// </summary>
    public MusicOctave Octave { get; }

    /// <summary>
    /// The MIDI note number (0-127), the data byte sent in note messages.
    /// </summary>
    public byte Number => (byte)((byte)Note + (byte)Octave * NotesPerOctave);

    /// <summary>
    /// Creates the note from its MIDI note number.
    /// </summary>
    /// <param name="note">The MIDI note number, 0 to 127 (60 is middle C, C4).</param>
    /// <exception cref="ArgumentOutOfRangeException">The number is above 127.</exception>
    public MidiNote(byte note)
    {
        if (note > MaxNoteNumber) throw new ArgumentOutOfRangeException(nameof(note), note, "The note must be 0 to 127");
        Note = (MusicNote)(note % NotesPerOctave);
        Octave = (MusicOctave)(note / NotesPerOctave);
    }

    /// <summary>
    /// Creates the note from a note and an octave.
    /// </summary>
    /// <param name="note">The note inside the octave.</param>
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
    /// Returns the note name, e.g. <c>C4</c> or <c>Do4</c> for middle C (MIDI note 60).
    /// </summary>
    /// <param name="useLatinName"><see langword="true"/> for Latin names (Do, Re, Mi...), <see langword="false"/>
    /// (default) for English names (C, D, E...). Accidentals are always written as sharps (<c>#</c>).</param>
    /// <param name="middleCOctave">The octave number given to middle C. The default, 4, is
    /// scientific pitch notation (ISO); some software uses 3 (e.g. Yamaha, Ableton) or 5.
    /// It only changes the text, never the MIDI note. Between <see cref="MinMiddleCOctave"/> and <see cref="MaxMiddleCOctave"/>.</param>
    /// <returns>The note name, e.g. <c>C#4</c>, <c>Do#4</c> or <c>C-1</c></returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="middleCOctave"/> is outside
    /// <see cref="MinMiddleCOctave"/> to <see cref="MaxMiddleCOctave"/>.</exception>
    public string GetName(bool useLatinName = false, int middleCOctave = DefaultMiddleCOctave)
    {
        ValidateMiddleCOctave(middleCOctave);
        var names = useLatinName ? LatinNames : EnglishNames;
        var octave = (int)Octave - MiddleCOctaveValue + middleCOctave;
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
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="middleCOctave"/> is outside
    /// <see cref="MinMiddleCOctave"/> to <see cref="MaxMiddleCOctave"/>.</exception>
    /// <exception cref="FormatException">The name is not a note, or it is outside MIDI notes 0 to 127.</exception>
    public static MidiNote Parse(string name, int middleCOctave = DefaultMiddleCOctave)
    {
        ArgumentNullException.ThrowIfNull(name);
        ValidateMiddleCOctave(middleCOctave);
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
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="middleCOctave"/> is outside
    /// <see cref="MinMiddleCOctave"/> to <see cref="MaxMiddleCOctave"/> (a wrong setting, not a wrong name).</exception>
    public static bool TryParse([NotNullWhen(true)] string? name, out MidiNote note, int middleCOctave = DefaultMiddleCOctave)
    {
        ValidateMiddleCOctave(middleCOctave);
        note = default;
        if (string.IsNullOrWhiteSpace(name)) return false;
        var text = name.AsSpan().Trim();

        if (!TryReadPitchClass(ref text, out var pitchClass)) return false;
        var accidental = ReadAccidentals(ref text);
        if (!TryReadOctave(text, out var octave)) return false;

        // long: many accidentals cannot overflow.
        var number = ((long)octave - middleCOctave + MiddleCOctaveValue) * NotesPerOctave + pitchClass + accidental;
        if (number is < 0 or > MaxNoteNumber) return false;

        note = new MidiNote((byte)number);
        return true;
    }

    /// <summary>
    /// Reads the natural note at the start of <paramref name="text"/> (English or Latin) and removes it.
    /// </summary>
    private static bool TryReadPitchClass(ref ReadOnlySpan<char> text, out int pitchClass)
    {
        foreach (var (noteName, value) in ParseNames)
        {
            if (!text.StartsWith(noteName, StringComparison.OrdinalIgnoreCase)) continue;
            pitchClass = value;
            text = text[noteName.Length..];
            return true;
        }

        pitchClass = -1;
        return false;
    }

    /// <summary>
    /// Reads the sharps (+1 each) and flats (-1 each) at the start of <paramref name="text"/> and removes them.
    /// </summary>
    private static long ReadAccidentals(ref ReadOnlySpan<char> text)
    {
        long accidental = 0;
        while (!text.IsEmpty && text[0] is '#' or '♯' or 'b' or '♭')
        {
            accidental += text[0] is '#' or '♯' ? 1 : -1;
            text = text[1..];
        }

        return accidental;
    }

    /// <summary>
    /// Reads the octave: an optional '-' and digits only (no '+', spaces or other signs).
    /// Leading zeros are allowed; a number too large for an int is not a valid octave.
    /// </summary>
    private static bool TryReadOctave(ReadOnlySpan<char> text, out int octave)
    {
        octave = 0;
        var digits = text.StartsWith("-") ? text[1..] : text;
        if (digits.IsEmpty) return false;
        foreach (var c in digits)
        {
            if (c is < '0' or > '9') return false;
        }

        return int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out octave);
    }

    private static void ValidateMiddleCOctave(int middleCOctave, [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(middleCOctave))] string? paramName = null)
    {
        if (middleCOctave is < MinMiddleCOctave or > MaxMiddleCOctave)
            throw new ArgumentOutOfRangeException(paramName, middleCOctave,
                $"The octave of middle C must be between {MinMiddleCOctave} and {MaxMiddleCOctave} (4 is scientific pitch notation).");
    }

    /// <summary>
    /// Returns the English name in scientific pitch notation, e.g. <c>C4</c> for MIDI note 60.
    /// Same as <see cref="GetName"/> with its default arguments.
    /// </summary>
    /// <returns>The note name.</returns>
    public override string ToString() => GetName();

    /// <summary>
    /// Whether both notes have the same MIDI note number.
    /// </summary>
    /// <param name="other">The note to compare with.</param>
    /// <returns>true if they are the same note.</returns>
    public bool Equals(MidiNote other) => Number == other.Number;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is MidiNote other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Number;

    /// <summary>
    /// Compares the pitch of two notes (their MIDI note numbers).
    /// </summary>
    /// <param name="other">The note to compare with.</param>
    /// <returns>Less than zero if this note is lower, zero if equal, greater than zero if higher.</returns>
    public int CompareTo(MidiNote other) => Number.CompareTo(other.Number);

    /// <summary>Whether both notes have the same MIDI note number.</summary>
    /// <param name="left">The first note.</param>
    /// <param name="right">The second note.</param>
    /// <returns>true if they are the same note.</returns>
    public static bool operator ==(MidiNote left, MidiNote right) => left.Equals(right);

    /// <summary>Whether the notes have different MIDI note numbers.</summary>
    /// <param name="left">The first note.</param>
    /// <param name="right">The second note.</param>
    /// <returns>true if they are different notes.</returns>
    public static bool operator !=(MidiNote left, MidiNote right) => !left.Equals(right);

    /// <summary>Whether <paramref name="left"/> is lower than <paramref name="right"/>.</summary>
    /// <param name="left">The first note.</param>
    /// <param name="right">The second note.</param>
    /// <returns>true if the first note is lower.</returns>
    public static bool operator <(MidiNote left, MidiNote right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> is lower than or the same as <paramref name="right"/>.</summary>
    /// <param name="left">The first note.</param>
    /// <param name="right">The second note.</param>
    /// <returns>true if the first note is not higher.</returns>
    public static bool operator <=(MidiNote left, MidiNote right) => left.CompareTo(right) <= 0;

    /// <summary>Whether <paramref name="left"/> is higher than <paramref name="right"/>.</summary>
    /// <param name="left">The first note.</param>
    /// <param name="right">The second note.</param>
    /// <returns>true if the first note is higher.</returns>
    public static bool operator >(MidiNote left, MidiNote right) => left.CompareTo(right) > 0;

    /// <summary>Whether <paramref name="left"/> is higher than or the same as <paramref name="right"/>.</summary>
    /// <param name="left">The first note.</param>
    /// <param name="right">The second note.</param>
    /// <returns>true if the first note is not lower.</returns>
    public static bool operator >=(MidiNote left, MidiNote right) => left.CompareTo(right) >= 0;
}
