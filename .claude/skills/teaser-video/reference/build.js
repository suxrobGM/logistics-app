// The edit: normalize each take into a fixed-length segment, then crossfade the
// chain into the final cut. Also emits the GIF and the review material.
//
//   node build.js
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { ffmpeg, probe, TAKES } from "./rig.js";

const HERE = path.dirname(fileURLToPath(import.meta.url));
const SEG = path.join(HERE, "segments");
const OUT = path.join(HERE, "out");
fs.mkdirSync(SEG, { recursive: true });
fs.mkdirSync(OUT, { recursive: true });

// Crop the app's persistent sidebar (it carries the account avatar) and land
// exactly on 1080p. Set RAIL_W to 0 when there is nothing to crop.
const RAIL_W = 56;
const RAIL = `crop=1920:1080:${RAIL_W}:0`;
// Cards are filmed at the same width but have centred content.
const CARD = `crop=1920:1080:${Math.round(RAIL_W / 2)}:0`;

// Never add a zoompan push-in here. It quantizes the zoom per frame and the
// picture visibly shakes. Camera movement belongs in the take (cameraScroll).
//
// vf:    filters applied in order
// speed: >1 compresses time (setpts)
// pad:   freeze the last frame when the source is shorter than dur
const SEGMENTS = [
  { name: "s01", src: "c1-hook.mp4", ss: 0.15, dur: 3.6, vf: [CARD] },
  { name: "s02", src: "c2-meet.mp4", ss: 0.15, dur: 2.9, vf: [CARD], pad: true },
  { name: "s03", src: "t1-app.mp4", ss: 1.0, dur: 5.0, vf: [RAIL], speed: 2.4 },
  { name: "s04", src: "t2-detail.mp4", ss: 1.2, dur: 4.6, vf: [RAIL] },
  { name: "s05", src: "c3-close.mp4", ss: 0.3, dur: 4.2, vf: [CARD] },
];

const XFADE = 0.28;
const FPS = 60;

async function buildSegment(s) {
  const out = path.join(SEG, `${s.name}.mp4`);
  const chain = [];
  if (s.speed) chain.push(`setpts=PTS/${s.speed}`);
  chain.push(...s.vf);
  // A settled animation stops producing screencast frames, so the clip can be
  // shorter than the segment - clone the last frame to fill it.
  if (s.pad) chain.push("tpad=stop_mode=clone:stop_duration=6");
  chain.push(`fps=${FPS}`, "format=yuv420p");

  await ffmpeg(
    [
      "-y",
      "-ss",
      String(s.ss),
      "-i",
      path.join(TAKES, s.src),
      "-vf",
      chain.join(","),
      "-t",
      String(s.dur),
      "-an",
      "-c:v",
      "libx264",
      "-preset",
      "medium",
      "-crf",
      "17",
      "-pix_fmt",
      "yuv420p",
      "-r",
      String(FPS),
      out,
    ],
    s.name,
  );
  return out;
}

async function main() {
  const files = [];
  for (const s of SEGMENTS) {
    files.push(await buildSegment(s));
    console.log("built", s.name);
  }

  const total = SEGMENTS.reduce((a, s) => a + s.dur, 0) - XFADE * (SEGMENTS.length - 1);
  const parts = [];
  let prev = "0:v";
  let offset = SEGMENTS[0].dur - XFADE;
  for (let i = 1; i < files.length; i++) {
    parts.push(
      `[${prev}][${i}:v]xfade=transition=fade:duration=${XFADE}:offset=${offset.toFixed(3)}[v${i}]`,
    );
    prev = `v${i}`;
    offset += SEGMENTS[i].dur - XFADE;
  }
  parts.push(`[${prev}]fade=t=in:st=0:d=0.5,fade=t=out:st=${(total - 0.7).toFixed(2)}:d=0.7[vout]`);

  const teaser = path.join(OUT, "teaser.mp4");
  await ffmpeg(
    [
      "-y",
      ...files.flatMap((f) => ["-i", f]),
      "-filter_complex",
      parts.join(";"),
      "-map",
      "[vout]",
      "-c:v",
      "libx264",
      "-preset",
      "slow",
      "-crf",
      "18",
      "-pix_fmt",
      "yuv420p",
      "-profile:v",
      "high",
      "-level",
      "4.2",
      "-movflags",
      "+faststart",
      "-r",
      String(FPS),
      teaser,
    ],
    "xfade",
  );
  console.log("teaser:", teaser, JSON.stringify(await probe(teaser)));

  await buildGif(teaser);
  await buildReview(teaser);
}

/** GitHub will not inline-play a committed mp4, so the README needs a GIF. */
async function buildGif(src, { start = 6, dur = 12, fps = 15, width = 900 } = {}) {
  const palette = path.join(OUT, "palette.png");
  const gif = path.join(OUT, "teaser.gif");
  const filters = `fps=${fps},scale=${width}:-1:flags=lanczos`;

  await ffmpeg(
    [
      "-y",
      "-ss",
      String(start),
      "-t",
      String(dur),
      "-i",
      src,
      "-vf",
      `${filters},palettegen=max_colors=192:stats_mode=diff`,
      palette,
    ],
    "palettegen",
  );
  await ffmpeg(
    [
      "-y",
      "-ss",
      String(start),
      "-t",
      String(dur),
      "-i",
      src,
      "-i",
      palette,
      "-lavfi",
      `${filters}[x];[x][1:v]paletteuse=dither=bayer:bayer_scale=3:diff_mode=rectangle`,
      "-loop",
      "0",
      gif,
    ],
    "paletteuse",
  );

  const mb = fs.statSync(gif).size / 1e6;
  console.log("gif:", gif, `${mb.toFixed(1)} MB`);
  if (mb > 10) console.log("WARNING: over 10 MB - reduce width or fps");
}

/** Full-resolution frames for the PII read, plus a sheet for the overview. */
async function buildReview(src) {
  const dir = path.join(OUT, "frames");
  fs.rmSync(dir, { recursive: true, force: true });
  fs.mkdirSync(dir, { recursive: true });

  await ffmpeg(["-y", "-i", src, "-vf", "fps=1", path.join(dir, "f%02d.png")], "frames");
  await ffmpeg(
    [
      "-y",
      "-i",
      src,
      "-vf",
      "fps=1,scale=480:-1,tile=5x8",
      "-frames:v",
      "1",
      path.join(OUT, "contact-sheet.png"),
    ],
    "sheet",
  );
  console.log("review frames:", fs.readdirSync(dir).length, "in", dir);
  console.log("Read the frames, not just the sheet, before delivering.");
}

main().catch((err) => {
  console.error(err.message);
  process.exit(1);
});
