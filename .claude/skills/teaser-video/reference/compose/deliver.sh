#!/usr/bin/env bash
# deliver.sh <render.mp4> <gif-src.mp4> <out-dir>
# Web MP4 (faststart, AAC), README GIF from the grain-free excerpt render, and 2fps review frames.
#   npx remotion render Teaser out/render.mp4 --codec=h264 --crf=16 --audio-codec=aac --audio-bitrate=320k
#   npx remotion render Teaser out/gif-src.mp4 --frames=720-1439 --props='{"gif":true}' --codec=h264 --crf=14 --muted
set -euo pipefail
src=$1
gif_src=$2
out=$3
mkdir -p "$out/review"

ffmpeg -v error -y -i "$src" -c:v libx264 -preset slow -crf 21 -pix_fmt yuv420p -profile:v high \
  -c:a aac -b:a 192k -movflags +faststart "$out/teaser.mp4"

ffmpeg -v error -y -i "$gif_src" \
  -vf "fps=15,scale=800:-1:flags=lanczos,split[a][b];[a]palettegen=max_colors=128:stats_mode=diff[p];[b][p]paletteuse=dither=sierra2_4a:diff_mode=rectangle" \
  "$out/teaser.gif"

rm -f "$out"/review/*.jpg
ffmpeg -v error -y -i "$src" -vf "fps=2,scale=960:-1" "$out/review/f%03d.jpg"

for f in "$out/teaser.mp4" "$out/teaser.gif"; do
  printf '%s  %s bytes  ' "$f" "$(stat -c %s "$f")"
  ffprobe -v error -show_entries format=duration:stream=width,height -of csv=p=0 "$f" | tr '\n' ' '
  echo
done
