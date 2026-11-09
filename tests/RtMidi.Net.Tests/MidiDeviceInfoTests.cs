using RtMidi.Net.Enums;

namespace RtMidi.Net.Tests;

public class MidiDeviceInfoTests
{
    [Fact]
    public void SamePort_IsEqual()
    {
        var device = new MidiDeviceInfo("Piano", 1, MidiApi.LinuxAlsa, MidiDeviceType.Input);

        Assert.Equal(new MidiDeviceInfo("Piano", 1, MidiApi.LinuxAlsa, MidiDeviceType.Input), device);
        Assert.NotEqual(new MidiDeviceInfo("Piano", 1, MidiApi.LinuxAlsa, MidiDeviceType.Output), device);
        Assert.NotEqual(new MidiDeviceInfo("Piano", 2, MidiApi.LinuxAlsa, MidiDeviceType.Input), device);

        var (name, port, api, type) = device;
        Assert.Equal(("Piano", 1u, MidiApi.LinuxAlsa, MidiDeviceType.Input), (name, port, api, type));
    }
}
