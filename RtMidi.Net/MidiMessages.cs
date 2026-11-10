using System.Diagnostics.CodeAnalysis;


namespace RtMidi.Net;

/// <summary>
/// A MIDI message. Messages without data (e.g. <see cref="MidiMessageType.TimingClock"/>) use this type directly;
/// the others use a derived record, so a received message is usually examined with pattern matching.
/// </summary>
/// <remarks>
/// Messages are records: two messages with the same values are equal. The byte arrays of
/// <see cref="MidiMessageUnknown.MessageData"/> and <see cref="MidiMessageSystemExclusive.Data"/> are compared by
/// content.
/// </remarks>
/// <example>
/// <code>
/// switch (message)
/// {
///     case MidiMessageNote { Type: MidiMessageType.NoteOn, Velocity: > 0 } noteOn:
///         Console.WriteLine($"{noteOn.Note} pressed with velocity {noteOn.Velocity}");
///         break;
///     case MidiMessageControlChange controlChange:
///         Console.WriteLine($"CC {controlChange.ControlFunction} = {controlChange.Value}");
///         break;
/// }
/// </code>
/// </example>
/// <param name="Type">The message type. For channel messages it is the status byte without the channel.</param>
public record MidiMessage(MidiMessageType Type);

/// <summary>
/// A message that could not be decoded (empty, unknown status byte, missing data bytes or data bytes above 127).
/// Sending it sends <paramref name="MessageData"/> unchanged.
/// </summary>
/// <param name="MessageData">The raw bytes of the message, as received.</param>
public record MidiMessageUnknown(byte[] MessageData) : MidiMessage(MidiMessageType.Unknown)
{
    /// <summary>Compares the type and the bytes of <see cref="MessageData"/>.</summary>
    /// <param name="other">The other message.</param>
    /// <returns><see langword="true"/> if both have the same bytes.</returns>
    public virtual bool Equals(MidiMessageUnknown? other) =>
        ReferenceEquals(this, other) ||
        (base.Equals(other) && MessageData.AsSpan().SequenceEqual(other.MessageData));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.AddBytes(MessageData);
        return hash.ToHashCode();
    }
}

/// <summary>
/// Base of the channel messages that refer to a note: <see cref="MidiMessageNote"/> and <see cref="MidiMessageNoteAfterTouch"/>.
/// </summary>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Note">The note. <see cref="MidiNote.ToString"/> gives its name in scientific pitch notation (middle C is C4).</param>
/// <param name="Type">The message type.</param>
public abstract record MidiMessageNoteBase(MidiChannel Channel, MidiNote Note, MidiMessageType Type) : MidiMessage(Type);

/// <summary>
/// Note On or Note Off, depending on <see cref="MidiMessage.Type"/>.
/// </summary>
/// <remarks>
/// Many devices send a Note On with velocity 0 instead of a Note Off. Messages are delivered as received,
/// so check both when looking for released keys.
/// </remarks>
/// <example>
/// <code>
/// output.SendMessage(new MidiMessageNote(MidiChannel.Channel01, MidiNote.Parse("C4"), 100, MidiMessageType.NoteOn));
/// </code>
/// </example>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Note">The note (MIDI note 60 is middle C, C4).</param>
/// <param name="Velocity">How hard the key was pressed or released, 0 to 127. A Note On with 0 means Note Off.</param>
/// <param name="Type"><see cref="MidiMessageType.NoteOn"/> or <see cref="MidiMessageType.NoteOff"/>; any other type
/// can't be sent.</param>
public record MidiMessageNote(MidiChannel Channel, MidiNote Note, byte Velocity, MidiMessageType Type) : MidiMessageNoteBase(Channel, Note, Type);

/// <summary>
/// Polyphonic Key Pressure: the aftertouch of a single held note.
/// </summary>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Note">The held note.</param>
/// <param name="Pressure">The pressure on the key, 0 to 127.</param>
public record MidiMessageNoteAfterTouch(MidiChannel Channel, MidiNote Note, byte Pressure) : MidiMessageNoteBase(Channel, Note, MidiMessageType.PolyphonicKeyPressure);

