using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RtMidi.Net.InteropServices;

namespace RtMidi.Net.Tests.Native;

/// <summary>
/// A test that calls the native RtMidi library. It is skipped when the library
/// is not next to the tests (see RtMidiTestLibrary in the project file).
/// </summary>
public sealed class NativeFactAttribute : FactAttribute
{
    public NativeFactAttribute([CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (!NativeRtMidi.IsAvailable)
        {
            Skip = "Native RtMidi library not available (build the tests with -p:RtMidiTestLibrary=...).";
        }
    }
}

internal static class NativeRtMidi
{
    private static readonly Lazy<bool> Available = new(() =>
        NativeLibrary.TryLoad("rtmidi", typeof(RtMidiBase).Assembly, null, out _));

    public static bool IsAvailable => Available.Value;
}
