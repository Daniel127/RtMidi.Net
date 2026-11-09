using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RtMidi.Net.InteropServices;

namespace RtMidi.Net.Tests.Native;

/// <summary>
/// A test that calls the native RtMidi library. It only runs against the dummy-only
/// build of RtMidi (see RtMidiTestLibrary in the project file) and is skipped otherwise,
/// so it never opens real ALSA/CoreMIDI/WinMM ports of a system RtMidi.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class NativeFactAttribute : FactAttribute
{
    public NativeFactAttribute([CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (!NativeRtMidi.IsAvailable)
        {
            Skip = "Dummy-only RtMidi library not available (build the tests with -p:RtMidiTestLibrary=...).";
        }
    }
}

internal static class NativeRtMidi
{
    private static readonly Lazy<bool> Available = new(IsDummyOnlyLibrary);

    private static bool IsDummyOnlyLibrary()
    {
        if (!NativeLibrary.TryLoad("rtmidi", typeof(RtMidiBase).Assembly, null, out _)) return false;

        // The test build has the dummy API as its only compiled API; any other API
        // means a regular RtMidi (e.g. installed on the system) was found instead.
        return RtMidiBase.GetCompiledApi().SequenceEqual([MidiApi.RtMidiDummy]);
    }

    public static bool IsAvailable => Available.Value;
}
