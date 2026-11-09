using System.Diagnostics.CodeAnalysis;

namespace RtMidi.Net;

/// <summary>
/// Follows the Control Changes of each channel and assembles the values that take more than one message:
/// registered and non-registered parameters (RPN/NRPN) and 14-bit controllers.
/// </summary>
/// <remarks>
/// <para>
/// Pass every received message to <see cref="TryDecode"/>, in order. It returns <see langword="true"/> with a
/// <see cref="MidiParameterChange"/>, <see cref="MidiParameterIncrement"/> or <see cref="MidiHighResolutionControlChange"/>
/// when a message completes one, and <see langword="false"/> for the rest. The messages themselves are not changed, so
/// they can still be handled as usual.
/// </para>
/// <para>
/// It follows the MIDI 1.0 Detailed Specification (Registered and Non-Registered Parameter Numbers, Control Change
/// MSB and LSB) and the Recommended Practices RP-015 and RP-018:
/// </para>
/// <list type="bullet">
/// <item>A parameter is selected once both bytes of its number have been received (in any order); after that, one
/// byte is enough to change it. Data Entry applies to the parameter selected last, RPN or NRPN.</item>
/// <item>Data Entry, Data Increment and Data Decrement produce nothing while no parameter or the null parameter
/// (7FH 7FH) is selected, and a Data Entry LSB needs the MSB sent after the parameter was selected.</item>
/// <item>An MSB resets the LSB of its controller; an LSB without a previous MSB produces nothing.</item>
/// <item>Reset All Controllers (121) deselects the parameter, and System Reset forgets every channel.</item>
/// </list>
/// <para>
/// Many devices use these controllers in their own ways, which is why this class is experimental: its rules or shape
/// may change in a minor version. Not thread-safe: use one instance per input, from one thread (e.g. the
/// <see cref="MidiInputClient.MessageReceived"/> handler).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var decoder = new MidiControllerDecoder();
/// input.MessageReceived += (_, e) =&gt;
/// {
///     if (decoder.TryDecode(e.Message, out var result) &amp;&amp;
///         result is MidiParameterChange { RegisteredParameter: MidiRegisteredParameter.PitchBendSensitivity } range)
///         Console.WriteLine($"Pitch bend range: {range.ValueMsb} semitones");
/// };
/// </code>
/// </example>
[Experimental(DiagnosticId)]
public sealed class MidiControllerDecoder
{
    /// <summary>
    /// The diagnostic id of the experimental controller decoder API. Suppress it (e.g.
    /// <c>&lt;NoWarn&gt;RTMIDI001&lt;/NoWarn&gt;</c>) to use the API.
    /// </summary>
    public const string DiagnosticId = "RTMIDI001";

    private const byte Null7Bit = 0x7F;
    private const byte ExpressionAfterReset = 127;
    private const int ChannelCount = 16;

    private readonly bool[] _isHighResolution = new bool[32];
    private readonly ChannelState[] _channels = new ChannelState[ChannelCount];

    /// <summary>
    /// Creates a decoder with the default <see cref="MidiControllerDecoderOptions"/>.
    /// </summary>
    public MidiControllerDecoder() : this(new MidiControllerDecoderOptions())
    {
    }

    /// <summary>
    /// Creates a decoder with the given options.
    /// </summary>
    /// <param name="options">Which controllers are combined into 14-bit values.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
    public MidiControllerDecoder(MidiControllerDecoderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        foreach (var controller in options.HighResolutionControllers) _isHighResolution[(byte)controller] = true;
        for (var i = 0; i < ChannelCount; i++) _channels[i] = new ChannelState();
    }

    /// <summary>
    /// Follows a received message and returns the value it completes, if any.
    /// </summary>
    /// <param name="message">The next received message, of any type.</param>
    /// <param name="result">The assembled value, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the message completed a value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
    public bool TryDecode(MidiMessage message, [NotNullWhen(true)] out MidiControllerEvent? result)
    {
        ArgumentNullException.ThrowIfNull(message);
        result = null;

        if (message.Type == MidiMessageType.SystemReset)
        {
            Reset();
            return false;
        }

        if (message is not MidiMessageControlChange controlChange || (byte)controlChange.Channel >= ChannelCount)
        {
            return false;
        }

        result = Decode(controlChange.Channel, _channels[(byte)controlChange.Channel], controlChange.ControlNumber,
            controlChange.Value);
        return result is not null;
    }

    /// <summary>
    /// Forgets the state of every channel, as after a System Reset or when the input is reconnected.
    /// </summary>
    public void Reset()
    {
        foreach (var channel in _channels) channel.Clear();
    }

    /// <summary>
    /// Forgets the state of one channel.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="channel"/> is not a MIDI channel.</exception>
    public void Reset(MidiChannel channel)
    {
        if ((byte)channel >= ChannelCount)
        {
            throw new ArgumentOutOfRangeException(nameof(channel), channel, "Not a MIDI channel.");
        }

        _channels[(byte)channel].Clear();
    }

