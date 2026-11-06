#!/usr/bin/env bash
# Builds librtmidi.so with only RtMidi's dummy API, for the unit tests that call
# the native library (tests/RtMidi.Net.Tests/Native). The dummy API needs no MIDI
# system, has no ports and accepts every call, so it runs on any CI machine.
#
# RtMidi 6.0 compiles the dummy classes when no backend is selected but leaves
# them out of its list of compiled APIs, so creating a client fails with
# "no compiled API support found". This builds a patched copy that lists it.
# It is only for tests and is never packed.
#
# Usage: build-rtmidi-dummy.sh <output-dir>
set -euo pipefail

out_dir="${1:?output directory required}"
src_dir="$(cd "$(dirname "$0")/../../external/rtmidi" && pwd)"
work_dir="$(mktemp -d)"
trap 'rm -rf "$work_dir"' EXIT

cp "$src_dir"/RtMidi.cpp "$src_dir"/RtMidi.h "$src_dir"/rtmidi_c.cpp "$src_dir"/rtmidi_c.h "$work_dir"/
# Add RTMIDI_DUMMY before the UNSPECIFIED terminator of rtmidi_compiled_apis[].
perl -0pi -e 's/(rtmidi_compiled_apis\[\] = \{.*?)(\n  RtMidi::UNSPECIFIED,\n\};)/$1\n  RtMidi::RTMIDI_DUMMY,$2/s' "$work_dir/RtMidi.cpp"
grep -q "RtMidi::RTMIDI_DUMMY," "$work_dir/RtMidi.cpp" || { echo "Could not patch rtmidi_compiled_apis" >&2; exit 1; }

mkdir -p "$out_dir"
g++ -shared -fPIC -O2 -std=c++11 -DRTMIDI_EXPORT \
  "$work_dir/RtMidi.cpp" "$work_dir/rtmidi_c.cpp" \
  -lpthread -o "$out_dir/librtmidi.so"

echo "Built $out_dir/librtmidi.so (dummy API)"
