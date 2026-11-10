using System.Diagnostics.CodeAnalysis;

namespace RtMidi.Net;

/// <summary>
/// Options of <see cref="MidiControllerDecoder"/>.
/// </summary>
[Experimental(MidiControllerDecoder.DiagnosticId)]
public sealed class MidiControllerDecoderOptions
{
    /// <summary>
    /// The controllers whose MSB (0-31) and LSB (32-63) are combined into <see cref="MidiHighResolutionControlChange"/>.
    /// </summary>
    /// <remarks>
    /// The default is the controllers 0-31 the specification names, except Bank Select (it takes effect with the next
    /// Program Change, not as a continuous value) and Data Entry (it carries parameter values): 1, 2, 4, 5, 7, 8, 10,
    /// 11, 12, 13, 16, 17, 18 and 19. The undefined numbers are left out because many devices use 3, 9, 14-15, 20-31
    /// and their LSB numbers as independent 7-bit controllers. Control Changes of other controllers produce no event.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A controller is not 0-31, or is Data Entry.</exception>
    public IReadOnlyCollection<MidiControlFunction> HighResolutionControllers
    {
        get => _highResolutionControllers;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            foreach (var controller in value)
            {
                if ((byte)controller > 31 || controller == MidiControlFunction.DataEntry)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), controller,
                        "Only controllers 0 to 31 have an LSB; Data Entry is decoded as a parameter.");
                }
            }

            _highResolutionControllers = [.. value.Distinct()];
        }
    }

    private readonly IReadOnlyCollection<MidiControlFunction> _highResolutionControllers = DefaultHighResolutionControllers;

    private static readonly MidiControlFunction[] DefaultHighResolutionControllers =
    [
        MidiControlFunction.ModulationWheel,
        MidiControlFunction.BreathController,
        MidiControlFunction.FootController,
        MidiControlFunction.PortamentoTime,
        MidiControlFunction.ChannelVolume,
        MidiControlFunction.Balance,
        MidiControlFunction.Pan,
        MidiControlFunction.Expression,
        MidiControlFunction.EffectControl1,
        MidiControlFunction.EffectControl2,
        MidiControlFunction.GeneralPurpose1,
        MidiControlFunction.GeneralPurpose2,
        MidiControlFunction.GeneralPurpose3,
        MidiControlFunction.GeneralPurpose4,
    ];
}
