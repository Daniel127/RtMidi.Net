using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Every native signature below is blittable, so the runtime marshaller is not
// needed: LibraryImport generates the stubs at compile time (trimming/AOT safe).
[assembly: DisableRuntimeMarshalling]

namespace RtMidi.Net.InteropServices;

/// <summary>
/// Native functions of the RtMidi C API (<c>rtmidi_c.h</c>, RtMidi 6.1).
/// </summary>
/// <remarks>
/// <para>Types follow the C header exactly:</para>
/// <list type="bullet">
/// <item><c>enum RtMidiApi</c> is a C <c>int</c>, so APIs are passed as <see cref="int"/>
/// (<see cref="Enums.MidiApi"/> is a byte-sized enum).</item>
/// <item><c>bool</c> is the 1-byte C/C++ bool.</item>
/// <item><c>size_t</c> is <see cref="nuint"/>.</item>
/// <item>Strings are UTF-8.</item>
/// <item>Buffers are blittable arrays, pinned by the generated code, so no unsafe code is needed here.</item>
/// </list>
/// </remarks>
internal static partial class RtMidiInterop
{
    private const string RtMidiLibrary = "rtmidi";

    #region RtMidi

    /// <summary>
    /// <para>Determine the available compiled MIDI APIs.</para>
    /// <para>
    /// If <paramref name="apis"/> is null, returns the number of available APIs.
    /// Otherwise, fills <paramref name="apis"/> (an array of C <c>enum RtMidiApi</c>, that is <c>int</c>).
    /// </para>
    /// </summary>
    /// <param name="apis">An array or null.</param>
    /// <param name="apisSize">Number of elements pointed to by apis</param>
    /// <returns>
    /// Number of items needed for apis array if apis==NULL, or
    /// number of items written to apis array otherwise.  A negative
    /// return value indicates an error.
    /// </returns>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int rtmidi_get_compiled_api([Out] int[]? apis, uint apisSize);

    /// <summary>
    /// Return the short lower-case name of a compiled MIDI API, or null if unknown.
    /// </summary>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr rtmidi_api_name(int api);

    /// <summary>
    /// Return the display name of a compiled MIDI API ("Unknown" if unknown).
    /// </summary>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr rtmidi_api_display_name(int api);

    /// <summary>
    /// Return the current RtMidi version.
    /// </summary>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr rtmidi_get_version();

    /// <summary>
    /// Return the compiled MIDI API having the given short name (case-sensitive),
    /// or <c>RTMIDI_API_UNSPECIFIED</c> if none matches.
    /// </summary>
    [LibraryImport(RtMidiLibrary, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int rtmidi_compiled_api_by_name(string name);

    /// <summary>
    /// Open a MIDI port.
    /// </summary>
    /// <param name="device">The RtMidi wrapper</param>
    /// <param name="portNumber">The port number</param>
    /// <param name="portName">Name for the application port. Must not be null.</param>
    [LibraryImport(RtMidiLibrary, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void rtmidi_open_port(IntPtr device, uint portNumber, string portName);

    /// <summary>
    /// Creates a virtual MIDI port to which other software applications can connect.
    /// </summary>
    [LibraryImport(RtMidiLibrary, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void rtmidi_open_virtual_port(IntPtr device, string portName);

    /// <summary>
    /// Close a MIDI connection.
    /// </summary>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void rtmidi_close_port(IntPtr device);

    /// <summary>
    /// Return the number of available MIDI ports.
    /// </summary>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint rtmidi_get_port_count(IntPtr device);

    /// <summary>
    /// <para>Copy the name of a MIDI port into <paramref name="bufOut"/> (UTF-8, null terminated).</para>
    /// <para>If <paramref name="bufOut"/> is null, writes the required buffer length (including the terminator) to <paramref name="bufLen"/>.</para>
    /// </summary>
    /// <returns>-1 on error.</returns>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int rtmidi_get_port_name(IntPtr device, uint portNumber, [Out] byte[]? bufOut, ref int bufLen);

    #endregion

    #region RtMidiIn

    /// <summary>
    /// Create a default RtMidiIn.
    /// </summary>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr rtmidi_in_create_default();

    /// <summary>
    /// Create a RtMidiIn with the given api, client name and queue size limit.
    /// </summary>
    [LibraryImport(RtMidiLibrary, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr rtmidi_in_create(int api, string clientName, uint queueSizeLimit);

    /// <summary>
    /// Free the given RtMidiIn.
    /// </summary>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void rtmidi_in_free(IntPtr device);

    /// <summary>
    /// Returns the MIDI API of the given RtMidiIn.
    /// </summary>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int rtmidi_in_get_current_api(IntPtr device);

    /// <summary>
    /// Set a callback function to be invoked for incoming MIDI messages.
    /// </summary>
    /// <param name="device">The RtMidiIn wrapper</param>
    /// <param name="callback"><c>void (*)(double timeStamp, const unsigned char* message, size_t messageSize, void* userData)</c></param>
    /// <param name="userData">Passed back to the callback.</param>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void rtmidi_in_set_callback(IntPtr device, IntPtr callback, IntPtr userData);

    /// <summary>
    /// Cancel use of the current callback function (if one exists).
    /// </summary>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void rtmidi_in_cancel_callback(IntPtr device);

    /// <summary>
    /// Specify whether certain MIDI message types should be ignored during input
    /// (true means ignored).
    /// </summary>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void rtmidi_in_ignore_types(IntPtr device,
        [MarshalAs(UnmanagedType.U1)] bool midiSysex,
        [MarshalAs(UnmanagedType.U1)] bool midiTime,
        [MarshalAs(UnmanagedType.U1)] bool midiSense);

    /// <summary>
    /// Copy the next available MIDI message in the input queue into <paramref name="message"/>
    /// and return its delta-time in seconds.
    /// </summary>
    /// <param name="device">The RtMidiIn wrapper</param>
    /// <param name="message">Buffer for the message.</param>
    /// <param name="size">In: size of <paramref name="message"/>. Out: size of the message
    /// (0 if the queue is empty). If it is larger than the buffer, nothing is copied, the
    /// message stays queued for the next call and the call fails (returns -1).</param>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double rtmidi_in_get_message(IntPtr device, [Out] byte[] message, ref nuint size);

    #endregion

    #region RtMidiOut

    /// <summary>
    /// Create a default RtMidiOut.
    /// </summary>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr rtmidi_out_create_default();

    /// <summary>
    /// Create a RtMidiOut with the given api and client name.
    /// </summary>
    [LibraryImport(RtMidiLibrary, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr rtmidi_out_create(int api, string clientName);

    /// <summary>
    /// Free the given RtMidiOut.
    /// </summary>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void rtmidi_out_free(IntPtr device);

    /// <summary>
    /// Returns the MIDI API of the given RtMidiOut.
    /// </summary>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int rtmidi_out_get_current_api(IntPtr device);

    /// <summary>
    /// Immediately send a single message out an open MIDI output port.
    /// </summary>
    [LibraryImport(RtMidiLibrary)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int rtmidi_out_send_message(IntPtr device, byte[] message, int length);

    #endregion
}
