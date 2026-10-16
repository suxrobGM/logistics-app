#!/usr/bin/env bash
# deliver.sh <render.mkv> <out-dir> [gif-src.mp4]
# Web MP4 (loudness-normalized, faststart, AAC) named after the render, 2fps review frames, and
# the README GIF when a grain-free excerpt render is given.
#   npx remotion render Teaser out/teaser-ramp.mkv --props='{"music":"ramp"}' --codec=h264 --crf=16 --audio-codec=pcm-16
#   npx remotion render Teaser out/gif-src.mp4 --frames=720-1439 --props='{"gif":true,"music":"ramp"}' --codec=h264 --crf=14 --muted
set -euo pipefail
src=$1
out=$2
gif_src=${3:-}
name=$(basename "$src"); name=${name%.*}
mkdir -p "$out/review-$name"

# Two-pass loudnorm to web level (-14 LUFS, -1.5 dBTP): measure, then apply linearly so the mix is
# turned up or down as a whole rather than compressed.
stats=$(ffmpeg -hide_banner -i "$src" -vn -af loudnorm=I=-14:TP=-1.5:LRA=11:print_format=json -f null - 2>&1 | sed -n '/^{/,/^}/p')
get() { echo "$stats" | grep "\"$1\"" | sed 's/.*: "\(.*\)".*/\1/'; }
norm="loudnorm=I=-14:TP=-1.5:LRA=11:measured_I=$(get input_i):measured_TP=$(get input_tp):measured_LRA=$(get input_lra):measured_thresh=$(get input_thresh):offset=$(get target_offset):linear=true,aresample=48000"

ffmpeg -v error -y -i "$src" -af "$norm" -c:v libx264 -preset slow -crf 21 -pix_fmt yuv420p -profile:v high \
  -c:a aac -b:a 192k -movflags +faststart "$out/$name.mp4"

if [ -n "$gif_src" ]; then
  ffmpeg -v error -y -i "$gif_src" \
    -vf "fps=15,scale=800:-1:flags=lanczos,split[a][b];[a]palettegen=max_colors=128:stats_mode=diff[p];[b][p]paletteuse=dither=sierra2_4a:diff_mode=rectangle" \
    "$out/teaser.gif"
fi

rm -f "$out/review-$name"/*.jpg
ffmpeg -v error -y -i "$src" -vf "fps=2,scale=960:-1" "$out/review-$name/f%03d.jpg"

for f in "$out/$name.mp4" ${gif_src:+"$out/teaser.gif"}; do
  printf '%s  %s bytes  ' "$f" "$(stat -c %s "$f")"
  ffprobe -v error -show_entries format=duration:stream=width,height -of csv=p=0 "$f" | tr '\n' ' '
  echo
done
ffmpeg -hide_banner -i "$out/$name.mp4" -af ebur128=peak=true:framelog=quiet -f null - 2>&1 | grep -E "^\s+(I|Peak):"
