using System.Diagnostics;
using RtMidi.Net;

namespace RtMidi.Net.PackageTest;

/// <summary>
/// Helpers shared by the loopback and the stress test.
/// </summary>
internal static class Ports
{
    /// <summary>
    /// Finds an input port by name (CoreMIDI may take a moment to publish a new virtual port).
    /// </summary>
    public static async Task<MidiDeviceInfo?> FindInput(string portName)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            var port = MidiManager.GetAvailableDevices()
                .FirstOrDefault(d => d.Type == MidiDeviceType.Input && d.Name.Contains(portName));
            if (port is not null) return port;
            await Task.Delay(100);
        }
        return null;
    }

    public static int Fail(string message)
    {
        Console.Error.WriteLine($"FAILED: {message}");
        return 1;
    }
}