/// <summary>
/// Control Change: a controller (knob, fader, pedal...) moved, or a channel mode message (controllers 120 to 127).
/// </summary>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="ControlFunction">The controller, e.g. <see cref="MidiControlFunction.DamperPedal"/>. Any number from 0 to 127
/// is valid, also those without a name: <c>(MidiControlFunction)20</c>.</param>
/// <param name="Value">The controller value, 0 to 127.</param>
public record MidiMessageControlChange(MidiChannel Channel, MidiControlFunction ControlFunction, byte Value) : MidiMessage(MidiMessageType.ControlChange)
{
    /// <summary>
    /// The control number (0-127) of <see cref="ControlFunction"/>, the first data byte of the message.
    /// </summary>
    public byte ControlNumber => (byte)ControlFunction;
}

/// <summary>
/// Program Change: selects an instrument or preset.
/// </summary>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Program">The program number, 0 to 127 (shown as 1 to 128 by many devices).</param>
public record MidiMessageProgramChange(MidiChannel Channel, byte Program) : MidiMessage(MidiMessageType.ProgramChange);

/// <summary>
/// Channel Pressure: a single aftertouch value for the whole channel, for devices without per-key pressure.
/// </summary>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Pressure">The pressure, 0 to 127.</param>
public record MidiMessageChannelAfterTouch(MidiChannel Channel, byte Pressure) : MidiMessage(MidiMessageType.ChannelPressure);

/// <summary>
/// Pitch Bend Change, a 14-bit value split in two 7-bit bytes. Use <see cref="Value"/> for the combined value or
/// <see cref="Bend"/> for the value relative to the centre, and <see cref="FromValue"/> or <see cref="FromBend"/>
/// to create one from them.
/// </summary>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Lsb">The low 7 bits of the bend value, 0 to 127.</param>
/// <param name="Msb">The high 7 bits of the bend value, 0 to 127.</param>
public record MidiMessagePitchBendChange(MidiChannel Channel, byte Lsb, byte Msb) : MidiMessage(MidiMessageType.PitchBendChange)
{
    /// <summary>
    /// The <see cref="Value"/> of the centre position, 8192 (2000H): no bend.
    /// </summary>
    public const int CenterValue = 8192;

    /// <summary>
    /// The largest <see cref="Value"/>, 16383 (3FFFH): maximum bend up.
    /// </summary>
    public const int MaxValue = 16383;

    /// <summary>
    /// The 14-bit bend value: 0 to 16383, 8192 is the centre (no bend).
    /// </summary>
    public int Value => ((Msb & 0x7F) << 7) | (Lsb & 0x7F);

    /// <summary>
    /// The bend relative to the centre: -8192 (maximum down) to +8191 (maximum up), 0 is no bend.
    /// </summary>
    /// <remarks>
    /// How many semitones the maximum bend is depends on the receiver; it can be set with the Pitch Bend
    /// Sensitivity registered parameter (RPN 0, 2 semitones by default on most devices).
    /// </remarks>
    public int Bend => Value - CenterValue;

    /// <summary>
    /// Creates a Pitch Bend Change from the 14-bit value.
    /// </summary>
    /// <param name="channel">The MIDI channel.</param>
    /// <param name="value">0 to 16383; <see cref="CenterValue"/> is no bend.</param>
    /// <returns>The message, with the value split in its two 7-bit bytes.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is outside 0 to 16383.</exception>
    public static MidiMessagePitchBendChange FromValue(MidiChannel channel, int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxValue);
        return new MidiMessagePitchBendChange(channel, (byte)(value & 0x7F), (byte)(value >> 7));
    }

    /// <summary>
    /// Creates a Pitch Bend Change from the bend relative to the centre.
    /// </summary>
    /// <param name="channel">The MIDI channel.</param>
    /// <param name="bend">-8192 (maximum down) to +8191 (maximum up); 0 is no bend.</param>
    /// <returns>The message, with the value split in its two 7-bit bytes.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bend"/> is outside -8192 to +8191.</exception>
    public static MidiMessagePitchBendChange FromBend(MidiChannel channel, int bend)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bend, -CenterValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bend, MaxValue - CenterValue);
        return FromValue(channel, bend + CenterValue);
    }
}

