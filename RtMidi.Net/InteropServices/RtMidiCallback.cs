using System.Runtime.InteropServices;

namespace RtMidi.Net.InteropServices;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void RtMidiCallback(double timestamp, IntPtr message, UIntPtr size, IntPtr userData);