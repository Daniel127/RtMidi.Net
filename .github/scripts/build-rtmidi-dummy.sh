#!/usr/bin/env bash
# Builds librtmidi.so with no MIDI backend, for the unit tests that call the
# native library (tests/RtMidi.Net.Tests/Native). Only RtMidi's dummy API is
# left: it needs no MIDI system, has no ports and accepts every call, so it runs
# on any CI machine. It is only for tests and is never packed.
#
# Usage: build-rtmidi-dummy.sh <output-dir>
set -euo pipefail

out_dir="${1:?output directory required}"
src_dir="$(cd "$(dirname "$0")/../../external/rtmidi" && pwd)"

mkdir -p "$out_dir"
g++ -shared -fPIC -O2 -std=c++11 -DRTMIDI_EXPORT \
  "$src_dir/RtMidi.cpp" "$src_dir/rtmidi_c.cpp" \
  -lpthread -o "$out_dir/librtmidi.so"

echo "Built $out_dir/librtmidi.so (dummy API)"
