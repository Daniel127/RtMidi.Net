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
# Native libraries

The NuGet package includes the native RtMidi library for these platforms, so you don't need to build RtMidi yourself:

| Platform | Runtime identifier | MIDI API |
|:--|:--|:--|
| Windows x64 / x86 / ARM64 | `win-x64`, `win-x86`, `win-arm64` | Windows MultiMedia |
| Linux x64 / ARM64 / ARM32 (Raspberry Pi) | `linux-x64`, `linux-arm64`, `linux-arm` | ALSA |
| macOS (Intel and Apple Silicon) | `osx` | CoreMIDI |

On Linux, the library needs ALSA at runtime (`libasound2`), which is installed on most desktop distributions and on Raspberry Pi OS.

## How they are built

RtMidi is referenced as a git submodule in `external/rtmidi`, pinned to the version this wrapper supports. Clone with `git clone --recursive`, or run `git submodule update --init` in an existing clone.

The `Build` GitHub Actions workflow compiles the library for every platform and packs it under `runtimes/<rid>/native` in the NuGet package. Pushing a `vX.Y.Z` tag publishes that version to nuget.org using [Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing): no API key is stored in the repository. It needs a Trusted Publishing policy on nuget.org for this repository and the `ci-release.yml` workflow, plus a `NUGET_USER` secret with the nuget.org profile name. The Linux builds run in an Ubuntu 20.04 container, so they work with glibc 2.31 or newer.

To pack locally with your own builds, put them in `artifacts/native/<rid>/native/` (or pass `-p:RtMidiNativeDir=<folder>`) before running `dotnet pack`. To build the Linux library on your machine, run `.github/scripts/build-rtmidi-linux.sh artifacts/native/linux-x64/native`.

## How they are tested

After packing, CI installs the package into a small app ([`tests/RtMidi.Net.PackageTest`](tests/RtMidi.Net.PackageTest)) and runs it on every platform. The app loads RtMidi and lists the ports. Where virtual ports exist (macOS, and Linux when the runner has the ALSA sequencer), it also opens a virtual port, connects a `MidiInputClient`, sends Note On/Note Off, checks that both arrive through `OnMessageReceived`, and closes. Windows has no virtual ports without a third-party driver, so it only lists the ports.

To run it locally, pack into `artifacts/nuget` and run the app:

```bash
dotnet pack RtMidi.Net/RtMidi.Net.csproj -c Release -o artifacts/nuget
dotnet run --project tests/RtMidi.Net.PackageTest -- --loopback
```

To update RtMidi, check out the new tag in the submodule and commit it:

```bash
cd external/rtmidi
git fetch --tags
git checkout <tag>
cd ../..
git add external/rtmidi
```
