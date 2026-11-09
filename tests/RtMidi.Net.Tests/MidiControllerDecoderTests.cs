namespace RtMidi.Net.Tests;

public class MidiControllerDecoderTests
{
    private const MidiChannel Channel = MidiChannel.Channel01;
    private readonly MidiControllerDecoder _decoder = new();

    [Fact]
    public void PitchBendRange_FromTheSpecificationProcedure()
    {
        // Select RPN 0 with 101/100, set ±12 semitones with 6/38, deselect with the null RPN.
        Assert.Null(Feed(Cc(101, 0)));
        Assert.Null(Feed(Cc(100, 0)));
        var msb = Assert.IsType<MidiParameterChange>(Feed(Cc(6, 12)));
        var lsb = Assert.IsType<MidiParameterChange>(Feed(Cc(38, 0)));
        Assert.Null(Feed(Cc(101, 127)));
        Assert.Null(Feed(Cc(100, 127)));

        Assert.Equal(new MidiParameterChange(Channel, MidiParameterKind.Registered, 0, 0, 12, null), msb);
        Assert.False(msb.HasLsb);
        Assert.Equal(12 << 7, msb.Value);
        Assert.Equal(new MidiParameterChange(Channel, MidiParameterKind.Registered, 0, 0, 12, 0), lsb);
        Assert.True(lsb.HasLsb);
        Assert.Equal(MidiRegisteredParameter.PitchBendSensitivity, lsb.RegisteredParameter);

        // After the null parameter, Data Entry does nothing.
        Assert.Null(Feed(Cc(6, 2)));
        Assert.Null(Feed(Cc(96, 0)));
    }

    [Fact]
    public void ParameterNumberBytes_CanArriveInAnyOrder()
    {
        // The specification's own examples send 100 before 101.
        Feed(Cc(100, 3));
        Feed(Cc(101, 0));

        var change = Assert.IsType<MidiParameterChange>(Feed(Cc(6, 9)));
        Assert.Equal(MidiRegisteredParameter.TuningProgramChange, change.RegisteredParameter);
    }

    [Fact]
    public void Parameter_NeedsBothNumberBytes_ThenOneIsEnoughToChangeIt()
    {
        Feed(Cc(101, 0));
        Assert.Null(Feed(Cc(6, 1)));

        Feed(Cc(100, 1));
        Assert.Equal(1, Assert.IsType<MidiParameterChange>(Feed(Cc(6, 64))).Parameter);

        Feed(Cc(100, 2));
        Assert.Equal(2, Assert.IsType<MidiParameterChange>(Feed(Cc(6, 64))).Parameter);
    }

    [Fact]
    public void Nrpn_IsDecodedWithItsNumber()
    {
        Feed(Cc(99, 5));
        Feed(Cc(98, 20));

        var change = Assert.IsType<MidiParameterChange>(Feed(Cc(6, 100)));
        var fine = Assert.IsType<MidiParameterChange>(Feed(Cc(38, 3)));

        Assert.Equal(MidiParameterKind.NonRegistered, change.Kind);
        Assert.Equal((5 << 7) | 20, change.Parameter);
        Assert.Null(change.RegisteredParameter);
        Assert.Equal((100 << 7) | 3, fine.Value);
    }

    [Fact]
    public void DataEntry_AppliesToTheParameterSelectedLast()
    {
        Feed(Cc(101, 0));
        Feed(Cc(100, 0));
        Feed(Cc(99, 1));
        Feed(Cc(98, 2));
        Assert.Equal(MidiParameterKind.NonRegistered, Assert.IsType<MidiParameterChange>(Feed(Cc(6, 1))).Kind);

        // One byte of the RPN selects it again, with the LSB received before.
        Feed(Cc(101, 0));
        var change = Assert.IsType<MidiParameterChange>(Feed(Cc(6, 2)));
        Assert.Equal(MidiRegisteredParameter.PitchBendSensitivity, change.RegisteredParameter);
    }

