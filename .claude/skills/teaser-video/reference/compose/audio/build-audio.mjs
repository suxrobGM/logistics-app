// Downloads the licensed music and sound effects (Mixkit free licenses - neither may be
// redistributed as raw files, so they are fetched here, never committed) and prepares them:
//   public/audio/music-<name>.wav   the edited bed for each track, on the 120 BPM grid
//   public/audio/sfx/<role>.wav     effects trimmed to their onset and peak-normalized
// Needs an ffmpeg on PATH built with librubberband (the gyan.dev "full" build), or FFMPEG=<path>.
import { execFileSync } from "node:child_process";
import fs from "node:fs";

const FFMPEG = process.env.FFMPEG ?? "ffmpeg";
const OUT = new URL("../public/audio/", import.meta.url);
const SRC = new URL("src/", OUT);
const SFX = new URL("sfx/", OUT);
for (const dir of [OUT, SRC, SFX]) fs.mkdirSync(dir, { recursive: true });

const MUSIC_LICENSE = "Mixkit Stock Music Free License - https://mixkit.co/license/#musicFree";
const SFX_LICENSE = "Mixkit Sound Effects Free License - https://mixkit.co/license/#sfxFree";

/**
 * Edits in source seconds, measured with a beat tracker (120.01 and 124.00 BPM). Each piece lands
 * on a downbeat of the video's 120 BPM grid: 8s is the logo reveal, 40s the final hit.
 */
const MUSIC = {
  ramp: {
    title: "Ramp It Up",
    artist: "Ahjay Stelino",
    url: "https://assets.mixkit.co/music/69/69.mp3",
    tempo: 1,
    // The drop brings in a grainy shaker around 6-12 kHz that reads as hiss: pull it down.
    eq: "equalizer=f=9000:t=q:w=0.8:g=-8,highshelf=f=13000:g=-4",
    // Bars 4-23: the quiet intro, its build into the drop at 8s, the lift at 24s. Then the last
    // two bars of the closing phrase and the track's own ending.
    pieces: [
      { from: 8.129, to: 48.129 },
      { from: 92.121, to: 100.121 },
    ],
  },
  forest: {
    title: "Rising Forest",
    artist: "Diego Nava",
    url: "https://assets.mixkit.co/music/471/471.mp3",
    // 124 -> 120 BPM, pitch kept. Values below are in stretched seconds.
    tempo: 120 / 124,
    // The breakdown under the cold open, the re-entry on the logo reveal, then straight through.
    pieces: [{ from: 40.0, to: 88.0 }],
    fadeOut: 1.5,
  },
};

const EFFECTS = {
  swoosh: { id: 3115, title: "Fast transitions swoosh" },
  whoosh: { id: 1490, title: "Fast whoosh transition" },
  impact: { id: 498, title: "Deep heartbeat impact" },
  click: { id: 1113, title: "Mouse click close" },
  key: { id: 2541, title: "Single key press in a laptop" },
  pop: { id: 2356, title: "Dry pop up notification alert" },
  chime: { id: 2867, title: "Confirmation tone" },
  tick: { id: 2568, title: "Cool interface click tone" },
};

/** Silence before the drop and before the final hit, in video seconds. */
const GAPS = [
  [7.78, 8.0],
  [39.8, 40.0],
];

function ff(args) {
  execFileSync(FFMPEG, ["-hide_banner", "-v", "error", "-y", ...args], {
    stdio: ["ignore", "inherit", "inherit"],
  });
}

async function download(url, file) {
  if (fs.existsSync(file) && fs.statSync(file).size > 0) return;
  const res = await fetch(url, { headers: { "User-Agent": "Mozilla/5.0" } });
  if (!res.ok) throw new Error(`${url}: HTTP ${res.status}`);
  fs.writeFileSync(file, Buffer.from(await res.arrayBuffer()));
}

const path = (u) => decodeURIComponent(u.pathname.replace(/^\/([A-Za-z]:)/, "$1"));