/// <summary>
/// System Exclusive (SysEx): data whose format is defined by a manufacturer (<see cref="ManufacturerId"/>), or by the
/// MIDI specification for the Universal System Exclusive messages (<see cref="IsUniversal"/>). Only received when
/// <see cref="MidiInputClient.IgnoreSysexMessages"/> is <see langword="false"/>.
/// </summary>
/// <remarks>
/// Format: <c>F0 &lt;manufacturer ID&gt; ... F7</c>; universal messages are
/// <c>F0 &lt;7E|7F&gt; &lt;device ID&gt; &lt;sub-ID#1&gt; &lt;sub-ID#2&gt; ... F7</c> (MIDI 1.0 Detailed Specification,
/// System Exclusive Messages). The factories create the universal messages most devices understand:
/// <see cref="IdentityRequest()"/>, <see cref="GeneralMidiSystemOn()"/> and <see cref="GeneralMidiSystemOff()"/>.
/// </remarks>
/// <param name="Data">The whole message, from 0xF0 to 0xF7 included. It is sent as is.</param>
public record MidiMessageSystemExclusive(byte[] Data) : MidiMessage(MidiMessageType.SystemExclusive)
{
    /// <summary>The device ID that addresses every device ("all call"), 7FH.</summary>
    public const byte AllCallDeviceId = 0x7F;

    private const byte Start = 0xF0;
    private const byte End = 0xF7;
    private const byte NonRealTime = 0x7E;
    private const byte RealTime = 0x7F;

    /// <summary>
    /// The manufacturer ID after F0H, or <see langword="null"/> if the message is too short or malformed. For universal
    /// messages it is <see cref="MidiManufacturerId.UniversalNonRealTime"/> or <see cref="MidiManufacturerId.UniversalRealTime"/>.
    /// </summary>
    public MidiManufacturerId? ManufacturerId =>
        Data is [Start, ..] && MidiManufacturerId.TryRead(Data.AsSpan(1), out var id) ? id : null;

    /// <summary>Whether it is a Universal System Exclusive message (ID 7EH or 7FH).</summary>
    public bool IsUniversal => Data is [Start, NonRealTime or RealTime, ..];

    /// <summary>Whether it is a Universal Real Time message (ID 7FH), such as the MTC Full Message.</summary>
    public bool IsUniversalRealTime => Data is [Start, RealTime, ..];

    /// <summary>The device ID of a universal message (7FH addresses every device), or <see langword="null"/>.</summary>
    public byte? DeviceId => UniversalByte(2);

    /// <summary>The sub-ID#1 of a universal message (its category, e.g. 06H General Information), or <see langword="null"/>.</summary>
    public byte? SubId1 => UniversalByte(3);

    /// <summary>
    /// The sub-ID#2 of a universal message (the message in its category), or <see langword="null"/>. The Non-Real Time
    /// Sample Dump messages (sub-ID#1 01H, 02H, 03H) and the handshaking messages (7BH to 7FH) have no sub-ID#2, so
    /// it is <see langword="null"/> for them (MIDI 1.0 Detailed Specification, table of Universal System Exclusive
    /// messages).
    /// </summary>
    public byte? SubId2 =>
        IsUniversalRealTime || SubId1 is not (null or 0x01 or 0x02 or 0x03 or >= 0x7B) ? UniversalByte(4) : null;