    private MidiControllerEvent? Decode(MidiChannel channel, ChannelState state, byte control, byte value)
    {
        switch ((MidiControlFunction)control)
        {
            case MidiControlFunction.RegisteredParameterNumberMsb:
                state.SelectNumberByte(MidiParameterKind.Registered, msb: value, lsb: null);
                return null;
            case MidiControlFunction.RegisteredParameterNumberLsb:
                state.SelectNumberByte(MidiParameterKind.Registered, msb: null, lsb: value);
                return null;
            case MidiControlFunction.NonRegisteredParameterNumberMsb:
                state.SelectNumberByte(MidiParameterKind.NonRegistered, msb: value, lsb: null);
                return null;
            case MidiControlFunction.NonRegisteredParameterNumberLsb:
                state.SelectNumberByte(MidiParameterKind.NonRegistered, msb: null, lsb: value);
                return null;
            case MidiControlFunction.DataEntry:
                if (!state.TryGetParameter(out var kind, out var parameterMsb, out var parameterLsb)) return null;
                state.DataEntryMsb = value;
                return new MidiParameterChange(channel, kind, parameterMsb, parameterLsb, value, null);
            case MidiControlFunction.DataEntryLsb:
                if (!state.TryGetParameter(out kind, out parameterMsb, out parameterLsb) ||
                    state.DataEntryMsb is not { } dataMsb) return null;
                return new MidiParameterChange(channel, kind, parameterMsb, parameterLsb, dataMsb, value);
            case MidiControlFunction.DataIncrement:
            case MidiControlFunction.DataDecrement:
                if (!state.TryGetParameter(out kind, out parameterMsb, out parameterLsb)) return null;
                var steps = control == (byte)MidiControlFunction.DataIncrement ? 1 : -1;
                return new MidiParameterIncrement(channel, kind, parameterMsb, parameterLsb, steps, value);
            case MidiControlFunction.ResetAllControllers:
                state.ResetAllControllers();
                return null;
        }

        if (control < 32 && _isHighResolution[control])
        {
            state.ControllerMsb[control] = value;
            return new MidiHighResolutionControlChange(channel, (MidiControlFunction)control, value, null);
        }

        if (control is >= 32 and < 64 && _isHighResolution[control - 32] && state.ControllerMsb[control - 32] is { } msb)
        {
            return new MidiHighResolutionControlChange(channel, (MidiControlFunction)(control - 32), msb, value);
        }

        return null;
    }

    private sealed class ChannelState
    {
        private readonly byte?[] _registered = new byte?[2];    // [MSB, LSB] of the RPN
        private readonly byte?[] _nonRegistered = new byte?[2]; // [MSB, LSB] of the NRPN
        private MidiParameterKind? _selectedKind;

        public byte? DataEntryMsb { get; set; }

        public byte?[] ControllerMsb { get; } = new byte?[32];

        public void SelectNumberByte(MidiParameterKind kind, byte? msb, byte? lsb)
        {
            var number = kind == MidiParameterKind.Registered ? _registered : _nonRegistered;
            if (msb is not null) number[0] = msb;
            if (lsb is not null) number[1] = lsb;
            _selectedKind = kind;
            // The Data Entry MSB belongs to the previous selection: an LSB must not complete it.
            DataEntryMsb = null;
        }

        public bool TryGetParameter(out MidiParameterKind kind, out byte msb, out byte lsb)
        {
            kind = _selectedKind.GetValueOrDefault();
            msb = lsb = 0;
            if (_selectedKind is null) return false;

            var number = kind == MidiParameterKind.Registered ? _registered : _nonRegistered;
            if (number[0] is not { } numberMsb || number[1] is not { } numberLsb) return false;
            if (numberMsb == Null7Bit && numberLsb == Null7Bit) return false;

            msb = numberMsb;
            lsb = numberLsb;
            return true;
        }

        // RP-015: the RPN and NRPN numbers become null, modulation goes to 0 and expression to 127; Bank Select,
        // volume, pan and the parameter values are not reset.
        public void ResetAllControllers()
        {
            _registered[0] = _registered[1] = Null7Bit;
            _nonRegistered[0] = _nonRegistered[1] = Null7Bit;
            DataEntryMsb = null;
            ControllerMsb[(int)MidiControlFunction.ModulationWheel] = 0;
            ControllerMsb[(int)MidiControlFunction.Expression] = ExpressionAfterReset;
        }

        public void Clear()
        {
            Array.Clear(_registered);
            Array.Clear(_nonRegistered);
            Array.Clear(ControllerMsb);
            _selectedKind = null;
            DataEntryMsb = null;
        }
    }
}
