// Jitter check: proves a locked-off shot is actually locked off.
//
// On a static page, consecutive frames should be near-identical. Synthetic
// camera moves (zoompan) round the crop window to whole pixels every frame, so
// the picture snaps back and forth - which the eye reads as shaking, and this
// measures as a large mean per-pixel delta.
//
//   node jitter.js <segment-name> [atSecond]
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { PNG } from "pngjs";
import { ffmpeg } from "./rig.js";

const HERE = path.dirname(fileURLToPath(import.meta.url));
const SEG = path.join(HERE, "segments");
const TMP = path.join(HERE, "jitter-tmp");

// A locked-off static shot lands well under this; visible shake reads 3+.
const THRESHOLD = 1.0;

async function check(name, atSecond) {
  fs.rmSync(TMP, { recursive: true, force: true });
  fs.mkdirSync(TMP, { recursive: true });

  await ffmpeg(
    [
      "-y",
      "-ss",
      String(atSecond),
      "-i",
      path.join(SEG, `${name}.mp4`),
      "-frames:v",
      "6",
      "-vf",
      "scale=640:-1",
      path.join(TMP, "j%02d.png"),
    ],
    "extract",
  );

  const frames = fs
    .readdirSync(TMP)
    .sort()
    .map((f) => PNG.sync.read(fs.readFileSync(path.join(TMP, f))));

  let worst = 0;
  for (let i = 1; i < frames.length; i++) {
    const a = frames[i - 1].data;
    const b = frames[i].data;
    let sum = 0;
    for (let p = 0; p < a.length; p += 4) sum += Math.abs(a[p] - b[p]);
    worst = Math.max(worst, sum / (a.length / 4));
  }

  fs.rmSync(TMP, { recursive: true, force: true });
  return worst;
}

async function main() {
  const name = process.argv[2];
  const at = Number(process.argv[3] || 1.5);
  if (!name) {
    console.error("usage: node jitter.js <segment-name> [atSecond]");
    process.exit(1);
  }

  const worst = await check(name, at);
  // A shot that genuinely scrolls also exceeds the threshold, so the verdict is
  // only meaningful on a moment you believe is still.
  const verdict =
    worst < THRESHOLD
      ? "still - no jitter"
      : "MOVING - fine if the shot scrolls here, otherwise it is shaking: drop the synthetic camera move";
  console.log(`${name} @${at}s  worst mean frame delta: ${worst.toFixed(3)}  -> ${verdict}`);
}

main().catch((err) => {
  console.error(err.message);
  process.exit(1);
});