async function buildMusic(name, m) {
  const src = new URL(`music-${name}.mp3`, SRC);
  await download(m.url, path(src));

  const stretched = new URL(`music-${name}-stretched.wav`, SRC);
  const tempoFilter =
    m.tempo === 1
      ? "anull"
      : `rubberband=tempo=${m.tempo}:transients=crisp:detector=percussive:pitchq=quality:channels=together`;
  // -4 dB of headroom first: both masters peak above 0 dBFS once decoded.
  ff([
    "-i",
    path(src),
    "-af",
    `aresample=48000,${tempoFilter},${m.eq ?? "anull"},volume=-4dB`,
    "-c:a",
    "pcm_f32le",
    path(stretched),
  ]);

  // Map the pieces onto video time, then cut the gaps out as their own regions. Every region
  // boundary gets a fade (30ms out, 3ms in on the downbeat) so no edit or gap clicks.
  const timeline = [];
  let t = 0;
  for (const p of m.pieces) {
    timeline.push({ at: t, from: p.from, to: p.to });
    t += p.to - p.from;
  }
  const total = t;
  const cuts = [0, ...GAPS.flat(), ...timeline.slice(1).map((p) => p.at), total]
    .filter((v, i, a) => a.indexOf(v) === i)
    .sort((a, b) => a - b);
  const regions = [];
  for (let i = 0; i + 1 < cuts.length; i++) {
    const [a, b] = [cuts[i], cuts[i + 1]];
    const silent = GAPS.some(([g0, g1]) => a >= g0 && b <= g1);
    const piece = timeline.findLast((p) => p.at <= a);
    regions.push({ a, b, silent, src: piece.from + (a - piece.at) });
  }
  const filters = regions.map((r, i) => {
    const len = r.b - r.a;
    const body = `[0:a]atrim=start=${r.src.toFixed(4)}:duration=${len.toFixed(4)},asetpts=PTS-STARTPTS`;
    const shape = r.silent
      ? ",volume=0"
      : `${i === 0 ? "" : ",afade=t=in:d=0.003"},afade=t=out:st=${Math.max(0, len - 0.03).toFixed(4)}:d=0.03`;
    return `${body}${shape}[r${i}]`;
  });
  const concat = `${regions.map((_, i) => `[r${i}]`).join("")}concat=n=${regions.length}:v=0:a=1[cat]`;
  const fade = m.fadeOut
    ? `afade=t=out:st=${(total - m.fadeOut).toFixed(3)}:d=${m.fadeOut}`
    : "anull";
  ff([
    "-i",
    path(stretched),
    "-filter_complex",
    `${filters.join(";")};${concat};[cat]${fade}[out]`,
    "-map",
    "[out]",
    "-c:a",
    "pcm_s24le",
    path(new URL(`music-${name}.wav`, OUT)),
  ]);
  console.log(`music-${name}.wav  ${total.toFixed(2)}s  ${m.title} by ${m.artist}`);
}

async function buildEffect(role, e) {
  const src = new URL(`sfx-${e.id}.wav`, SRC);
  await download(`https://assets.mixkit.co/active_storage/sfx/${e.id}/${e.id}.wav`, path(src));
  // Drop only digital silence at either end (a whoosh swells up from quiet), end on a short fade.
  ff([
    "-i",
    path(src),
    "-af",
    "aresample=48000,silenceremove=start_periods=1:start_threshold=-70dB,areverse,silenceremove=start_periods=1:start_threshold=-70dB,afade=t=in:d=0.02,areverse,loudnorm=I=-20:TP=-3:LRA=11:linear=true,aresample=48000",
    "-ac",
    "2",
    "-c:a",
    "pcm_s24le",
    path(new URL(`${role}.wav`, SFX)),
  ]);
  peaks[role] = peakSeconds(path(new URL(`${role}.wav`, SFX)));
  console.log(`sfx/${role}.wav  peak at ${peaks[role]}s  ${e.title}`);
}

/** Where the effect is loudest, so a cue can land that moment on the cut rather than the file start. */
function peakSeconds(file) {
  const raw = execFileSync(FFMPEG, ["-v", "error", "-i", file, "-ac", "1", "-f", "f32le", "-"], {
    maxBuffer: 1 << 26,
  });
  const x = new Float32Array(raw.buffer, raw.byteOffset, raw.byteLength / 4);
  const win = 480;
  let best = 0;
  let at = 0;
  for (let i = 0; i + win <= x.length; i += win) {
    let e = 0;
    for (let j = i; j < i + win; j++) e += x[j] * x[j];
    if (e > best) {
      best = e;
      at = i;
    }
  }
  return +(at / 48000).toFixed(3);
}

const which = process.argv[2];
for (const [name, m] of Object.entries(MUSIC)) {
  if (!which || which === name) await buildMusic(name, m);
}
const peaks = {};
for (const [role, e] of Object.entries(EFFECTS)) await buildEffect(role, e);
fs.writeFileSync(new URL("peaks.json", SFX), JSON.stringify(peaks, null, 2));

const credits = [
  "# Audio credits",
  "",
  ...Object.values(MUSIC).map(
    (m) => `- Music: "${m.title}" by ${m.artist} (${m.url}). ${MUSIC_LICENSE}`,
  ),
  ...Object.values(EFFECTS).map(
    (e) =>
      `- SFX: "${e.title}" (https://mixkit.co/free-sound-effects/, id ${e.id}). ${SFX_LICENSE}`,
  ),
  "",
];
fs.writeFileSync(new URL("CREDITS.md", OUT), credits.join("\n"));
