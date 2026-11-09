# Migrating from 1.x to 2.0

Most code keeps working after a few renames. The change to watch is the octave numbering (step 4): it compiles
without errors but plays notes one octave higher if you build notes from `MusicOctave` values.

The full list of changes is in the [changelog](CHANGELOG.md).

## 1. Target framework

RtMidi.Net 2.0 targets .NET 8 and .NET 10. Projects on .NET 6 or .NET 7 have to move to .NET 8 or later.

## 2. Native library

The package now includes the native RtMidi library for Windows, Linux (including Raspberry Pi) and macOS.
Remove any `rtmidi.dll`, `librtmidi.so` or `librtmidi.dylib` you copied into your project or output folder,
so the one in the package is used. On Linux, ALSA (`libasound2`) must be installed, as before.

## 3. One namespace

Every public type is now in `RtMidi.Net`. Remove these usings; `using RtMidi.Net;` covers everything:

```csharp
using RtMidi.Net;
using RtMidi.Net.Clients; // remove
using RtMidi.Net.Enums;   // remove
using RtMidi.Net.Events;  // remove
```

Fully qualified names change the same way, e.g. `RtMidi.Net.Enums.MidiDeviceType` is now
`RtMidi.Net.MidiDeviceType`.

## 4. Octaves: middle C is now C4

Octaves follow scientific pitch notation: MIDI note 60 (middle C) is `C4`, and 69 (A440) is `A4`. In 1.x
middle C was `C5`. MIDI note numbers are unchanged, so messages you receive or send as numbers are the same.

What changes:

| | 1.x | 2.0 |
|:--|:--|:--|
| `new MidiNote(60).GetName()` | `C5` | `C4` |
| `new MidiNote(60).Octave` | `MusicOctave.Octave5` | `MusicOctave.Octave4` |
| Lowest octave (notes 0-11) | `Octave0` | `OctaveMinus1` |
| `new MidiNote(MusicNote.C, MusicOctave.Octave4)` | note 48 | note 60 |

If you create notes from a `MusicNote` and a `MusicOctave`, subtract one from the octave to keep the same
notes:

```csharp
// 1.x: middle C
var note = new MidiNote(MusicNote.C, MusicOctave.Octave5);

// 2.0: middle C
var note = new MidiNote(MusicNote.C, MusicOctave.Octave4);
// or
var note = new MidiNote(60);
var note = MidiNote.Parse("C4");
```

If you show note names and want to keep the old numbering, pass the octave of middle C:

```csharp
note.GetName(middleCOctave: 5); // "C5", as in 1.x
note.GetName(middleCOctave: 3); // "C3", as in Yamaha or Ableton
```

## 5. Renamed and removed members

| 1.x | 2.0 |
|:--|:--|
| `midiNote.GetByteRepresentation()` | `midiNote.Number` |
| `pitchBend.Chanel` | `pitchBend.Channel` |
| `input.OnMessageReceived += handler;` + `input.ActivateMessageReceivedEvent();` | `input.MessageReceived += handler;` |
| `input.DeactivateMessageReceivedEvent();` | `input.MessageReceived -= handler;` |
| `catch (IndexOutOfRangeException)` around `MidiManager.GetDeviceInfo` | `catch (ArgumentOutOfRangeException)` |

## 6. Receiving messages with the event

The event is active while it has handlers, so there is no separate step to turn it on. Subscribe before
`Open` so no message goes to the queue in between:

```csharp
// 1.x
input.OnMessageReceived += OnMessage;
input.ActivateMessageReceivedEvent();
input.Open();
// ...
input.DeactivateMessageReceivedEvent();

// 2.0
input.MessageReceived += OnMessage;
input.Open();
// ...
input.MessageReceived -= OnMessage;
```

While the event has no handlers, messages are queued and read with `TryGetMessage`, `GetMessage` or
`GetMessageAsync`, as before.

## 7. Polling for messages

`GetMessage` and `GetMessageAsync` now return the time since the previous message, as reported by RtMidi.
In 1.x they returned how long the call waited. If you need the waiting time, measure it with a `Stopwatch`.

The cancellation token of `GetMessageAsync` is optional, `GetMessage` accepts one too, and the new
`TryGetMessage` returns immediately when the queue is empty.

## 8. Message records

The records that can only have one type no longer take a `Type` argument. Remove it from constructor calls
and positional patterns:

```csharp
// 1.x
var cc = new MidiMessageControlChange(MidiChannel.Channel01, 7, 100, MidiMessageType.ControlChange);
if (message is MidiMessageControlChange(var channel, var controller, var value, _)) { }

// 2.0
var cc = new MidiMessageControlChange(MidiChannel.Channel01, 7, 100);
if (message is MidiMessageControlChange(var channel, var controller, var value)) { }
```

`MidiMessageNote` keeps its `Type`, because it is either `NoteOn` or `NoteOff`. Property patterns such as
`message is MidiMessageNote { Type: MidiMessageType.NoteOn } note` work as before.

`MidiMessageUnknown` no longer takes a type either. To send raw bytes, use `SendMessage` with the bytes.

## 9. Sending messages

`SendMessage` takes a `ReadOnlySpan<byte>` for raw bytes; a `byte[]` still works unchanged. Values that don't
fit MIDI (for example a velocity of 200) now throw `ArgumentOutOfRangeException` instead of being sent wrong.

## 10. Other changes that can affect your code

- `MidiManager.GetAvailableDevices` returns `IReadOnlyList<MidiDeviceInfo>`. If you added or removed items,
  copy it first with `.ToList()`.
- `MidiDeviceInfo` is a record: two instances for the same port compare equal.
- `MidiInputClient`, `MidiOutputClient` and `MidiMessageReceivedEventArgs` are sealed, so they can't be
  inherited. Wrap them instead.
- Errors from the native library throw `RtMidiException`.
- Received messages that are malformed come as `MidiMessageUnknown` instead of throwing, and MIDI Time Code
  Quarter Frames come as `MidiMessageTimeCodeQuarterFrame` instead of throwing `NotImplementedException`.
