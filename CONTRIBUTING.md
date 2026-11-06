# Contributing

Issues and pull requests are welcome.

## Getting the code

RtMidi is a git submodule in `external/rtmidi`, pinned to the version this wrapper supports. Clone with:

```bash
git clone --recursive https://github.com/Daniel127/RtMidi.Net
```

In an existing clone, run `git submodule update --init`.

## Native libraries

CI compiles RtMidi for every supported platform and packs it under `runtimes/<rid>/native` in the NuGet package. The Linux builds run in an Ubuntu 20.04 container, so they work with glibc 2.31 or newer.

To pack locally with your own builds:

1. Put the libraries in `artifacts/native/<rid>/native/`, or pass `-p:RtMidiNativeDir=<folder>`.
2. Run `dotnet pack`.

To build the Linux library on your machine, run `.github/scripts/build-rtmidi-linux.sh artifacts/native/linux-x64/native`. It needs `g++` and `libasound2-dev`.

## Building

You need the .NET 10 SDK (see `global.json`); it builds the .NET 8 and .NET 10 targets.

## Unit tests

[`tests/RtMidi.Net.Tests`](tests/RtMidi.Net.Tests) covers message parsing and encoding, notes and the input polling without any MIDI device:

```bash
dotnet test --project tests/RtMidi.Net.Tests
```

The tests in `Native/` also call the native library. They are skipped unless you pass one; CI uses RtMidi built with its dummy API, which needs no MIDI system:

```bash
.github/scripts/build-rtmidi-dummy.sh artifacts/test-native
dotnet test --project tests/RtMidi.Net.Tests -p:RtMidiTestLibrary=$PWD/artifacts/test-native/librtmidi.so
```

## Testing the package

[`tests/RtMidi.Net.PackageTest`](tests/RtMidi.Net.PackageTest) installs the built package and uses it on every platform in CI:

- It loads RtMidi and lists the ports.
- Where virtual ports exist (macOS, and Linux with the ALSA sequencer), it also runs a loopback. It opens a virtual port, connects a `MidiInputClient`, sends Note On/Note Off, checks that both arrive through `OnMessageReceived`, and closes.
- Windows has no virtual ports without a third-party driver, so it only lists the ports.

To run it locally:

```bash
dotnet pack RtMidi.Net/RtMidi.Net.csproj -c Release -o artifacts/nuget
dotnet run --project tests/RtMidi.Net.PackageTest -f net10.0 -- --loopback
```

## Updating RtMidi

Check out the new tag in the submodule and commit it:

```bash
cd external/rtmidi
git fetch --tags
git checkout <tag>
cd ../..
git add external/rtmidi
```

Check `rtmidi_c.h` for changes to the C API (for example the `RtMidiWrapper` struct) and update `RtMidi.Net/InteropServices` to match.
