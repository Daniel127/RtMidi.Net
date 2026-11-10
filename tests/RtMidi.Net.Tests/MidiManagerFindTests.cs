namespace RtMidi.Net.Tests;

public class MidiManagerFindTests
{
    private static readonly MidiDeviceInfo[] AlsaDevices =
    [
        Device("Midi Through:Midi Through Port-0 14:0", 0),
        Device("USB Keyboard:USB Keyboard MIDI 1 20:0", 1),
        Device("loopMIDI Port:loopMIDI Port 128:0", 2),
    ];

    private static readonly MidiDeviceInfo[] WinMmDevices =
    [
        Device("loopMIDI Port 1", 0),
        Device("loopMIDI Port", 1),
        Device("Microsoft GS Wavetable Synth 0", 2),
    ];

    [Theory]
    [InlineData("usb keyboard", 1u)]   // ignores case
    [InlineData("KEYBOARD MIDI", 1u)]
    [InlineData("20:0", 1u)]
    [InlineData("loopmidi", 2u)]
    [InlineData("Midi", 0u)]           // several match: the first one
    public void FindByName_PartialMatch_ReturnsTheFirst(string name, uint expectedPort)
    {
        Assert.Equal(expectedPort, MidiManager.FindByName(AlsaDevices, name)?.Port);
    }

    [Fact]
    public void FindByName_PrefersAnExactMatch()
    {
        Assert.Equal(1u, MidiManager.FindByName(WinMmDevices, "LOOPMIDI PORT")?.Port);
        Assert.Equal(0u, MidiManager.FindByName(WinMmDevices, "loopMIDI Port 1")?.Port);
        Assert.Equal(2u, MidiManager.FindByName(WinMmDevices, "GS Wavetable")?.Port);
    }

    [Fact]
    public void FindByName_NoMatch_ReturnsNull()
    {
        Assert.Null(MidiManager.FindByName(AlsaDevices, "Roland"));
        Assert.Null(MidiManager.FindByName([], "loopMIDI"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Find_InvalidName_Throws(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(() => MidiManager.FindInputDevice(name!));
        Assert.ThrowsAny<ArgumentException>(() => MidiManager.FindOutputDevice(name!));
    }

    private static MidiDeviceInfo Device(string name, uint port) =>
        new(name, port, MidiApi.LinuxAlsa, MidiDeviceType.Input);
}
