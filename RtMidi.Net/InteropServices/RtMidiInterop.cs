using System.Runtime.InteropServices;
using RtMidi.Net.Enums;

namespace RtMidi.Net.InteropServices;

using RtMidiPtr = IntPtr;

internal static class RtMidiInterop
{
	private const string RtMidiLibrary = "rtmidi";

	#region RtMidi

	/// <summary>
	/// <para>Determine the available compiled MIDI APIs.</para>
	/// <para>
	/// If the given `apis` parameter is null, returns the number of available APIs.
	/// Otherwise, fill the given apis array with the <see cref="MidiApi"/> values.
	/// </para>
	/// </summary>
	/// <param name="apis">An array or a null value.</param>
	/// <param name="apisSize">Number of elements pointed to by apis</param>
	/// <returns>
	/// Number of items needed for apis array if apis==NULL, or
	/// number of items written to apis array otherwise.  A negative
	/// return value indicates an error.
	/// </returns>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern int rtmidi_get_compiled_api (IntPtr apis, uint apisSize);
	
	/// <summary>
	/// <para>Return the name of a specified compiled MIDI API.</para>
	/// <para>
	/// This obtains a short lower-case name used for identification purposes.
	/// This value is guaranteed to remain identical across library versions.
	/// </para>
	/// </summary>
	/// <param name="api">The API</param>
	/// <returns>
	/// The API name
	/// <para>If the API is unknown, this function will return the empty string.</para>
	/// </returns>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr rtmidi_api_name (MidiApi api);
	
	/// <summary>
	/// <para>Return the display name of a specified compiled MIDI API.</para>
	/// <para>
	/// This obtains a long name used for display purposes.
	/// </para>
	/// </summary>
	/// <param name="api">The API</param>
	/// <returns>
	/// The API display name
	/// <para>If the API is unknown, this function will return the empty string.</para>
	/// </returns>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr rtmidi_api_display_name (MidiApi api);
	
	/// <summary>
	/// <para>Return the compiled MIDI API having the given name.</para>
	/// <para>
	/// A case insensitive comparison will check the specified name
	/// against the list of compiled APIs.
	/// </para>
	/// </summary>
	/// <param name="name"></param>
	/// <returns>
	/// Return the one which matches.
	/// <para>On failure, the function returns <see cref="MidiApi.Unspecified"/>.</para>
	/// </returns>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	internal static extern MidiApi rtmidi_compiled_api_by_name (string name);
	
	/// <summary>
	/// Open a MIDI port.
	/// </summary>
	/// <param name="rtMidiObject">Pointer to <see cref="RtMidiBase"/> object</param>
	/// <param name="portNumber">Must be greater than 0</param>
	/// <param name="portName">Name for the application port.</param>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	internal static extern void rtmidi_open_port (RtMidiPtr rtMidiObject, uint portNumber, string portName);
	
	/// <summary>
	/// Creates a virtual MIDI port to which other software applications can connect.
	/// </summary>
	/// <param name="rtMidiObject">Pointer to <see cref="RtMidiBase"/> object</param>
	/// <param name="portName">Name for the application port.</param>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	internal static extern void rtmidi_open_virtual_port (RtMidiPtr rtMidiObject, string portName);
	
	/// <summary>
	/// Close a MIDI connection.
	/// </summary>
	/// <param name="rtMidiObject">Pointer to <see cref="RtMidiBase"/> object</param>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern void rtmidi_close_port (RtMidiPtr rtMidiObject);
	
	/// <summary>
	/// Return the number of available MIDI ports.
	/// </summary>
	/// <param name="rtMidiObject">Pointer to <see cref="RtMidiBase"/> object</param>
	/// <returns>This function returns the number of MIDI ports of the selected API.</returns>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint rtmidi_get_port_count (RtMidiPtr rtMidiObject);
	
	/// <summary>
	/// <para>Access a string identifier for the specified MIDI port number.</para>
	/// <para>
	/// To prevent memory leaks a char buffer must be passed to this function.
	/// NULL can be passed as bufOut parameter, and that will write the required buffer length in the bufLen.
	/// </para>
	/// </summary>
	/// <param name="rtMidiObject">Pointer to <see cref="RtMidiBase"/> object</param>
	/// <param name="portNumber">MIDI port number</param>
	/// <param name="bufOut">OUT value with the name</param>
	/// <param name="bufLen">OUT value with the name size</param>
	/// <returns>
	/// The name of the port with the given Id is returned.
	/// <para>
	/// An empty string is returned if an invalid port specifier
	/// is provided. User code should assume a UTF-8 encoding.
	/// </para>
	/// </returns>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int rtmidi_get_port_name (RtMidiPtr rtMidiObject, uint portNumber, IntPtr bufOut, ref uint bufLen);

	#endregion

	#region RtMidiIn

	/// <summary>
	/// Create a default RtMidiInPtr value, with no initialization.
	/// </summary>
	/// <returns>The RtMidiPtr created</returns>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern RtMidiPtr rtmidi_in_create_default ();
	
	/// <summary>
	/// Create a  RtMidiInPtr value, with given api, clientName and queueSizeLimit.
	/// </summary>
	/// <param name="api">An optional API id can be specified.</param>
	/// <param name="clientName">
	/// An optional client name can be specified. This
	/// will be used to group the ports that are created
	/// by the application.</param>
	/// <param name="queueSizeLimit">
	/// An optional size of the MIDI input queue can be
	/// specified.</param>
	/// <returns>The RtMidiPtr created</returns>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	internal static extern RtMidiPtr rtmidi_in_create (MidiApi api, string clientName, uint queueSizeLimit);
	
