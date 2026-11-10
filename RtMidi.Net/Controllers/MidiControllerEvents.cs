using System.Diagnostics.CodeAnalysis;

namespace RtMidi.Net;

/// <summary>
/// A value that <see cref="MidiControllerDecoder"/> assembled from one or more Control Change messages.
/// </summary>
/// <param name="Channel">The MIDI channel of the Control Changes.</param>
[Experimental(MidiControllerDecoder.DiagnosticId)]
public abstract record MidiControllerEvent(MidiChannel Channel);

/// <summary>
/// A controller with 14-bit resolution: the MSB from a controller 0-31 and, when it has been received, the LSB from
/// the matching controller 32-63.
/// </summary>
/// <remarks>
/// The decoder raises one when the MSB arrives (with no LSB) and again when the LSB arrives, so a device that sends
/// only MSBs still works. While a value moves, the first event of each pair has the LSB at 0 (the specification says
/// receiving an MSB resets the LSB), so a falling value can briefly jump: use <see cref="HasLsb"/> to wait for the
/// complete value when that matters.
/// </remarks>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Controller">The MSB controller, 0 to 31, e.g. <see cref="MidiControlFunction.ChannelVolume"/>.</param>
/// <param name="ValueMsb">The coarse value, 0 to 127.</param>
/// <param name="ValueLsb">The fine value, 0 to 127, or <see langword="null"/> when only the MSB has been received.</param>
[Experimental(MidiControllerDecoder.DiagnosticId)]
public sealed record MidiHighResolutionControlChange(MidiChannel Channel, MidiControlFunction Controller, byte ValueMsb, byte? ValueLsb)
    : MidiControllerEvent(Channel)
{
    /// <summary>The 14-bit value, 0 to 16383: <c>(MSB &lt;&lt; 7) | LSB</c>, with the LSB at 0 if it is missing.</summary>
    public int Value => (ValueMsb << 7) | (ValueLsb ?? 0);

    /// <summary>Whether <see cref="ValueLsb"/> was received.</summary>
    [MemberNotNullWhen(true, nameof(ValueLsb))]
    public bool HasLsb => ValueLsb.HasValue;
}

/// <summary>
/// A registered (RPN) or non-registered (NRPN) parameter received a value with Data Entry (Control Changes 6 and 38).
/// </summary>
/// <remarks>
/// Like <see cref="MidiHighResolutionControlChange"/>, it is raised when the Data Entry MSB arrives and again when the
/// LSB arrives. Many parameters use the two bytes separately rather than as one number: Pitch Bend Sensitivity has
/// semitones in the MSB and cents in the LSB, and Channel Coarse Tuning only uses the MSB.
/// </remarks>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Kind">Registered or non-registered.</param>
/// <param name="ParameterMsb">The parameter number MSB (Control Change 101 or 99).</param>
/// <param name="ParameterLsb">The parameter number LSB (Control Change 100 or 98).</param>
/// <param name="ValueMsb">The Data Entry MSB (Control Change 6), 0 to 127.</param>
/// <param name="ValueLsb">The Data Entry LSB (Control Change 38), or <see langword="null"/> if it has not been received.</param>
[Experimental(MidiControllerDecoder.DiagnosticId)]
public sealed record MidiParameterChange(MidiChannel Channel, MidiParameterKind Kind, byte ParameterMsb, byte ParameterLsb,
    byte ValueMsb, byte? ValueLsb) : MidiControllerEvent(Channel)
{
    /// <summary>The 14-bit parameter number, <c>(MSB &lt;&lt; 7) | LSB</c>.</summary>
    public int Parameter => (ParameterMsb << 7) | ParameterLsb;

    /// <summary>The parameter as a <see cref="MidiRegisteredParameter"/>, or <see langword="null"/> for a non-registered one.</summary>
    public MidiRegisteredParameter? RegisteredParameter =>
        Kind == MidiParameterKind.Registered ? (MidiRegisteredParameter)Parameter : null;

    /// <summary>The 14-bit value, 0 to 16383: <c>(MSB &lt;&lt; 7) | LSB</c>, with the LSB at 0 if it is missing.</summary>
    public int Value => (ValueMsb << 7) | (ValueLsb ?? 0);

    /// <summary>Whether <see cref="ValueLsb"/> was received.</summary>
    [MemberNotNullWhen(true, nameof(ValueLsb))]
    public bool HasLsb => ValueLsb.HasValue;
}

/// <summary>
/// A registered (RPN) or non-registered (NRPN) parameter received Data Increment (Control Change 96) or Data Decrement
/// (97): its value changes by one step, relative to the value the receiver has.
/// </summary>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Kind">Registered or non-registered.</param>
/// <param name="ParameterMsb">The parameter number MSB (Control Change 101 or 99).</param>
/// <param name="ParameterLsb">The parameter number LSB (Control Change 100 or 98).</param>
/// <param name="Steps">+1 for Data Increment, -1 for Data Decrement.</param>
/// <param name="DataByte">The value byte of the Control Change. RP-018 says receivers ignore it, but some devices use it.</param>
[Experimental(MidiControllerDecoder.DiagnosticId)]
public sealed record MidiParameterIncrement(MidiChannel Channel, MidiParameterKind Kind, byte ParameterMsb, byte ParameterLsb,
    int Steps, byte DataByte) : MidiControllerEvent(Channel)
{
    /// <summary>The 14-bit parameter number, <c>(MSB &lt;&lt; 7) | LSB</c>.</summary>
    public int Parameter => (ParameterMsb << 7) | ParameterLsb;

    /// <summary>The parameter as a <see cref="MidiRegisteredParameter"/>, or <see langword="null"/> for a non-registered one.</summary>
    public MidiRegisteredParameter? RegisteredParameter =>
        Kind == MidiParameterKind.Registered ? (MidiRegisteredParameter)Parameter : null;

    /// <summary>
    /// Which part of the value a step changes, according to RP-018: the MSB for RPN 2, 3 and 4, the LSB for the other
    /// registered parameters, and defined by the manufacturer for non-registered ones.
    /// </summary>
    public MidiParameterStepUnit StepUnit => Kind switch
    {
        MidiParameterKind.NonRegistered => MidiParameterStepUnit.ManufacturerDefined,
        _ => (MidiRegisteredParameter)Parameter is MidiRegisteredParameter.ChannelCoarseTuning
            or MidiRegisteredParameter.TuningProgramChange or MidiRegisteredParameter.TuningBankSelect
            ? MidiParameterStepUnit.Msb
            : MidiParameterStepUnit.Lsb,
    };
}
