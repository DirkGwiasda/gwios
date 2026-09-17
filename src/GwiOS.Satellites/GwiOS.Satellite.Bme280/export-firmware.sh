#!/usr/bin/env bash
# Copies the firmware images built in the VM into dist/, where Flash-Satellite.ps1 picks them up on
# Windows. The build itself cannot live in dist/ or anywhere else in the shared folder: its paths exceed the
# 260-character limit of the Windows host.
#
# Usage: ./export-firmware.sh [build-directory]   (default: ~/build/gwios-satellite-bme280)

set -euo pipefail

project_directory="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
build_directory="${1:-$HOME/build/gwios-satellite-bme280}"
dist_directory="$project_directory/dist"
flasher_args="$build_directory/flasher_args.json"

if [[ ! -f "$flasher_args" ]]; then
    echo "No build found in '$build_directory'. Build first: idf.py -B $build_directory build" >&2
    exit 1
fi

# flasher_args.json lists every image with its path relative to the build directory. Exporting exactly
# those keeps this script correct when the partition layout gains or loses an image.
mapfile -t image_files < <(python3 -c 'import json, sys; print("\n".join(json.load(open(sys.argv[1]))["flash_files"].values()))' "$flasher_args")

rm -rf "$dist_directory"
mkdir -p "$dist_directory"
cp "$flasher_args" "$dist_directory/"

for image_file in "${image_files[@]}"; do
    mkdir -p "$dist_directory/$(dirname "$image_file")"
    cp "$build_directory/$image_file" "$dist_directory/$image_file"
    echo "Exported $image_file"
done

echo "Firmware exported to $dist_directory"
