using RtMidi.Net.Enums;
using RtMidi.Net.InteropServices;

namespace RtMidi.Net.Clients;

public abstract class MidiClient : IDisposable
{
    private readonly MidiDeviceInfo? _deviceInfo;
    internal readonly RtMidiBase RtMidiClient;
    private bool _disposedValue;

    private protected MidiClient(uint deviceId, RtMidiBase rtMidiClient)
    {
        RtMidiClient = rtMidiClient;
        _deviceInfo = new MidiDeviceInfo(
            RtMidiClient.GetPortName(deviceId),
            deviceId,
            RtMidiClient.GetCurrentApi(),
            RtMidiClient is RtMidiIn ? MidiDeviceType.Input : MidiDeviceType.Output);
    }

    private protected MidiClient(RtMidiBase rtMidiClient)
    {
        RtMidiClient = rtMidiClient;
    }

    private protected MidiClient(MidiDeviceInfo deviceInfo, RtMidiBase rtMidiClient)
    {
        _deviceInfo = deviceInfo;
        RtMidiClient = rtMidiClient;
    }

    public void Open()
    {
        if (RtMidiClient.IsPortOpen) return;
        if (_deviceInfo is null)
            throw new InvalidOperationException("This client was created without a device; use OpenVirtual instead.");
        RtMidiClient.OpenPort(_deviceInfo.Port);
    }

    /// <summary>
    /// Creates a virtual MIDI port that other applications (or another client in
    /// this process) can connect to. Supported by CoreMIDI (macOS), ALSA and JACK;
    /// Windows MultiMedia does not support virtual ports.
    /// </summary>
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
        if (!_disposedValue)
        {
            if (disposing)
            {
                RtMidiClient.Dispose();
            }
            _disposedValue = true;
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