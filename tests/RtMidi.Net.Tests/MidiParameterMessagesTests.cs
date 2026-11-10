namespace RtMidi.Net.Tests;

public class MidiParameterMessagesTests
{
    private const MidiChannel Channel = MidiChannel.Channel02;

    // Parameter numbers from the MIDI 1.0 table "Registered Parameter Numbers" (midi.org): (MSB << 7) | LSB.
    [Theory]
    [InlineData(MidiRegisteredParameter.PitchBendSensitivity, 0x00, 0x00)]
    [InlineData(MidiRegisteredParameter.ChannelFineTuning, 0x00, 0x01)]
    [InlineData(MidiRegisteredParameter.ChannelCoarseTuning, 0x00, 0x02)]
    [InlineData(MidiRegisteredParameter.TuningProgramChange, 0x00, 0x03)]
    [InlineData(MidiRegisteredParameter.TuningBankSelect, 0x00, 0x04)]
    [InlineData(MidiRegisteredParameter.ModulationDepthRange, 0x00, 0x05)]
    [InlineData(MidiRegisteredParameter.MpeConfiguration, 0x00, 0x06)]
    [InlineData(MidiRegisteredParameter.Sound3DAzimuthAngle, 0x3D, 0x00)]
    [InlineData(MidiRegisteredParameter.Sound3DGain, 0x3D, 0x02)]
    [InlineData(MidiRegisteredParameter.Sound3DRollAngle, 0x3D, 0x08)]
    [InlineData(MidiRegisteredParameter.Null, 0x7F, 0x7F)]
    public void RegisteredParameter_IsTheNumberFromTheTable(MidiRegisteredParameter parameter, int msb, int lsb)
    {
        Assert.Equal((msb << 7) | lsb, (int)parameter);
    }

    [Fact]
    public void SetRegisteredParameter_WithLsb_SelectsSetsAndDeselects()
    {
        // Pitch bend range ±12 semitones, 0 cents.
        var messages = MidiParameterMessages.SetRegisteredParameter(Channel, MidiRegisteredParameter.PitchBendSensitivity, 12, 0);

        Assert.Equal(
        [
            Cc(101, 0), Cc(100, 0), Cc(6, 12), Cc(38, 0), Cc(101, 127), Cc(100, 127),
        ], messages);
    }

    [Fact]
    public void SetRegisteredParameter_MsbOnly_HasNoDataEntryLsb()
    {
        var messages = MidiParameterMessages.SetRegisteredParameter(Channel, MidiRegisteredParameter.ChannelCoarseTuning, 0x40);

        Assert.Equal([Cc(101, 0), Cc(100, 2), Cc(6, 0x40), Cc(101, 127), Cc(100, 127)], messages);
    }

    [Fact]
    public void SetRegisteredParameter_3DSound_UsesBothNumberBytes()
    {
        var messages = MidiParameterMessages.SetRegisteredParameter(Channel, MidiRegisteredParameter.Sound3DGain, 1, 2);

        Assert.Equal(Cc(101, 0x3D), messages[0]);
        Assert.Equal(Cc(100, 0x02), messages[1]);
    }

    [Fact]
    public void SetNonRegisteredParameter_UsesControllers99And98_AndDeselectsWithTheNullRpn()
    {
        var messages = MidiParameterMessages.SetNonRegisteredParameter(Channel, (5 << 7) | 20, 100, 3);

        Assert.Equal([Cc(99, 5), Cc(98, 20), Cc(6, 100), Cc(38, 3), Cc(101, 127), Cc(100, 127)], messages);
        Assert.Equal([Cc(99, 0), Cc(98, 1), Cc(6, 7), Cc(101, 127), Cc(100, 127)],
            MidiParameterMessages.SetNonRegisteredParameter(Channel, 1, 7));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(16383)] // the null parameter
    [InlineData(16384)]
    public void Parameters_OutOfRange_Throw(int parameter)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiParameterMessages.SetNonRegisteredParameter(Channel, parameter, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiParameterMessages.SetNonRegisteredParameter(Channel, parameter, 0, 0));
        if (parameter is >= 0 and <= 0xFFFF)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                MidiParameterMessages.SetRegisteredParameter(Channel, (MidiRegisteredParameter)parameter, 0));
        }
    }

    [Fact]
    public void DataBytes_Above127_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MidiParameterMessages.SetRegisteredParameter(Channel, MidiRegisteredParameter.PitchBendSensitivity, 128));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MidiParameterMessages.SetRegisteredParameter(Channel, MidiRegisteredParameter.PitchBendSensitivity, 2, 128));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(16383, 127, 127)]
    [InlineData(8192 + 5, 64, 5)]
    public void SetHighResolutionController_SendsMsbThenLsb(int value, byte msb, byte lsb)
    {
        var messages = MidiParameterMessages.SetHighResolutionController(Channel, MidiControlFunction.ChannelVolume, value);

        Assert.Equal([Cc(7, msb), Cc(39, lsb)], messages);
    }

    [Theory]
    [InlineData(MidiControlFunction.DataEntry)]
    [InlineData(MidiControlFunction.ChannelVolumeLsb)]
    [InlineData(MidiControlFunction.DamperPedal)]
    public void SetHighResolutionController_OnlyAcceptsMsbControllers(MidiControlFunction controller)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiParameterMessages.SetHighResolutionController(Channel, controller, 0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(16384)]
    public void SetHighResolutionController_ValueOutOfRange_Throws(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MidiParameterMessages.SetHighResolutionController(Channel, MidiControlFunction.Pan, value));
    }

    private static MidiMessageControlChange Cc(byte control, byte value) => new(Channel, (MidiControlFunction)control, value);
}