	/// <summary>
	/// Free the given RtMidiInPtr
	/// </summary>
	/// <param name="rtMidiObject">Pointer to <see cref="RtMidiIn"/> object</param>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern void rtmidi_in_free (RtMidiPtr rtMidiObject);
	
	/// <summary>
	/// Returns the MIDI API specifier for the given instance of RtMidiIn.
	/// </summary>
	/// <param name="rtMidiObject">Pointer to <see cref="RtMidiIn"/> object</param>
	/// <returns>The <see cref="MidiApi"/> value</returns>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern MidiApi rtmidi_in_get_current_api (RtMidiPtr rtMidiObject);
	
	/// <summary>
	/// Set a callback function to be invoked for incoming MIDI messages.
	/// </summary>
	/// <param name="rtMidiObject">Pointer to <see cref="RtMidiIn"/> object</param>
	/// <param name="callback">A callback function must be given.</param>
	/// <param name="userData">Optionally, a pointer to additional data can be
	/// passed to the callback function whenever it is called.</param>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern void rtmidi_in_set_callback (RtMidiPtr rtMidiObject, RtMidiCallback callback, byte[]? userData);
	
	/// <summary>
	/// Cancel use of the current callback function (if one exists).
	/// </summary>
	/// <param name="rtMidiObject">Pointer to <see cref="RtMidiIn"/> object</param>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern void rtmidi_in_cancel_callback (RtMidiPtr rtMidiObject);
	
	/// <summary>
	/// Specify whether certain MIDI message types should be queued or ignored during input.
	/// <remarks>
	/// By default, MIDI timing and active sensing messages are ignored
	/// during message input because of their relative high data rates.
	/// MIDI sysex messages are ignored by default as well.  Variable
	/// values of "true" imply that the respective message type will be
	/// ignored.
	/// </remarks>
	/// </summary>
	/// <param name="rtMidiObject">Pointer to <see cref="RtMidiIn"/> object</param>
	/// <param name="midiSysex">Ignore midi Sysex messages?</param>
	/// <param name="midiTime">Ignore midi Time messages?</param>
	/// <param name="midiSense">Ignore midi Sense messages?</param>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern void rtmidi_in_ignore_types (RtMidiPtr rtMidiObject, bool midiSysex, bool midiTime, bool midiSense);
	
	/// <summary>
	/// Fill the user-provided array with the data bytes for the next available
	/// MIDI message in the input queue and return the event delta-time in seconds.
	/// </summary>
	/// <param name="rtMidiObject">Pointer to <see cref="RtMidiIn"/> object</param>
	/// <param name="message">Must point to a char* that is already allocated.
	/// SYSEX messages maximum size being 1024, a statically
	/// allocated array could be sufficient.</param>
	/// <param name="size">Is used to return the size of the message obtained.
	/// Must be set to the size of <paramref name="message"/> when calling.</param>
	/// <returns>Timestamp of messages</returns>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern double rtmidi_in_get_message (RtMidiPtr rtMidiObject, IntPtr message, ref int size);

	#endregion

	#region RtMidiOut

	/// <summary>
	/// Create a default RtMidiOutPtr value, with no initialization.
	/// </summary>
	/// <returns>The RtMidiOutPtr created</returns>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern RtMidiPtr rtmidi_out_create_default ();
	
	/// <summary>
	/// Create a RtMidiOutPtr value, with given and clientName.
	/// </summary>
	/// <param name="api">An optional API id can be specified.</param>
	/// <param name="clientName">An optional client name can be specified. This
	/// will be used to group the ports that are created
	/// by the application.</param>
	/// <returns>The RtMidiOutPtr created</returns>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	internal static extern RtMidiPtr rtmidi_out_create (MidiApi api, string clientName);
	
	/// <summary>
	/// Free the given RtMidiOutPtr.
	/// </summary>
	/// <param name="rtMidiObject">Pointer to <see cref="RtMidiOut"/> object</param>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern void rtmidi_out_free (RtMidiPtr rtMidiObject);
	
	/// <summary>
	/// Returns the MIDI API specifier for the given instance of RtMidiOut.
	/// </summary>
	/// <param name="rtMidiObject">Pointer to <see cref="RtMidiOut"/> object</param>
	/// <returns>The <see cref="MidiApi"/> value</returns>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern MidiApi rtmidi_out_get_current_api (RtMidiPtr rtMidiObject);
	
	/// <summary>
	/// Immediately send a single message out an open MIDI output port.
	/// </summary>
	/// <remarks>
	/// An exception is thrown if an error occurs during output or an
	/// output connection was not previously established.
	/// </remarks>
	/// <param name="rtMidiObject">Pointer to <see cref="RtMidiOut"/> object</param>
	/// <param name="message">A pointer to the MIDI message as raw bytes</param>
	/// <param name="length">Length of the MIDI message in bytes</param>
	/// <returns></returns>
	[DllImport(RtMidiLibrary, CallingConvention = CallingConvention.Cdecl)]
	internal static extern int rtmidi_out_send_message (RtMidiPtr rtMidiObject, byte[] message, int length);

	#endregion
	
}