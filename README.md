# Status

[SonarDashboardLink]: https://sonarcloud.io/dashboard?id=Daniel127_RtMidi.Net
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=Daniel127_RtMidi.Net&metric=alert_status)][SonarDashboardLink] [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Daniel127_RtMidi.Net&metric=ncloc)][SonarDashboardLink] [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=Daniel127_RtMidi.Net&metric=bugs)][SonarDashboardLink] [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Daniel127_RtMidi.Net&metric=coverage)][SonarDashboardLink] [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Daniel127_RtMidi.Net&metric=sqale_rating)][SonarDashboardLink]

[GitHubBadgeMaster]: https://github.com/Daniel127/RtMidi.Net/actions/workflows/ci-release.yml/badge.svg?branch=main
[GitHubBadgeDevelop]: https://github.com/Daniel127/RtMidi.Net/actions/workflows/ci-release.yml/badge.svg?branch=develop
[GitHubActionsLink]: https://github.com/Daniel127/RtMidi.Net/actions/workflows/ci-release.yml

[NugetUrl]: https://www.nuget.org/packages/RtMidi.Net
[NugetBadge]: https://img.shields.io/nuget/v/RtMidi.Net

[RtMidiUrl]: https://github.com/thestk/rtmidi

| Branch | Build | Deployment |
|:----:|:-------------:|:----:|
| main | [![Build][GitHubBadgeMaster]][GitHubActionsLink] | [![Nuget package][NugetBadge]][NugetUrl] |
| develop | [![Build][GitHubBadgeDevelop]][GitHubActionsLink] | N/A |


# What is it?

This project is a .NET wrapper for the [RtMidi][RtMidiUrl] project.

I have created it to use it in an own project in RaspberryPi with a very basic utility as it is the reading of the notes, nevertheless I have done it thinking of being able to use all the options of the MIDI protocol, although I have not come to test them all.

If you find any error or have any improvement do not hesitate to make a PR.

# How to use?

The package targets .NET 8 and .NET 10.

To connect to a device you can use the ``MidiInputClient`` and ``MidiOutputClient`` classes, ``MidiManager`` has useful methods to know the environment.

There is a project (WorkerTest) to test the key reading but it really receives any kind of MIDI message so don't hesitate to extend it if you need it.

If you subscribe to the ``OnMessageReceived`` event of the ``MidiInputClient`` don't forget to use ``ActivateMessageReceivedEvent`` for this event to work, this is because RtMidi includes a queue to store the events and later you can read the events with ``GetMessage``, if you need to stop reading the events in real time you can use ``DeactivateMessageReceivedEvent`` to deactivate the event.

```csharp
uint devicePort = 0;
var device = MidiManager.GetDeviceInfo(devicePort, RtMidi.Net.Enums.MidiDeviceType.Input);
MidiInputClient midiInputClient = new MidiInputClient(device);
midiInputClient.OnMessageReceived += MidiClient_OnMessageReceived;
midiInputClient.ActivateMessageReceivedEvent();
midiInputClient.Open();

//...wait or do something...

midiInputClient.Close();
midiInputClient.Dispose();
```

Without the event, read the queue with ``TryGetMessage`` (returns immediately), ``GetMessage`` or ``GetMessageAsync`` (wait for the next message). The timestamp is the time since the previous message, as reported by RtMidi.

```csharp
using var input = new MidiInputClient(device);
input.Open();
var (message, timestamp) = await input.GetMessageAsync(cancellationToken);
if (message is MidiMessageNote { Type: MidiMessageType.NoteOn } note)
    Console.WriteLine($"{note.Note.GetName()} velocity {note.Velocity}");
```

## Note names and octaves

``MidiNote`` uses scientific pitch notation (ISO): MIDI note 60 (middle C) is ``C4``, 69 (A440) is ``A4``, 0 is ``C-1`` and 127 is ``G9``. ``MusicOctave`` follows it too (``MusicOctave.Octave4`` is the octave of middle C).

Some software numbers octaves differently (middle C as ``C3`` in Yamaha or Ableton, ``C5`` in others, or ``Do3`` in Spanish conservatories). That only changes the text, so ``GetName`` and ``Parse`` take the octave of middle C (from -10 to 10):

```csharp
var note = new MidiNote(60);
string a = note.GetName();                                      // "C4"
string b = note.GetName(useLatinName: true);                    // "Do4"
string c = note.GetName(useLatinName: true, middleCOctave: 3);  // "Do3"
byte bFlat = MidiNote.Parse("Bb3").Number;                      // 58 (also "A#3", "Sib3", "La#3")
byte middleC = MidiNote.Parse("C3", middleCOctave: 3).Number;   // 60
```

> Changed in 2.0: before, ``MusicOctave`` and ``GetName`` numbered the octaves from MIDI note 0, so middle C was ``C5`` / ``Octave5``. ``MidiNote(MusicNote, MusicOctave)`` now gives a note one octave higher for the same enum value name (``Octave4`` was MIDI 48, now it is 60); MIDI note numbers and bytes are unchanged.
# Native libraries

The NuGet package includes the native RtMidi library for these platforms, so you don't need to build RtMidi yourself:

| Platform | Runtime identifier | MIDI API |
|:--|:--|:--|
| Windows x64 / x86 / ARM64 | `win-x64`, `win-x86`, `win-arm64` | Windows MultiMedia |
| Linux x64 / ARM64 / ARM32 (Raspberry Pi) | `linux-x64`, `linux-arm64`, `linux-arm` | ALSA |
| macOS (Intel and Apple Silicon) | `osx` | CoreMIDI |

On Linux, the library needs ALSA at runtime (`libasound2`), which is installed on most desktop distributions and on Raspberry Pi OS.

If you want to build the library or the native binaries yourself, see [CONTRIBUTING.md](https://github.com/Daniel127/RtMidi.Net/blob/main/CONTRIBUTING.md).
