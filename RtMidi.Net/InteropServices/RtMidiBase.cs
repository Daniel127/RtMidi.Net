using RtMidi.Net.Enums;
using System.Runtime.InteropServices;

namespace RtMidi.Net.InteropServices;

/// <summary>
/// RtMidi base
/// </summary>
internal abstract class RtMidiBase : IDisposable
{
    protected readonly IntPtr RtMidiPtr;

    /// <summary>
    /// Check if a port is open.
    /// </summary>
    /// <remarks>
    /// Note that this only applies to connections made with the openPort()
    /// function, not to virtual ports.
    /// </remarks>
    /// <returns>Returns true if a port is open and false if not.</returns>
    public bool IsPortOpen { get; private set; }

    protected RtMidiBase(IntPtr rtMidiPtr)
    {
        RtMidiPtr = rtMidiPtr;
        IsPortOpen = false;
    }

    /// <summary>
    /// Returns the MIDI API specifier for the current instance of RtMidi.
    /// </summary>
    /// <returns></returns>
    public abstract MidiApi GetCurrentApi();

    /// <summary>
    /// Open a MIDI input connection given by enumeration number.
    /// </summary>
    /// <param name="portNumber">An optional port number greater than 0 can be specified.
    /// Otherwise, the default or first port found is opened.</param>
    /// <param name="portName">An optional name for the application port that is used to connect to portId can be specified.</param>
    public void OpenPort(uint portNumber, string? portName = null)
    {
        portName ??= GetPortName(portNumber); //Work around if string is null, TODO Try send IntPtr.Zero
        RtMidiInterop.rtmidi_open_port(RtMidiPtr, portNumber, portName);
        IsPortOpen = true;
    }

    /// <summary>
    /// Create a virtual input port, with optional name, to allow software connections (OS X, JACK and ALSA only).
    /// </summary>
    /// <remarks>
    /// This function creates a virtual MIDI input port to which other
    /// software applications can connect.  This type of functionality
    /// is currently only supported by the Macintosh OS-X, any JACK,
    /// and Linux ALSA APIs (the function returns an error for the other APIs).
    /// </remarks>
    /// <param name="portName">An optional name for the application port that is
    /// used to connect to portId can be specified.</param>
    public void OpenVirtualPort(string portName)
    {
        RtMidiInterop.rtmidi_open_virtual_port(RtMidiPtr, portName);
    }

    /// <summary>
    /// Return the number of available MIDI input ports.
    /// </summary>
    /// <returns>This function returns the number of MIDI ports of the selected API.</returns>
    public uint GetPortCount()
    {
        return RtMidiInterop.rtmidi_get_port_count(RtMidiPtr);
    }

    /// <summary>
    /// Return a string identifier for the specified MIDI input port number.
    /// </summary>
    /// <param name="portNumber">The port number</param>
    /// <param name="nameSize">Optional name size to return</param>
    /// <returns>
    /// The name of the port with the given Id is returned.
    /// <para>An empty string is returned if an invalid port specifier
    /// is provided. User code should assume a UTF-8 encoding.</para>
    /// </returns>
    public string GetPortName(uint portNumber, uint? nameSize = null)
    {
        string? message;
        var messagePtr = IntPtr.Zero;
        var bufferNameSize = 1U;
        int resultInterop;

        if (!nameSize.HasValue)
        {
            resultInterop = RtMidiInterop.rtmidi_get_port_name(RtMidiPtr, portNumber, messagePtr, ref bufferNameSize);
            if (resultInterop == -1) throw new Exception();
        }
        else
        {
            bufferNameSize = nameSize.Value;
        }

        messagePtr = Marshal.AllocHGlobal((int)bufferNameSize);
        try
        {
            resultInterop = RtMidiInterop.rtmidi_get_port_name(RtMidiPtr, portNumber, messagePtr, ref bufferNameSize);
            message = Marshal.PtrToStringUTF8(messagePtr);
            if (resultInterop == -1) throw new Exception();
        }
        finally
        {
            Marshal.FreeHGlobal(messagePtr);
        }

        return message ?? "";
    }

    /// <summary>
    /// Close an open MIDI connection (if one exists).
    /// </summary>
    public void ClosePort()
    {
        RtMidiInterop.rtmidi_close_port(RtMidiPtr);
        IsPortOpen = false;
    }

    #region Dispose

    /// <summary>
    /// Release the unmanaged resources on Dispose
    /// </summary>
    protected abstract void ReleaseUnmanagedResources();

    /// <inheritdoc />
    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    ~RtMidiBase()
    {
        ReleaseUnmanagedResources();
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
        var apis = Array.Empty<byte>();
        var unmanagedPointer = Marshal.AllocHGlobal(apis.Length);
        Marshal.Copy(apis, 0, unmanagedPointer, apis.Length);
        var result = RtMidiInterop.rtmidi_get_compiled_api(unmanagedPointer, sizeof(MidiApi));
        apis = new byte[result];
        Marshal.Copy(unmanagedPointer, apis, 0, result);
        var apisResult = apis.Cast<MidiApi>().ToList();
        Marshal.FreeHGlobal(unmanagedPointer);
        return apisResult;
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
        var ptr = RtMidiInterop.rtmidi_api_name(api);
        var name = Marshal.PtrToStringUTF8(ptr);
        return name ?? string.Empty;
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
        var ptr = RtMidiInterop.rtmidi_api_display_name(api);
        var name = Marshal.PtrToStringUTF8(ptr);
        return name ?? string.Empty;
    }

    /// <summary>
    /// Return the version of the RtMidi library.
    /// </summary>
    /// <returns> The version of the RtMidi library </returns>
    public static string GetRtMidiVersion()
    {
        var ptr = RtMidiInterop.rtmidi_get_version();
        var name = Marshal.PtrToStringUTF8(ptr);
        return name ?? string.Empty;
    }

    /// <summary>
    /// Return the compiled MIDI API having the given name.
    /// </summary>
    /// <remarks>
    /// A case insensitive comparison will check the specified name
    /// against the list of compiled APIs.
    /// </remarks>
    /// <param name="name">The API name</param>
    /// <returns>
    /// Return the one which matches.
    /// On failure, the function returns UNSPECIFIED.
    /// </returns>
    public static MidiApi GetCompiledApiByName(string name)
    {
        return RtMidiInterop.rtmidi_compiled_api_by_name(name);
    }

    #endregion
}