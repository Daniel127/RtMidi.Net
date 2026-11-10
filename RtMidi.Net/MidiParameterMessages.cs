namespace RtMidi.Net;

/// <summary>
/// Builds the Control Change sequences that set a registered (RPN) or non-registered (NRPN) parameter, or a
/// 14-bit controller, following the procedure of the MIDI 1.0 specification.
/// </summary>
/// <remarks>
/// <para>
/// A parameter is set by selecting its number (Control Changes 101 and 100 for an RPN, 99 and 98 for an NRPN) and then
/// sending its value with Data Entry (Control Change 6 for the MSB and, when the parameter uses it, 38 for the LSB).
/// The sequences end by selecting the null parameter (RPN 7FH 7FH), so later Data Entry messages, e.g. from a slider,
/// don't change the parameter by accident.
/// </para>
/// <para>
/// Send the messages in order, on the same output: <c>foreach (var message in messages) output.SendMessage(message);</c>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Pitch bend range of ±12 semitones on channel 1
/// var messages = MidiParameterMessages.SetRegisteredParameter(MidiChannel.Channel01,
///     MidiRegisteredParameter.PitchBendSensitivity, 12, 0);
/// foreach (var message in messages) output.SendMessage(message);
/// </code>
/// </example>
public static class MidiParameterMessages
{
    private const byte MaxDataByte = 127;
    private const int MaxFourteenBitValue = 16383;

    /// <summary>
    /// The messages that set a registered parameter that only uses the Data Entry MSB (e.g.
    /// <see cref="MidiRegisteredParameter.ChannelCoarseTuning"/>).
    /// </summary>
    /// <param name="channel">The MIDI channel.</param>
    /// <param name="parameter">The parameter. Any number from 0 to 16383 except <see cref="MidiRegisteredParameter.Null"/>.</param>
    /// <param name="valueMsb">The Data Entry MSB, 0 to 127.</param>
    /// <returns>Control Changes 101, 100, 6, then 101 and 100 with 127 (null).</returns>
    /// <exception cref="ArgumentOutOfRangeException">A value is out of range, or <paramref name="parameter"/> is the null parameter.</exception>
    public static IReadOnlyList<MidiMessageControlChange> SetRegisteredParameter(MidiChannel channel,
        MidiRegisteredParameter parameter, byte valueMsb)
    {
        return Build(channel, MidiParameterKind.Registered, (int)parameter, valueMsb, null);
    }

    /// <summary>
    /// The messages that set a registered parameter with the Data Entry MSB and LSB (e.g.
    /// <see cref="MidiRegisteredParameter.PitchBendSensitivity"/>: semitones and cents).
    /// </summary>
    /// <param name="channel">The MIDI channel.</param>
    /// <param name="parameter">The parameter. Any number from 0 to 16383 except <see cref="MidiRegisteredParameter.Null"/>.</param>
    /// <param name="valueMsb">The Data Entry MSB, 0 to 127.</param>
    /// <param name="valueLsb">The Data Entry LSB, 0 to 127.</param>
    /// <returns>Control Changes 101, 100, 6, 38, then 101 and 100 with 127 (null).</returns>
    /// <exception cref="ArgumentOutOfRangeException">A value is out of range, or <paramref name="parameter"/> is the null parameter.</exception>
    public static IReadOnlyList<MidiMessageControlChange> SetRegisteredParameter(MidiChannel channel,
        MidiRegisteredParameter parameter, byte valueMsb, byte valueLsb)
    {
        return Build(channel, MidiParameterKind.Registered, (int)parameter, valueMsb, valueLsb);
    }

    /// <summary>
    /// The messages that set a non-registered parameter (NRPN) with the Data Entry MSB only.
    /// </summary>
    /// <param name="channel">The MIDI channel.</param>
    /// <param name="parameter">The parameter number, <c>(MSB &lt;&lt; 7) | LSB</c>: 0 to 16382 (16383 is the null parameter).
    /// The manufacturer of the device defines them.</param>
    /// <param name="valueMsb">The Data Entry MSB, 0 to 127.</param>
    /// <returns>Control Changes 99, 98, 6, then 101 and 100 with 127 (null).</returns>
    /// <exception cref="ArgumentOutOfRangeException">A value is out of range.</exception>
    public static IReadOnlyList<MidiMessageControlChange> SetNonRegisteredParameter(MidiChannel channel,
        int parameter, byte valueMsb)
    {
        return Build(channel, MidiParameterKind.NonRegistered, parameter, valueMsb, null);
    }

