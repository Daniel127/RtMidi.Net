namespace RtMidi.Net;

/// <summary>
/// Helpers for General MIDI Level 1 (GM 1), the standard set of 128 instruments and 47 percussion sounds that
/// GM-compatible synthesizers play for each program and drum note.
/// </summary>
/// <remarks>
/// See <see cref="GeneralMidiProgram"/>, <see cref="GeneralMidiPercussion"/> and <see cref="GeneralMidiFamily"/>.
/// Sound set: https://www.midi.org/specifications-old/item/gm-level-1-sound-set (archived).
/// </remarks>
/// <example>
/// <code>
/// output.SendMessage(new MidiMessageProgramChange(MidiChannel.Channel01, (byte)GeneralMidiProgram.Violin));
/// output.SendMessage(new MidiMessageNote(GeneralMidi.PercussionChannel,
///     new MidiNote((byte)GeneralMidiPercussion.AcousticSnare), 100, MidiMessageType.NoteOn));
/// </code>
/// </example>
public static class GeneralMidi
{
    /// <summary>
    /// Channel 10, which plays the <see cref="GeneralMidiPercussion"/> sounds instead of a program.
    /// </summary>
    public const MidiChannel PercussionChannel = MidiChannel.Channel10;

    /// <summary>
    /// The family of a program: each family groups 8 consecutive programs.
    /// </summary>
    /// <param name="program">A program, 0 to 127.</param>
    /// <returns>The family, e.g. <see cref="GeneralMidiFamily.Strings"/> for <see cref="GeneralMidiProgram.Violin"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="program"/> is above 127.</exception>
    public static GeneralMidiFamily GetFamily(GeneralMidiProgram program)
    {
        if ((byte)program > 127)
        {
            throw new ArgumentOutOfRangeException(nameof(program), program, "A program is 0 to 127.");
        }

        return (GeneralMidiFamily)((byte)program / 8);
    }
}
