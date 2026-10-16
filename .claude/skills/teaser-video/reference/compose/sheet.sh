#!/bin/bash
# Needs a TTF named arial.ttf next to it (drawtext cannot take a Windows path without escaping pain).
# sheet.sh out.png f1 f2 ... -> 2-column sheet of out/preview/f*.jpg (960x540 each)
out=$1; shift; n=$#; args=(); i=0; filt=""; lay=""
for f in "$@"; do args+=(-i "out/preview/f$f.jpg"); filt+="[$i]drawtext=fontfile=arial.ttf:text='$f':x=10:y=10:fontsize=28:fontcolor=yellow:box=1:boxcolor=black@0.6[v$i];"; lay+="$((i%2*960))_$((i/2*540))|"; i=$((i+1)); done
ins=""; for j in $(seq 0 $((n-1))); do ins+="[v$j]"; done
ffmpeg -v error -y "${args[@]}" -filter_complex "${filt}${ins}xstack=inputs=$n:layout=${lay%|}:fill=black" "$out"
