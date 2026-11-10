# Changelog

All notable changes to RtMidi.Net are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows
[Semantic Versioning](https://semver.org/).

## [2.0.0] - Unreleased

Version 2.0 targets current .NET, ships the native RtMidi libraries in the package, cleans up the API and adds
helpers for the MIDI 1.0 specification (General MIDI, RPN/NRPN, 14-bit controllers, MIDI Time Code and universal
System Exclusive messages). It has breaking changes: see the [migration guide](MIGRATION.md) to update from 1.x.

Two APIs are experimental: their rules or shape may change in a minor version, and using them is a compile error
until you suppress its diagnostic (e.g. `<NoWarn>$(NoWarn);RTMIDI001;RTMIDI002</NoWarn>`):

- `RTMIDI001`: `MidiControllerDecoder`, its options and the events it returns (`MidiControllerEvent`, `MidiParameterChange`,
  `MidiParameterIncrement`, `MidiHighResolutionControlChange`).
- `RTMIDI002`: `MidiTimeCodeReader`, `MidiTimeCodeUpdate` and `MidiTimeCodeSource`.

### Breaking changes

- Targets .NET 8 and .NET 10 (was .NET 6 and .NET 7).
- All public types are in the `RtMidi.Net` namespace: `RtMidi.Net.Clients`, `RtMidi.Net.Enums` and
  `RtMidi.Net.Events` are gone, so one `using RtMidi.Net;` is enough.
- Octaves follow scientific pitch notation: middle C (MIDI note 60) is `C4` / `MusicOctave.Octave4`
  (it was `C5` / `Octave5`). `MusicOctave` now goes from `OctaveMinus1` to `Octave9`; the MIDI note numbers
  are unchanged. `MidiNote.GetName` returns `C4` for note 60.
- `MidiNote.GetByteRepresentation()` is removed; use `MidiNote.Number`.
- `MidiMessagePitchBendChange.Chanel` is removed; use `Channel`.
- `MidiInputClient.OnMessageReceived` is renamed to `MessageReceived`, and it starts raising as soon as it
  has a handler. `ActivateMessageReceivedEvent` and `DeactivateMessageReceivedEvent` are removed: subscribe
  and unsubscribe instead.
- The message records whose type can only be one value (`MidiMessageNoteAfterTouch`,
  `MidiMessageControlChange`, `MidiMessageProgramChange`, `MidiMessageChannelAfterTouch`,
  `MidiMessagePitchBendChange`, `MidiMessageSystemExclusive`, `MidiMessageTimeCodeQuarterFrame`,
  `MidiMessageSongPositionPointer`, `MidiMessageSongSelect` and `MidiMessageUnknown`) no longer take a
  `Type` parameter, so their positional deconstruction has one value less.
- `MidiMessageControlChange.ControlFunction` is a `MidiControlFunction` (was `byte`); the number is in the new
  `ControlNumber` property.
- `MidiOutputClient.SendMessage(byte[])` is now `SendMessage(ReadOnlySpan<byte>)`. Arrays still convert
  implicitly; an empty message (or a null array) throws `ArgumentException`.
- `MidiInputClient.GetMessage` and `GetMessageAsync` return the delta-time reported by RtMidi (time since
  the previous message), not the time spent waiting.
- `MidiManager.GetAvailableDevices` returns `IReadOnlyList<MidiDeviceInfo>` (was `List<MidiDeviceInfo>`).
- `MidiManager.GetDeviceInfo` throws `ArgumentOutOfRangeException` (was `IndexOutOfRangeException`) when the
  port does not exist, including a port equal to the number of ports.
- `MidiDeviceInfo` is a sealed record: two instances for the same port are equal.
- `MidiInputClient`, `MidiOutputClient` and `MidiMessageReceivedEventArgs` are sealed.
- `MidiNote(MusicNote, MusicOctave)` throws `ArgumentOutOfRangeException` for a note above MIDI note 127
  (G9) or an undefined `MusicNote` value.
- Native RtMidi errors throw `RtMidiException` instead of crashing the process.
- Sending a message whose values don't fit MIDI (e.g. a data byte above 127, a channel above 16) throws
  `ArgumentOutOfRangeException`, and a record whose `Type` does not match it throws `ArgumentException`,
  instead of sending wrong bytes.

### Added

- The NuGet package includes the native RtMidi 6.1.0 library for Windows (x64, x86, ARM64), Linux (x64,
  ARM64, ARM32 for Raspberry Pi) and macOS (Intel and Apple Silicon), so RtMidi no longer has to be built
  or installed separately.
- `MidiManager.FindInputDevice` and `FindOutputDevice`, which find a port by part of its name, ignoring case
  (port names and numbers change between platforms and runs; see the README).
- `MidiInputClient()` and `MidiOutputClient()` constructors without a device, for virtual ports
  (`OpenVirtual`).
- `MidiInputClient.TryGetMessage`, which returns immediately. `GetMessage` takes an optional
  `CancellationToken`, the token of `GetMessageAsync` is optional, and both return a named tuple
  (`Message`, `Timestamp`).
- `MidiNote.Number`, `MidiNote.Parse` and `MidiNote.TryParse` (English or Latin names, `#`/`b`, e.g. `C4`,
  `Do#4`, `Bb3`), and the `middleCOctave` option of `GetName` and `Parse` for other octave numberings
  (e.g. 3 for Yamaha, Ableton or `Do3`).
- `MidiNote` equality and comparison (`==`, `<`, `IEquatable<MidiNote>`, `IComparable<MidiNote>`) by note
  number.
- `MidiControlFunction`, the Control Change controllers named as in the MIDI 1.0 specification table (e.g.
  `DamperPedal`, `ChannelVolume`, `AllNotesOff`), and `MidiMessageControlChange.ControlNumber`.
- `MidiMessagePitchBendChange.Bend`, the bend relative to the centre (-8192 to +8191), the `CenterValue` and
  `MaxValue` constants, and the `FromValue` and `FromBend` factories.
- `MidiMessageSongPositionPointer.FromPosition` and `MaxPosition`.
- General MIDI Level 1: `GeneralMidiProgram` (the 128 instruments, as Program Change values),
  `GeneralMidiPercussion` (the 47 drum sounds, as note numbers), `GeneralMidiFamily`, and
  `GeneralMidi.PercussionChannel` and `GeneralMidi.GetFamily`. The names are those of the GM 1 Sound Set.
- Registered and non-registered parameters (RPN/NRPN) and 14-bit controllers: `MidiRegisteredParameter` (the
  registered parameters of the MIDI 1.0 table, e.g. `PitchBendSensitivity`) and `MidiParameterMessages`, which
  builds the Control Change sequences that set them (`SetRegisteredParameter`, `SetNonRegisteredParameter`,
  `SetHighResolutionController`).
- `MidiControllerDecoder` (experimental, `RTMIDI001`), which assembles RPN/NRPN changes, Data Increment/Decrement
  and 14-bit controller values from received Control Changes, following the MIDI 1.0 specification, RP-015 and
  RP-018. `MidiControllerDecoderOptions` chooses the 14-bit controllers.
- `MidiMessageTimeCodeQuarterFrame` for MIDI Time Code Quarter Frame messages (they threw
  `NotImplementedException`).
- MIDI Time Code: `MidiTimeCode` (hours, minutes, seconds, frames and `MidiTimeCodeRate`, including 30 fps
  drop-frame) with `AddFrames`, `ToQuarterFrames` and `ToFullMessage`, and
  `MidiMessageSystemExclusive.TryGetTimeCode` to read a Full Message.
- `MidiTimeCodeReader` (experimental, `RTMIDI002`), which assembles the time from Quarter Frame sequences
  (forward and reverse) and Full Messages.
- System Exclusive helpers: `MidiManufacturerId` (one- and three-byte IDs), the universal header
  (`MidiMessageSystemExclusive.ManufacturerId`, `IsUniversal`, `IsUniversalRealTime`, `DeviceId`, `SubId1`,
  `SubId2`, `AllCallDeviceId`), the `IdentityRequest`, `GeneralMidiSystemOn` and `GeneralMidiSystemOff`
  factories, and
  `TryGetIdentityReply`, which reads the answer as a `MidiIdentityReply`.
- `MidiApi.WindowsMidiServices`, RtMidi's new Windows API (not compiled into the Windows library yet).
- XML documentation for the whole public API, a symbol package (`.snupkg`) with Source Link, and a
  package icon.

### Fixed

- Malformed or truncated incoming messages are returned as `MidiMessageUnknown` instead of throwing.
- `GetMessage` and `GetMessageAsync` no longer busy-wait on a CPU core.
- SysEx messages longer than 64 KB are no longer dropped.
- An exception thrown by a `MessageReceived` handler no longer reaches the native library (it is written to
  `System.Diagnostics.Trace`).
- Disposing an input client while a message is being delivered no longer crashes, and an input client with
  a handler can be garbage collected.
- Using a client after `Dispose` throws `ObjectDisposedException` instead of reaching the freed native
  object. `Close`, `Dispose` and unsubscribing from `MessageReceived` still work after it.
- `MidiMessageSystemExclusive` and `MidiMessageUnknown` compare their type and bytes for equality (they compared
  the array references, so two identical messages were not equal).
- Port names, the list of compiled APIs and opening a port without a name are read correctly from RtMidi.

## [1.3.0] - 2023-09-06

- Updated to RtMidi 6.0.

[2.0.0]: https://github.com/Daniel127/RtMidi.Net/compare/v1.3.0...HEAD
[1.3.0]: https://github.com/Daniel127/RtMidi.Net/releases/tag/v1.3.0
