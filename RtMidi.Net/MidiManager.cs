using RtMidi.Net.Enums;
using RtMidi.Net.InteropServices;

namespace RtMidi.Net;

/// <summary>
/// Information about RtMidi and the available MIDI ports.
/// </summary>
public static class MidiManager
{
    /// <summary>
    /// MIDI APIs compiled into the native RtMidi library.
    /// </summary>
    public static IReadOnlyCollection<MidiApi> GetAvailableApis() => RtMidiBase.GetCompiledApi();

    /// <summary>
    /// Short lower-case name of an API (e.g. "alsa"), or an empty string if unknown.
    /// </summary>
    public static string GetApiName(MidiApi api) => RtMidiBase.GetApiName(api);

    /// <summary>
    /// Display name of an API (e.g. "ALSA").
    /// </summary>
    public static string GetApiDisplayName(MidiApi api) => RtMidiBase.GetApiDisplayName(api);

    /// <summary>
    /// Version of the native RtMidi library.
    /// </summary>
    public static string GetRtMidiVersion() => RtMidiBase.GetRtMidiVersion();

    /// <summary>
    /// Input and output ports of the default API.
    /// </summary>
    /// <exception cref="RtMidiException">The MIDI system could not be initialised.</exception>
    public static List<MidiDeviceInfo> GetAvailableDevices()
    {
        using var clientMidiIn = new RtMidiIn();
        using var clientMidiOut = new RtMidiOut();
        return GetAvailableDevicesIntern(clientMidiIn, clientMidiOut);
    }

    /// <summary>
    /// Input and output ports of the given API.
    /// </summary>
    /// <exception cref="RtMidiException">The MIDI system could not be initialised.</exception>
    public static List<MidiDeviceInfo> GetAvailableDevices(MidiApi api)
    {
        using var clientMidiIn = new RtMidiIn(api, $"MidiManager Client {api} Input");
        using var clientMidiOut = new RtMidiOut(api, $"MidiManager Client {api} Output");
        return GetAvailableDevicesIntern(clientMidiIn, clientMidiOut);
    }

    /// <summary>
    /// Information about one port.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">There is no port with that id.</exception>
    /// <exception cref="RtMidiException">The MIDI system could not be initialised.</exception>
    public static MidiDeviceInfo GetDeviceInfo(uint deviceId, MidiDeviceType deviceType, MidiApi api = MidiApi.Unspecified)
    {
        using RtMidiBase clientMidi = deviceType == MidiDeviceType.Input
            ? new RtMidiIn(api, $"MidiManager Client {api} Input")
            : new RtMidiOut(api, $"MidiManager Client {api} Output");

        var midiDevicesCount = clientMidi.GetPortCount();
        if (deviceId >= midiDevicesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId), deviceId,
                $"There are {midiDevicesCount} {deviceType} devices, so the id must be less than {midiDevicesCount}.");
        }

        var currentApi = clientMidi.GetCurrentApi();
        var deviceName = clientMidi.GetPortName(deviceId);
        return new MidiDeviceInfo(deviceName, deviceId, currentApi, deviceType);
    }

    private static List<MidiDeviceInfo> GetAvailableDevicesIntern(RtMidiIn clientMidiIn, RtMidiOut clientMidiOut)
    {
        var devicesIn = GetAvailableDevicesInfo(clientMidiIn, MidiDeviceType.Input);
        var devicesOut = GetAvailableDevicesInfo(clientMidiOut, MidiDeviceType.Output);
        return devicesIn.Union(devicesOut).ToList();

        static IEnumerable<MidiDeviceInfo> GetAvailableDevicesInfo(RtMidiBase clientMidi,
            MidiDeviceType deviceType)
        {
            var midiDevices = new List<MidiDeviceInfo>();
            var midiDevicesCount = clientMidi.GetPortCount();
            var currentApi = clientMidi.GetCurrentApi();

            for (uint port = 0; port < midiDevicesCount; port++)
            {
                var deviceName = clientMidi.GetPortName(port);
                midiDevices.Add(new MidiDeviceInfo(deviceName, port, currentApi, deviceType));
            }

            return midiDevices;
        }
    }
}