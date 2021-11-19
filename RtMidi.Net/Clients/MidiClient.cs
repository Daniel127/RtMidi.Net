using RtMidi.Net.Enums;
using RtMidi.Net.InteropServices;

namespace RtMidi.Net;

public abstract class MidiClient : IDisposable
{
    private readonly MidiDeviceInfo _deviceInfo;
    internal readonly RtMidiBase RtMidiClient;
    private bool disposedValue;

    internal MidiClient(uint deviceId, RtMidiBase rtMidiClient)
    {
        RtMidiClient = rtMidiClient;
        _deviceInfo = new MidiDeviceInfo(
            RtMidiClient.GetPortName(deviceId),
            deviceId,
            RtMidiClient.GetCurrentApi(),
            RtMidiClient is RtMidiIn ? MidiDeviceType.Input : MidiDeviceType.Output);
    }

    internal MidiClient(MidiDeviceInfo deviceInfo, RtMidiBase rtMidiClient)
    {
        _deviceInfo = deviceInfo;
        RtMidiClient = rtMidiClient;
    }

    public void Open()
    {
        if (RtMidiClient.IsPortOpen) return;
        RtMidiClient.OpenPort(_deviceInfo.Port);
    }

    public void OpenVirtual(string virtualPortName)
    {
        RtMidiClient.OpenVirtualPort(virtualPortName);
    }

    public void Close()
    {
        RtMidiClient.ClosePort();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                RtMidiClient.Dispose();
            }
            disposedValue = true;
        }
    }

    ~MidiClient()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}