    [Fact]
    public void DataEntryLsb_NeedsTheMsbOfTheCurrentParameter()
    {
        Feed(Cc(101, 0));
        Feed(Cc(100, 0));
        Assert.Null(Feed(Cc(38, 50))); // no MSB yet
        Feed(Cc(6, 2));

        // Selecting another parameter forgets the Data Entry MSB of the previous one.
        Feed(Cc(100, 1));
        Assert.Null(Feed(Cc(38, 50)));
    }

    [Theory]
    [InlineData(96, 1)]
    [InlineData(97, -1)]
    public void DataIncrementAndDecrement_GiveOneStep(byte control, int steps)
    {
        Feed(Cc(101, 0));
        Feed(Cc(100, 0));

        var increment = Assert.IsType<MidiParameterIncrement>(Feed(Cc(control, 5)));

        Assert.Equal(new MidiParameterIncrement(Channel, MidiParameterKind.Registered, 0, 0, steps, 5), increment);
        Assert.Equal(MidiRegisteredParameter.PitchBendSensitivity, increment.RegisteredParameter);
    }

    [Theory]
    [InlineData(MidiRegisteredParameter.PitchBendSensitivity, MidiParameterStepUnit.Lsb)]
    [InlineData(MidiRegisteredParameter.ChannelFineTuning, MidiParameterStepUnit.Lsb)]
    [InlineData(MidiRegisteredParameter.ChannelCoarseTuning, MidiParameterStepUnit.Msb)]
    [InlineData(MidiRegisteredParameter.TuningProgramChange, MidiParameterStepUnit.Msb)]
    [InlineData(MidiRegisteredParameter.TuningBankSelect, MidiParameterStepUnit.Msb)]
    [InlineData(MidiRegisteredParameter.ModulationDepthRange, MidiParameterStepUnit.Lsb)]
    public void Increment_StepUnit_FollowsRp018(MidiRegisteredParameter parameter, MidiParameterStepUnit unit)
    {
        var number = (int)parameter;
        var increment = new MidiParameterIncrement(Channel, MidiParameterKind.Registered, (byte)(number >> 7), (byte)(number & 0x7F), 1, 0);

        Assert.Equal(unit, increment.StepUnit);
        Assert.Equal(MidiParameterStepUnit.ManufacturerDefined, (increment with { Kind = MidiParameterKind.NonRegistered }).StepUnit);
    }

    [Fact]
    public void HighResolutionController_EmitsOnMsbAndAgainOnLsb()
    {
        var msb = Assert.IsType<MidiHighResolutionControlChange>(Feed(Cc(7, 100)));
        var lsb = Assert.IsType<MidiHighResolutionControlChange>(Feed(Cc(39, 5)));
        var fine = Assert.IsType<MidiHighResolutionControlChange>(Feed(Cc(39, 6)));

        Assert.Equal(new MidiHighResolutionControlChange(Channel, MidiControlFunction.ChannelVolume, 100, null), msb);
        Assert.Equal(100 << 7, msb.Value);
        Assert.Equal((100 << 7) | 5, lsb.Value);
        Assert.True(lsb.HasLsb);
        Assert.Equal((100 << 7) | 6, fine.Value); // a fine adjustment only needs the LSB
    }

    [Fact]
    public void Lsb_WithoutMsb_GivesNothing()
    {
        Assert.Null(Feed(Cc(39, 5)));
    }

    [Theory]
    [InlineData(0)]   // Bank Select
    [InlineData(32)]
    [InlineData(20)]  // undefined: often an independent 7-bit knob
    [InlineData(52)]
    [InlineData(64)]  // single-byte controllers
    [InlineData(74)]
    public void OtherControllers_GiveNothing(byte control)
    {
        Assert.Null(Feed(Cc(control, 1)));
    }

    [Fact]
    public void Options_ChooseTheHighResolutionControllers()
    {
        var decoder = new MidiControllerDecoder(new MidiControllerDecoderOptions
        {
            HighResolutionControllers = [(MidiControlFunction)20, MidiControlFunction.BankSelect],
        });

        Assert.True(decoder.TryDecode(Cc(20, 1), out _));
        Assert.True(decoder.TryDecode(Cc(52, 2), out var result));
        Assert.Equal((1 << 7) | 2, Assert.IsType<MidiHighResolutionControlChange>(result).Value);
        Assert.True(decoder.TryDecode(Cc(0, 1), out _));
        Assert.False(decoder.TryDecode(Cc(7, 1), out _));
    }