    /// <summary>
    /// The messages that set a non-registered parameter (NRPN) with the Data Entry MSB and LSB.
    /// </summary>
    /// <param name="channel">The MIDI channel.</param>
    /// <param name="parameter">The parameter number, <c>(MSB &lt;&lt; 7) | LSB</c>: 0 to 16382 (16383 is the null parameter).
    /// The manufacturer of the device defines them.</param>
    /// <param name="valueMsb">The Data Entry MSB, 0 to 127.</param>
    /// <param name="valueLsb">The Data Entry LSB, 0 to 127.</param>
    /// <returns>Control Changes 99, 98, 6, 38, then 101 and 100 with 127 (null).</returns>
    /// <exception cref="ArgumentOutOfRangeException">A value is out of range.</exception>
    public static IReadOnlyList<MidiMessageControlChange> SetNonRegisteredParameter(MidiChannel channel,
        int parameter, byte valueMsb, byte valueLsb)
    {
        return Build(channel, MidiParameterKind.NonRegistered, parameter, valueMsb, valueLsb);
    }

    /// <summary>
    /// The two messages that set a controller with 14-bit resolution: its MSB (controller 0-31) and then its LSB
    /// (controller 32-63). The MSB goes first because receiving an MSB resets the LSB.
    /// </summary>
    /// <param name="channel">The MIDI channel.</param>
    /// <param name="controller">The MSB controller, 0 to 31, e.g. <see cref="MidiControlFunction.ChannelVolume"/>.
    /// Not <see cref="MidiControlFunction.DataEntry"/>: use the parameter methods for it.</param>
    /// <param name="value">The 14-bit value, 0 to 16383.</param>
    /// <returns>The MSB and LSB Control Changes.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="controller"/> is not 0-31 or is Data Entry, or
    /// <paramref name="value"/> is out of range.</exception>
    public static IReadOnlyList<MidiMessageControlChange> SetHighResolutionController(MidiChannel channel,
        MidiControlFunction controller, int value)
    {
        if ((byte)controller > 31 || controller == MidiControlFunction.DataEntry)
        {
            throw new ArgumentOutOfRangeException(nameof(controller), controller,
                "Only controllers 0 to 31 have an LSB; Data Entry is set with the parameter methods.");
        }

        ArgumentOutOfRangeException.ThrowIfNegative(value);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxFourteenBitValue);
        return
        [
            new MidiMessageControlChange(channel, controller, (byte)(value >> 7)),
            new MidiMessageControlChange(channel, controller + 32, (byte)(value & 0x7F)),
        ];
    }

    private static MidiMessageControlChange[] Build(MidiChannel channel, MidiParameterKind kind, int parameter,
        byte valueMsb, byte? valueLsb)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(parameter);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(parameter, (int)MidiRegisteredParameter.Null);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(valueMsb, MaxDataByte);
        if (valueLsb > MaxDataByte)
        {
            throw new ArgumentOutOfRangeException(nameof(valueLsb), valueLsb, "A data byte is 0 to 127.");
        }

        var (msbController, lsbController) = kind == MidiParameterKind.Registered
            ? (MidiControlFunction.RegisteredParameterNumberMsb, MidiControlFunction.RegisteredParameterNumberLsb)
            : (MidiControlFunction.NonRegisteredParameterNumberMsb, MidiControlFunction.NonRegisteredParameterNumberLsb);

        var messages = new List<MidiMessageControlChange>(6)
        {
            new(channel, msbController, (byte)(parameter >> 7)),
            new(channel, lsbController, (byte)(parameter & 0x7F)),
            new(channel, MidiControlFunction.DataEntry, valueMsb),
        };
        if (valueLsb is { } dataLsb) messages.Add(new(channel, MidiControlFunction.DataEntryLsb, dataLsb));
        messages.Add(new(channel, MidiControlFunction.RegisteredParameterNumberMsb, 0x7F));
        messages.Add(new(channel, MidiControlFunction.RegisteredParameterNumberLsb, 0x7F));
        return [.. messages];
    }
}
