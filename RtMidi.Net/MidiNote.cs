using RtMidi.Net.Enums;

namespace RtMidi.Net;

public readonly struct MidiNote
{
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
    public MidiNote(byte note)
    {
        if (note > 127) throw new ArgumentOutOfRangeException(nameof(note), "The note must be 0 to 127");
        Note = (MusicNote)(note % 12);
        Octave = (MusicOctave)(note / 12);
    }

    /// <summary>
    /// Create a MidiNote
    /// </summary>
    /// <param name="note">The note</param>
    /// <param name="octave">The octave</param>
    public MidiNote(MusicNote note, MusicOctave octave)
    {
        Note = note;
        Octave = octave;
    }

    /// <summary>
    /// Get Byte for MIDI message
    /// </summary>
    /// <returns>The MIDI byte representation</returns>
    public byte GetByteRepresentation()
    {
        return (byte)((byte)Note + (byte)((byte)Octave * 12));
    }

    /// <summary>
    /// Returns the note name, useful for Debug purposes
    /// </summary>
    /// <param name="useLatinName">Use the latin representation (Do, Re, Mi...)</param>
    /// <returns>The note name</returns>
    public string GetName(bool useLatinName = false)
    {
        var notesNames = Enum.GetNames<MusicNote>();
        var angloSaxonNames = notesNames
            .Where(name => name.Length == 1
                           || name.StartsWith("CS")
                           || name.StartsWith("DS")
                           || name.StartsWith("FS")
                           || name.StartsWith("GS")
                           || name.StartsWith("AS"))
            .ToList();

        string noteName;

        if (useLatinName)
        {
            var latinName = notesNames.Except(angloSaxonNames).ToList();
            noteName = latinName[(int)Note];
        }
        else
        {
            noteName = angloSaxonNames[(int)Note];
        }

        return $"{noteName.Replace("Sharp", "#")}{(int)Octave}";
    }
}