    [Theory]
    [InlineData(32)]
    [InlineData(6)]
    [InlineData(64)]
    public void Options_RejectControllersWithoutLsb(byte control)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MidiControllerDecoderOptions { HighResolutionControllers = [(MidiControlFunction)control] });
    }

    [Fact]
    public void Channels_AreIndependent()
    {
        Feed(Cc(101, 0));
        Feed(Cc(100, 0));
        Feed(Cc(7, 10));

        Assert.False(_decoder.TryDecode(new MidiMessageControlChange(MidiChannel.Channel02, MidiControlFunction.DataEntry, 1), out _));
        Assert.False(_decoder.TryDecode(new MidiMessageControlChange(MidiChannel.Channel02, MidiControlFunction.ChannelVolumeLsb, 1), out _));
        var change = Assert.IsType<MidiParameterChange>(Feed(Cc(6, 1)));
        Assert.Equal(Channel, change.Channel);
    }

    [Fact]
    public void ResetAllControllers_DeselectsTheParameter_ButKeepsVolume()
    {
        Feed(Cc(101, 0));
        Feed(Cc(100, 0));
        Feed(Cc(7, 10));
        Feed(Cc(1, 50));

        Assert.Null(Feed(Cc(121, 0)));

        Assert.Null(Feed(Cc(6, 1)));
        Assert.Equal((10 << 7) | 3, Assert.IsType<MidiHighResolutionControlChange>(Feed(Cc(39, 3))).Value);
        Assert.Equal(3, Assert.IsType<MidiHighResolutionControlChange>(Feed(Cc(33, 3))).Value); // modulation is 0
    }

    [Fact]
    public void SystemReset_AndReset_ForgetEverything()
    {
        Feed(Cc(101, 0));
        Feed(Cc(100, 0));
        Feed(Cc(7, 10));

        Assert.False(_decoder.TryDecode(new MidiMessage(MidiMessageType.SystemReset), out _));
        Assert.Null(Feed(Cc(6, 1)));
        Assert.Null(Feed(Cc(39, 1)));

        Feed(Cc(7, 10));
        _decoder.Reset(MidiChannel.Channel02);
        Assert.NotNull(Feed(Cc(39, 1)));
        _decoder.Reset(Channel);
        Assert.Null(Feed(Cc(39, 1)));
    }

    [Fact]
    public void OtherMessages_GiveNothing()
    {
        Assert.False(_decoder.TryDecode(new MidiMessageProgramChange(Channel, 1), out var result));
        Assert.Null(result);
        Assert.Throws<ArgumentNullException>(() => _decoder.TryDecode(null!, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => _decoder.Reset((MidiChannel)16));
        Assert.Throws<ArgumentNullException>(() => new MidiControllerDecoder(null!));
    }

    [Fact]
    public void EncodedMessages_DecodeToTheSameValues()
    {
        // The last two messages deselect the parameter and give nothing.
        var messages = MidiParameterMessages.SetNonRegisteredParameter(Channel, 1234, 56, 78);
        var decoded = messages.Select(Feed).ToList();
        Assert.Equal([false, false, true, true, false, false], decoded.Select(e => e is not null));
        var last = decoded[3];

        var change = Assert.IsType<MidiParameterChange>(last);
        Assert.Equal(1234, change.Parameter);
        Assert.Equal((56 << 7) | 78, change.Value);

        var volume = MidiParameterMessages.SetHighResolutionController(Channel, MidiControlFunction.ChannelVolume, 12345);
        _decoder.TryDecode(volume[0], out _);
        Assert.True(_decoder.TryDecode(volume[1], out var high));
        Assert.Equal(12345, Assert.IsType<MidiHighResolutionControlChange>(high).Value);
    }

    private MidiControllerEvent? Feed(MidiMessage message) => _decoder.TryDecode(message, out var result) ? result : null;

    private static MidiMessageControlChange Cc(byte control, byte value) => new(Channel, (MidiControlFunction)control, value);
}