    /// <summary>
    /// Identity Request to every device: <c>F0 7E 7F 06 01 F7</c>. Devices that support it answer with an Identity Reply
    /// (see <see cref="TryGetIdentityReply"/>).
    /// </summary>
    /// <returns>The message.</returns>
    public static MidiMessageSystemExclusive IdentityRequest() => IdentityRequest(AllCallDeviceId);

    /// <summary>
    /// Identity Request: <c>F0 7E &lt;device ID&gt; 06 01 F7</c>.
    /// </summary>
    /// <param name="deviceId">The device to ask, 0 to 127; 7FH asks every device.</param>
    /// <returns>The message.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="deviceId"/> is above 127.</exception>
    public static MidiMessageSystemExclusive IdentityRequest(byte deviceId) => Universal(NonRealTime, deviceId, 0x06, 0x01);

    /// <summary>
    /// Turn General MIDI System On, to every device: <c>F0 7E 7F 09 01 F7</c>. A General MIDI device resets to its
    /// General MIDI configuration.
    /// </summary>
    /// <returns>The message.</returns>
    public static MidiMessageSystemExclusive GeneralMidiSystemOn() => GeneralMidiSystemOn(AllCallDeviceId);

    /// <summary>
    /// Turn General MIDI System On: <c>F0 7E &lt;device ID&gt; 09 01 F7</c>.
    /// </summary>
    /// <param name="deviceId">The device, 0 to 127; 7FH (recommended) addresses every device.</param>
    /// <returns>The message.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="deviceId"/> is above 127.</exception>
    public static MidiMessageSystemExclusive GeneralMidiSystemOn(byte deviceId) => Universal(NonRealTime, deviceId, 0x09, 0x01);

    /// <summary>
    /// Turn General MIDI System Off, to every device: <c>F0 7E 7F 09 02 F7</c>.
    /// </summary>
    /// <returns>The message.</returns>
    public static MidiMessageSystemExclusive GeneralMidiSystemOff() => GeneralMidiSystemOff(AllCallDeviceId);

    /// <summary>
    /// Turn General MIDI System Off: <c>F0 7E &lt;device ID&gt; 09 02 F7</c>.
    /// </summary>
    /// <param name="deviceId">The device, 0 to 127; 7FH (recommended) addresses every device.</param>
    /// <returns>The message.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="deviceId"/> is above 127.</exception>
    public static MidiMessageSystemExclusive GeneralMidiSystemOff(byte deviceId) => Universal(NonRealTime, deviceId, 0x09, 0x02);

    /// <summary>
    /// Reads an Identity Reply: <c>F0 7E &lt;device ID&gt; 06 02 mm ff ff dd dd ss ss ss ss F7</c>, where the manufacturer
    /// ID <c>mm</c> has one or three bytes.
    /// </summary>
    /// <param name="reply">The reply, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the message is an Identity Reply.</returns>
    public bool TryGetIdentityReply([NotNullWhen(true)] out MidiIdentityReply? reply)
    {
        reply = null;
        if (Data is not [Start, NonRealTime, <= 0x7F and var deviceId, 0x06, 0x02, ..] ||
            !MidiManufacturerId.TryRead(Data.AsSpan(5), out var manufacturer))
        {
            return false;
        }

        var rest = Data.AsSpan(5 + manufacturer.Length);
        if (rest.Length != 9 || rest[8] != End) return false;
        foreach (var value in rest[..8])
        {
            if (value > 0x7F) return false;
        }

        reply = new MidiIdentityReply(deviceId, manufacturer, rest[0] | (rest[1] << 7), rest[2] | (rest[3] << 7),
            ((uint)rest[4] << 24) | ((uint)rest[5] << 16) | ((uint)rest[6] << 8) | rest[7]);
        return true;
    }

