#!/usr/bin/env bash
# Builds librtmidi.so (ALSA backend) from the external/rtmidi submodule.
# Meant to run inside an old container (ubuntu:20.04) so the binary only needs
# an old glibc (2.31, same as Debian 11 / Raspberry Pi OS 11).
#
# Usage: build-rtmidi-linux.sh <output-dir>
set -euo pipefail

out_dir="${1:?output directory required}"
src_dir="$(cd "$(dirname "$0")/../../external/rtmidi" && pwd)"

if ! command -v g++ >/dev/null || [ ! -f /usr/include/alsa/asoundlib.h ]; then
  apt-get update -q
  DEBIAN_FRONTEND=noninteractive apt-get install -y -q --no-install-recommends g++ libasound2-dev
fi

mkdir -p "$out_dir"
g++ -shared -fPIC -O2 -std=c++11 \
  -D__LINUX_ALSA__ -DRTMIDI_EXPORT \
  -static-libstdc++ -static-libgcc \
  "$src_dir/RtMidi.cpp" "$src_dir/rtmidi_c.cpp" \
  -lasound -lpthread \
  -o "$out_dir/librtmidi.so"
strip --strip-unneeded "$out_dir/librtmidi.so"

echo "Built $out_dir/librtmidi.so"
ldd "$out_dir/librtmidi.so" || true
