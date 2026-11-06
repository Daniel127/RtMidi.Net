using RtMidi.Net.InteropServices;

namespace RtMidi.Net.Clients;

/// <summary>
/// Base of the MIDI input and output clients.
/// </summary>
public abstract class MidiClient : IDisposable
{
    private readonly MidiDeviceInfo? _deviceInfo;
    internal readonly RtMidiBase RtMidiClient;
    private bool _disposedValue;

    private protected MidiClient(RtMidiBase rtMidiClient)
    {
        RtMidiClient = rtMidiClient;
    }

    private protected MidiClient(MidiDeviceInfo deviceInfo, RtMidiBase rtMidiClient)
    {
        ArgumentNullException.ThrowIfNull(deviceInfo);
        _deviceInfo = deviceInfo;
        RtMidiClient = rtMidiClient;
    }

    /// <summary>
    /// Opens the device given in the constructor. Does nothing if it is already open.
    /// </summary>
    /// <exception cref="InvalidOperationException">The client was created without a device.</exception>
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

    /// <summary>
    /// Closes the port (device or virtual) if one is open.
    /// </summary>
    public void Close()
    {
        RtMidiClient.ClosePort();
    }

    /// <summary>
    /// Releases the native RtMidi object (which also closes the port).
    /// </summary>
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

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}