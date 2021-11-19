using RtMidi.Net.Enums;
using RtMidi.Net.InteropServices;

namespace RtMidi.Net;

public static class MidiManager
{
    public static IReadOnlyCollection<MidiApi> GetAvailableApis() => RtMidiBase.GetCompiledApi();
    public static string GetApiName(MidiApi api) => RtMidiBase.GetApiName(api);

    public static string GetApiDisplayName(MidiApi api) => RtMidiBase.GetApiDisplayName(api);

    public static List<MidiDeviceInfo> GetAvailableDevices()
    {
        using var clientMidiIn = new RtMidiIn();
        using var clientMidiOut = new RtMidiOut();
        return GetAvailableDevicesIntern(clientMidiIn, clientMidiOut);
    }

    public static List<MidiDeviceInfo> GetAvailableDevices(MidiApi api)
    {
        using var clientMidiIn = new RtMidiIn(api, $"MidiManager Client {api} Input");
        using var clientMidiOut = new RtMidiOut(api, $"MidiManager Client {api} Output");
        return GetAvailableDevicesIntern(clientMidiIn, clientMidiOut);
    }

    public static MidiDeviceInfo GetDeviceInfo(uint deviceId, MidiDeviceType deviceType, MidiApi api = MidiApi.Unspecified)
    {
        using RtMidiBase clientMidi = deviceType == MidiDeviceType.Input
            ? new RtMidiIn(api, $"MidiManager Client {api} Input")
            : new RtMidiOut(api, $"MidiManager Client {api} Output");

        var midiDevicesCount = clientMidi.GetPortCount();
        if (deviceId > midiDevicesCount)
        {
            throw new IndexOutOfRangeException(
                $"The number of devices ({midiDevicesCount}) is less than the id ({deviceId}) entered.");
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