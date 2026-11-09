namespace RtMidi.Net.Tests;

public class MidiControlFunctionTests
{
    // Control numbers from the MIDI 1.0 table "Control Changes and Mode Changes" (midi.org).
    [Theory]
    [InlineData(MidiControlFunction.BankSelect, 0)]
    [InlineData(MidiControlFunction.ModulationWheel, 1)]
    [InlineData(MidiControlFunction.DataEntry, 6)]
    [InlineData(MidiControlFunction.ChannelVolume, 7)]
    [InlineData(MidiControlFunction.Pan, 10)]
    [InlineData(MidiControlFunction.Expression, 11)]
    [InlineData(MidiControlFunction.GeneralPurpose4, 19)]
    [InlineData(MidiControlFunction.BankSelectLsb, 32)]
    [InlineData(MidiControlFunction.DataEntryLsb, 38)]
    [InlineData(MidiControlFunction.GeneralPurpose4Lsb, 51)]
    [InlineData(MidiControlFunction.DamperPedal, 64)]
    [InlineData(MidiControlFunction.Hold2, 69)]
    [InlineData(MidiControlFunction.SoundController5, 74)]
    [InlineData(MidiControlFunction.SoundController10, 79)]
    [InlineData(MidiControlFunction.PortamentoControl, 84)]
    [InlineData(MidiControlFunction.HighResolutionVelocityPrefix, 88)]
    [InlineData(MidiControlFunction.Effects1Depth, 91)]
    [InlineData(MidiControlFunction.Effects5Depth, 95)]
    [InlineData(MidiControlFunction.NonRegisteredParameterNumberLsb, 98)]
    [InlineData(MidiControlFunction.RegisteredParameterNumberMsb, 101)]
    [InlineData(MidiControlFunction.AllSoundOff, 120)]
    [InlineData(MidiControlFunction.AllNotesOff, 123)]
    [InlineData(MidiControlFunction.PolyModeOn, 127)]
    public void Value_IsTheControlNumber(MidiControlFunction function, byte number)
    {
        Assert.Equal(number, (byte)function);
        Assert.Equal(number, new MidiMessageControlChange(MidiChannel.Channel01, function, 0).ControlNumber);
    }

    [Fact]
    public void LsbControllers_Are32AboveTheirMsb()
    {
        foreach (var lsb in Enum.GetValues<MidiControlFunction>().Where(f => f.ToString().EndsWith("Lsb") && (byte)f < 64))
        {
            var msb = Enum.Parse<MidiControlFunction>(lsb.ToString()[..^3]);
            Assert.Equal((byte)msb + 32, (byte)lsb);
        }
    }

    [Fact]
    public void EveryNamedValue_IsAValidControlNumber()
    {
        var values = Enum.GetValues<MidiControlFunction>();
        Assert.All(values, value => Assert.InRange((byte)value, 0, 127));
        Assert.Equal(values.Length, values.Distinct().Count());
        // 73 controllers defined by the specification, each with one name.
        Assert.Equal(73, values.Length);
    }
}
