using RtMidi.Net.Enums;
using System.Runtime.InteropServices;
using System.Text;

namespace RtMidi.Net.InteropServices;

/// <summary>
/// RtMidi base
/// </summary>
internal abstract class RtMidiBase : IDisposable
{
    protected readonly IntPtr RtMidiPtr;
    private bool _disposed;

    /// <summary>
    /// Check if a port is open.
    /// </summary>
    /// <remarks>
    /// Note that this only applies to connections made with the openPort()
    /// function, not to virtual ports.
    /// </remarks>
    /// <returns>Returns true if a port is open and false if not.</returns>
    public bool IsPortOpen { get; private set; }

    /// <summary>
    /// Name given to the application port when <see cref="OpenPort"/> is called
    /// without one (the same default as the C++ API).
    /// </summary>
    protected abstract string DefaultPortName { get; }

    protected RtMidiBase(IntPtr rtMidiPtr)
    {
        RtMidiPtr = rtMidiPtr;
        IsPortOpen = false;
    }

    #region Errors

    // Layout of the native RtMidiWrapper struct (rtmidi_c.h, RtMidi 6.1):
    //   void* ptr; void* callback_proxy; void* error_callback_proxy; bool ok; char* msg;
    // ok is a 1-byte C bool right after the three pointers; msg is pointer-aligned after it.
    private static readonly int OkOffset = 3 * IntPtr.Size;
    private static readonly int MsgOffset = 4 * IntPtr.Size;

    private const string GenericError = "RtMidi reported an error.";

    private static bool IsOk(IntPtr wrapper) => Marshal.ReadByte(wrapper, OkOffset) != 0;

