using RtMidi.Net.InteropServices;

namespace RtMidi.Net.Clients;

/// <summary>
/// Base of the MIDI input and output clients.
/// </summary>
/// <remarks>
/// Each client owns a native RtMidi object, so dispose it when done (this also closes the port).
/// A client can be opened, closed and opened again.
/// </remarks>
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
    /// <exception cref="RtMidiException">The port could not be opened (e.g. it was disconnected).</exception>
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
    /// <param name="virtualPortName">The name other applications see.</param>
    /// <exception cref="RtMidiException">The API does not support virtual ports, or the port could not be created.</exception>
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
    /// <param name="disposing"><see langword="true"/> when called from <see cref="Dispose()"/>.</param>
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