    /// <summary>
    /// Reads an MTC Full Message: <c>F0 7F &lt;device ID&gt; 01 01 hr mn sc fr F7</c>.
    /// </summary>
    /// <param name="timeCode">The time, or the default value.</param>
    /// <returns><see langword="true"/> if the message is a valid MTC Full Message.</returns>
    public bool TryGetTimeCode(out MidiTimeCode timeCode)
    {
        if (Data is [Start, RealTime, <= 0x7F, 0x01, 0x01, <= 0x7F and var hours, <= 0x7F and var minutes,
                <= 0x7F and var seconds, <= 0x7F and var frames, End])
        {
            return MidiTimeCode.TryCreate(hours, minutes, seconds, frames, out timeCode);
        }

        timeCode = default;
        return false;
    }

    /// <summary>Compares the type and the bytes of <see cref="Data"/>.</summary>
    /// <param name="other">The other message.</param>
    /// <returns><see langword="true"/> if both have the same bytes.</returns>
    public virtual bool Equals(MidiMessageSystemExclusive? other) =>
        ReferenceEquals(this, other) ||
        (base.Equals(other) && Data.AsSpan().SequenceEqual(other.Data));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.AddBytes(Data);
        return hash.ToHashCode();
    }

    private byte? UniversalByte(int index) =>
        IsUniversal && Data.Length > index && Data[index] <= 0x7F ? Data[index] : null;

    private static MidiMessageSystemExclusive Universal(byte id, byte deviceId, byte subId1, byte subId2)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(deviceId, (byte)0x7F);
        return new MidiMessageSystemExclusive([Start, id, deviceId, subId1, subId2, End]);
    }
}

/// <summary>
/// MIDI Time Code Quarter Frame. Eight of them, one per <paramref name="Piece"/>, make a complete time code.
/// Only received when <see cref="MidiInputClient.IgnoreTimeMessages"/> is <see langword="false"/>.
/// </summary>
/// <param name="Piece">Which part of the time code this message carries (0-7):
/// 0/1 frames, 2/3 seconds, 4/5 minutes, 6/7 hours and rate.</param>
/// <param name="Value">The 4-bit value of that part (0-15).</param>
public record MidiMessageTimeCodeQuarterFrame(byte Piece, byte Value) : MidiMessage(MidiMessageType.TimeCodeQuarterFrame);

/// <summary>
/// Song Position Pointer, a 14-bit value (in MIDI beats) split in two 7-bit bytes. Use <see cref="Position"/>
/// for the combined value, and <see cref="FromPosition"/> to create one from it.
/// </summary>
/// <param name="Lsb">The low 7 bits of the position, 0 to 127.</param>
/// <param name="Msb">The high 7 bits of the position, 0 to 127.</param>
public record MidiMessageSongPositionPointer(byte Lsb, byte Msb) : MidiMessage(MidiMessageType.SongPositionPointer)
{
    /// <summary>
    /// The largest <see cref="Position"/>, 16383.
    /// </summary>
    public const int MaxPosition = 16383;

    /// <summary>
    /// The 14-bit position, in MIDI beats (sixteenth notes) since the start of the song.
    /// </summary>
    public int Position => ((Msb & 0x7F) << 7) | (Lsb & 0x7F);

    /// <summary>
    /// Creates a Song Position Pointer from the position.
    /// </summary>
    /// <param name="position">MIDI beats (sixteenth notes, 6 MIDI clocks each) since the start of the song, 0 to 16383.</param>
    /// <returns>The message, with the position split in its two 7-bit bytes.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> is outside 0 to 16383.</exception>
    public static MidiMessageSongPositionPointer FromPosition(int position)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(position, MaxPosition);
        return new MidiMessageSongPositionPointer((byte)(position & 0x7F), (byte)(position >> 7));
    }
}

/// <summary>
/// Song Select: chooses the song or sequence to play.
/// </summary>
/// <param name="SongNumber">The song number, 0 to 127.</param>
public record MidiMessageSongSelect(byte SongNumber) : MidiMessage(MidiMessageType.SongSelect);
