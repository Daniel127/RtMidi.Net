using RtMidi.Net.Enums;
using RtMidi.Net.InteropServices;

namespace RtMidi.Net;

/// <summary>
/// Information about RtMidi and the available MIDI ports.
/// </summary>
/// <example>
/// <code>
/// foreach (var (name, port, api, type) in MidiManager.GetAvailableDevices())
///     Console.WriteLine($"{type} {port}: {name} ({MidiManager.GetApiDisplayName(api)})");
///
/// var device = MidiManager.GetAvailableDevices().First(d => d.Type == MidiDeviceType.Input);
/// using var input = new MidiInputClient(device);
/// input.Open();
/// </code>
/// </example>
public static class MidiManager
{
    /// <summary>
    /// MIDI APIs compiled into the native RtMidi library for this platform.
    /// </summary>
    /// <returns>The APIs; there can be more than one.</returns>
    public static IReadOnlyCollection<MidiApi> GetAvailableApis() => RtMidiBase.GetCompiledApi();

    /// <summary>
    /// Short lower-case name of an API (e.g. "alsa"), or an empty string if unknown.
    /// </summary>
    /// <param name="api">The API.</param>
    /// <returns>The name, which does not change between RtMidi versions.</returns>
    public static string GetApiName(MidiApi api) => RtMidiBase.GetApiName(api);

    /// <summary>
    /// Display name of an API (e.g. "ALSA").
    /// </summary>
    /// <param name="api">The API.</param>
    /// <returns>The name, or "Unknown" if the API is unknown.</returns>
    public static string GetApiDisplayName(MidiApi api) => RtMidiBase.GetApiDisplayName(api);

    /// <summary>
    /// Version of the native RtMidi library.
    /// </summary>
    /// <returns>The version, e.g. "6.1.0".</returns>
    public static string GetRtMidiVersion() => RtMidiBase.GetRtMidiVersion();

    /// <summary>
    /// Input and output ports of the default API (RtMidi uses the first compiled API that has ports).
    /// </summary>
    /// <returns>The input ports first, then the output ports.</returns>
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
    /// <param name="api">The API, e.g. <see cref="MidiApi.LinuxAlsa"/>.</param>
    /// <returns>The input ports first, then the output ports.</returns>
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
    /// <param name="deviceId">The port number, from 0 to the number of ports of that type minus one.</param>
    /// <param name="deviceType">Input or output.</param>
    /// <param name="api">The API; <see cref="MidiApi.Unspecified"/> (default) for the default one.</param>
    /// <returns>The port.</returns>
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