    /// <summary>
    /// Reads the error text of the wrapper. Since RtMidi 6.1 it is a copy owned by
    /// the wrapper, valid until the next call on the same device.
    /// </summary>
    private static string ReadError(IntPtr wrapper)
    {
        var text = Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(wrapper, MsgOffset));
        return string.IsNullOrEmpty(text) ? GenericError : text;
    }

    /// <summary>
    /// Checks the result of an <c>rtmidi_*_create</c> call. If the native object
    /// could not be created (e.g. the MIDI backend is not available), frees the
    /// wrapper and throws, instead of crashing later on a null RtMidi pointer.
    /// </summary>
    protected static IntPtr EnsureCreated(IntPtr rtMidiPtr, Action<IntPtr> free)
    {
        if (rtMidiPtr == IntPtr.Zero) throw new RtMidiException(GenericError);

        if (IsOk(rtMidiPtr)) return rtMidiPtr;

        var error = ReadError(rtMidiPtr);
        free(rtMidiPtr);
        throw new RtMidiException(error);
    }

    /// <summary>
    /// Throws <see cref="RtMidiException"/> if the last native call failed.
    /// </summary>
    /// <remarks>
    /// Only valid right after a C API function that resets the status on entry
    /// (all of them except <c>rtmidi_in_ignore_types</c> and <c>rtmidi_set_error_callback</c>).
    /// </remarks>
    protected void ThrowIfError()
    {
        if (IsOk(RtMidiPtr)) return;
        throw new RtMidiException(ReadError(RtMidiPtr));
    }

    /// <summary>
    /// true if the last native call succeeded (for callers that handle the error themselves).
    /// </summary>
    protected bool LastCallSucceeded => IsOk(RtMidiPtr);

    #endregion

    /// <summary>
    /// Returns the MIDI API specifier for the current instance of RtMidi.
    /// </summary>
    public abstract MidiApi GetCurrentApi();

    /// <summary>
    /// Open a MIDI connection given by enumeration number.
    /// </summary>
    /// <param name="portNumber">The port number.</param>
    /// <param name="portName">An optional name for the application port that is used to connect to portId can be specified.</param>
    public void OpenPort(uint portNumber, string? portName = null)
    {
        // The C API copies the name into a std::string, so it must not be null.
        RtMidiInterop.rtmidi_open_port(RtMidiPtr, portNumber, portName ?? DefaultPortName);
        ThrowIfError();
        IsPortOpen = true;
    }

    /// <summary>
    /// Create a virtual port, to allow software connections (OS X, JACK and ALSA only).
    /// </summary>
    /// <remarks>
    /// This type of functionality is currently only supported by the Macintosh OS-X,
    /// any JACK, and Linux ALSA APIs (the function returns an error for the other APIs).
    /// </remarks>
    /// <param name="portName">The name of the virtual port.</param>
    public void OpenVirtualPort(string portName)
    {
        RtMidiInterop.rtmidi_open_virtual_port(RtMidiPtr, portName);
        ThrowIfError();
    }

    /// <summary>
    /// Return the number of available MIDI ports.
    /// </summary>
    public uint GetPortCount()
    {
        var count = RtMidiInterop.rtmidi_get_port_count(RtMidiPtr);
        ThrowIfError();
        return count;
    }

    /// <summary>
    /// Return a string identifier for the specified MIDI port number.
    /// </summary>
    /// <param name="portNumber">The port number</param>
    /// <returns>
    /// The name of the port with the given Id is returned.
    /// <para>An empty string is returned if an invalid port specifier is provided.</para>
    /// </returns>
    public string GetPortName(uint portNumber)
    {
        var length = 0;
        if (RtMidiInterop.rtmidi_get_port_name(RtMidiPtr, portNumber, null, ref length) < 0)
        {
            ThrowIfError();
            throw new RtMidiException($"Could not read the name of port {portNumber}.");
        }

        if (length <= 1) return string.Empty;

        var buffer = new byte[length];
        if (RtMidiInterop.rtmidi_get_port_name(RtMidiPtr, portNumber, buffer, ref length) < 0)
        {
            ThrowIfError();
            throw new RtMidiException($"Could not read the name of port {portNumber}.");
        }

        var terminator = Array.IndexOf(buffer, (byte)0);
        return Encoding.UTF8.GetString(buffer, 0, terminator < 0 ? buffer.Length : terminator);
    }

    /// <summary>
    /// Close an open MIDI connection (if one exists).
    /// </summary>
    public void ClosePort()
    {
        RtMidiInterop.rtmidi_close_port(RtMidiPtr);
        IsPortOpen = false;
        ThrowIfError();
    }

    #region Dispose

    /// <summary>
    /// Release the unmanaged resources (the native RtMidi object).
    /// </summary>
    protected abstract void ReleaseUnmanagedResources();

    /// <summary>
    /// Dispose pattern.
    /// </summary>
    /// <param name="disposing">true from <see cref="Dispose()"/>, false from the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;
        // RtMidiPtr is zero when construction failed in EnsureCreated (the
        // wrapper was already freed there), so there is nothing to release.
        if (RtMidiPtr != IntPtr.Zero) ReleaseUnmanagedResources();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    ~RtMidiBase()
    {
        Dispose(disposing: false);
    }

    #endregion

    #region Static Functions

    /// <summary>
    /// A static function to determine the available compiled MIDI APIs.
    /// </summary>
    /// <remarks>
    /// Note that there can be more than one
    /// API compiled for certain operating systems.
    /// </remarks>
    /// <returns>Available APIs</returns>
    public static IReadOnlyCollection<MidiApi> GetCompiledApi()
    {
        var count = RtMidiInterop.rtmidi_get_compiled_api(null, 0);
        if (count <= 0) return [];

        // C enum RtMidiApi values are ints.
        var apis = new int[count];
        count = RtMidiInterop.rtmidi_get_compiled_api(apis, (uint)apis.Length);

        return apis.Take(Math.Max(count, 0)).Select(api => (MidiApi)api).ToList();
    }

    /// <summary>
    /// Return the name of a specified compiled MIDI API.
    /// </summary>
    /// <remarks>
    /// This obtains a short lower-case name used for identification purposes.
    /// This value is guaranteed to remain identical across library versions.
    /// </remarks>
    /// <param name="api">The API</param>
    /// <returns>
    /// The lower-case name
    /// <para>If the API is unknown, this function will return the empty string.</para>
    /// </returns>
    public static string GetApiName(MidiApi api)
    {
        return Marshal.PtrToStringUTF8(RtMidiInterop.rtmidi_api_name((int)api)) ?? string.Empty;
    }

    /// <summary>
    /// Return the display name of a specified compiled MIDI API.
    /// </summary>
    /// <remarks>
    /// This obtains a long name used for display purposes.
    /// </remarks>
    /// <param name="api">The API</param>
    /// <returns>
    /// The long name
    /// <para>If the API is unknown, this function will return the empty string.</para>
    /// </returns>
    public static string GetApiDisplayName(MidiApi api)
    {
        return Marshal.PtrToStringUTF8(RtMidiInterop.rtmidi_api_display_name((int)api)) ?? string.Empty;
    }

    /// <summary>
    /// Return the version of the RtMidi library.
    /// </summary>
    /// <returns> The version of the RtMidi library </returns>
    public static string GetRtMidiVersion()
    {
        return Marshal.PtrToStringUTF8(RtMidiInterop.rtmidi_get_version()) ?? string.Empty;
    }

    /// <summary>
    /// Return the compiled MIDI API having the given name.
    /// </summary>
    /// <remarks>
    /// The name is compared with the short names of the compiled APIs
    /// (see <see cref="GetApiName"/>), case-sensitively.
    /// </remarks>
    /// <param name="name">The API name</param>
    /// <returns>
    /// Return the one which matches.
    /// On failure, the function returns UNSPECIFIED.
    /// </returns>
    public static MidiApi GetCompiledApiByName(string name)
    {
        return (MidiApi)RtMidiInterop.rtmidi_compiled_api_by_name(name);
    }

    #endregion
}
