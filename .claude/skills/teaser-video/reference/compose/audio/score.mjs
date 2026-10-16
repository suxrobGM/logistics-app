// Music bed for the teaser. 120 BPM: one beat = 0.5s, one bar = 2s.
// Sections are in bars and must match src/timeline.ts.
import {
  SR,
  add,
  adsr,
  biquad,
  buffer,
  fadeOut,
  midiHz,
  mixInto,
  noise,
  normalize,
  reverb,
  sawOsc,
  sineOsc,
  softClip,
  writeWav,
} from "./dsp.mjs";

const BEAT = 0.5;
const BAR = 2;
const BARS = 24;
const LENGTH = BARS * BAR + 1.5;

const CHORDS = {
  Am: { root: 33, pad: [57, 60, 64, 71] },
  F: { root: 29, pad: [53, 57, 60, 67] },
  C: { root: 36, pad: [52, 55, 60, 67] },
  G: { root: 31, pad: [55, 59, 62, 69] },
  E: { root: 28, pad: [56, 59, 64, 68] },
};
const PROGRESSION = [
  "Am",
  "Am",
  "F",
  "E", // cold open, chaos
  "Am",
  "F", // reveal
  "Am",
  "F",
  "C",
  "G",
  "Am",
  "F",
  "C",
  "G",
  "Am",
  "F",
  "C",
  "E", // main groove
  "Am",
  "F", // montage
  "Am",
  "F",
  "Am",
  "Am", // close
];
const chordAt = (bar) => CHORDS[PROGRESSION[Math.min(bar, BARS - 1)]];

const section = (bar) =>
  bar < 2
    ? "open"
    : bar < 4
      ? "chaos"
      : bar < 6
        ? "reveal"
        : bar < 18
          ? "groove"
          : bar < 20
            ? "montage"
            : "close";

const kicks = [];
const dry = buffer(LENGTH);
const drums = buffer(LENGTH);
const sendFx = buffer(LENGTH);
const tonal = buffer(LENGTH);

function kick(at, gain = 1) {
  kicks.push(at);
  const osc = sineOsc(0.25);
  add(
    drums,
    at,
    0.55,
    (t) => {
      const f = 42 + 130 * Math.exp(-t * 28);
      const body = osc(f) * Math.exp(-t * 6.5);
      const click = t < 0.004 ? noise() * (1 - t / 0.004) * 0.5 : 0;
      return Math.tanh((body + click) * 1.6);
    },
    0.72 * gain,
  );
}

function clap(at, gain = 1) {
  const bp = biquad("bp");
  add(
    drums,
    at,
    0.35,
    (t) => {
      const bursts = [0, 0.011, 0.022].reduce(
        (s, o) => s + (t >= o ? Math.exp(-(t - o) * 90) : 0),
        0,
      );
      const tail = Math.exp(-t * 14);
      return bp(noise(), 1500, 1.4) * (bursts * 0.6 + tail);
    },
    0.55 * gain,
    0.05,
  );
  add(sendFx, at, 0.2, (t) => noise() * Math.exp(-t * 20), 0.08 * gain);
}

function hat(at, open = false, gain = 1, pan = 0.25) {
  const hp = biquad("hp");
  const decay = open ? 7 : 55;
  add(
    drums,
    at,
    open ? 0.45 : 0.08,
    (t) => hp(noise(), 8500, 0.9) * Math.exp(-t * decay),
    0.22 * gain,
    pan,
  );
}

function snareHit(at, gain = 1) {
  const bp = biquad("bp");
  const tone = sineOsc();
  add(
    drums,
    at,
    0.22,
    (t) => (bp(noise(), 2200, 0.8) * 0.8 + tone(190) * 0.4) * Math.exp(-t * 22),
    0.35 * gain,
    -0.1,
  );
}

function crash(at, gain = 1) {
  const hp = biquad("hp");
  add(drums, at, 3.5, (t) => hp(noise(), 5000, 0.6) * Math.exp(-t * 1.4), 0.32 * gain);
  add(sendFx, at, 1, (t) => noise() * Math.exp(-t * 4), 0.1 * gain);
}

function impact(at, gain = 1) {
  const sub = sineOsc();
  const lp = biquad("lp");
  add(
    dry,
    at,
    3,
    (t) => {
      const f = 30 + 70 * Math.exp(-t * 7);
      return (
        Math.tanh(sub(f) * Math.exp(-t * 1.3) * 1.8) * 0.9 +
        lp(noise(), 400 + 3000 * Math.exp(-t * 8)) * Math.exp(-t * 3) * 0.6
      );
    },
    0.9 * gain,
  );
  add(sendFx, at, 1.5, (t) => noise() * Math.exp(-t * 3), 0.35 * gain);
}

function riser(at, len, gain = 1) {
  const bp = biquad("bp");
  const s1 = sawOsc(),
    s2 = sawOsc();
  add(
    dry,
    at,
    len,
    (t) => {
      const p = t / len;
      const env = p ** 2.2;
      const f = 300 + 7000 * p ** 2;
      const tone = (s1(110 * 2 ** (p * 2)) + s2(110.7 * 2 ** (p * 2))) * 0.12;
      return (bp(noise(), f, 1.8) * 0.9 + tone) * env;
    },
    0.5 * gain,
  );
}

function pad(at, len, notes, cutoff, gain = 1, attack = 0.6) {
  notes.forEach((n, k) => {
    const oscs = [sawOsc(), sawOsc(), sawOsc()];
    const det = [-0.11, 0, 0.12];
    const lp = biquad("lp");
    const hz = midiHz(n);
    add(
      tonal,
      at,
      len + 1.2,
      (t) => {
        const v = oscs.reduce((s, o, i) => s + o(hz * 2 ** (det[i] / 12)), 0) / 3;
        const c = typeof cutoff === "function" ? cutoff(at + t) : cutoff;
        return lp(v, c, 0.9) * adsr(t, len, attack, 0.4, 0.85, 1.2);
      },
      0.11 * gain,
      (k / (notes.length - 1)) * 1.2 - 0.6,
    );
  });
}

function bassNote(at, len, midi, gain = 1) {
  const s = sawOsc();
  const sub = sineOsc();
  const lp = biquad("lp");
  const hz = midiHz(midi);
  add(
    tonal,
    at,
    len + 0.05,
    (t) => {
      const env = adsr(t, len, 0.004, 0.12, 0.7, 0.04);
      const v = lp(s(hz * 2), 180 + 900 * Math.exp(-t * 18), 1.1) * 0.55 + sub(hz) * 0.55;
      return Math.tanh(v * 1.4) * env;
    },
    0.42 * gain,
  );
}

function pluck(at, midi, gain = 1, pan = 0) {
  const s1 = sawOsc(),
    s2 = sawOsc();
  const lp = biquad("lp");
  const hz = midiHz(midi);
  add(
    tonal,
    at,
    0.4,
    (t) => {
      const v = (s1(hz) + s2(hz * 1.004)) * 0.5;
      return lp(v, 600 + 5200 * Math.exp(-t * 22), 2) * Math.exp(-t * 9);
    },
    0.2 * gain,
    pan,
  );
  add(sendFx, at, 0.25, (t) => Math.sin(2 * Math.PI * hz * t) * Math.exp(-t * 10), 0.05 * gain);
}

function bell(at, midi, gain = 1, pan = 0) {
  const hz = midiHz(midi);
  add(
    tonal,
    at,
    3,
    (t) => {
      const m = Math.sin(2 * Math.PI * hz * 3.5 * t) * 1.6 * Math.exp(-t * 3);
      return Math.sin(2 * Math.PI * hz * t + m) * Math.exp(-t * 1.6);
    },
    0.12 * gain,
    pan,
  );
  add(sendFx, at, 1, (t) => Math.sin(2 * Math.PI * hz * t) * Math.exp(-t * 3), 0.25 * gain);
}

function tick(at, gain = 1) {
  const hp = biquad("hp");
  add(drums, at, 0.03, (t) => hp(noise(), 6000, 2) * Math.exp(-t * 300), 0.4 * gain, 0.4);
  const s = sineOsc();
  add(drums, at, 0.03, (t) => s(2400) * Math.exp(-t * 200), 0.08 * gain, 0.4);
}

for (let bar = 0; bar < BARS; bar++) {
  const t0 = bar * BAR;
  const sec = section(bar);
  const ch = chordAt(bar);

  if (sec === "open" || sec === "chaos") {
    const closed = (time) => 260 + 900 * Math.max(0, (time - 4) / 4) ** 2;
    pad(
      t0,
      BAR,
      ch.pad.map((n) => n - 12),
      closed,
      0.9,
      bar === 0 ? 1.4 : 0.3,
    );
    for (let e = 0; e < 8; e++)
      bassNote(t0 + e * 0.25, 0.16, ch.root + 12, sec === "open" ? 0.45 : 0.6);
    for (let b = 0; b < 4; b++) tick(t0 + b * BEAT, sec === "open" ? 0.8 : 0.5);
    if (sec === "chaos") {
      for (let s = 0; s < 16; s++)
        hat(t0 + s * 0.125, false, 0.25 + (0.5 * (s / 16) * (bar - 1)) / 2);
      if (bar === 2) [0, 1.5].forEach((o) => kick(t0 + o, 0.7));
      if (bar === 3) {
        const roll = [0, 0.5, 1, 1.25, 1.375, 1.5, 1.5625, 1.625, 1.6875];
        roll.forEach((o, k) => snareHit(t0 + o, 0.35 + k * 0.08));
      }
    }
  }

  if (sec === "reveal") {
    pad(t0, BAR, ch.pad, 2600, 1.1, 0.05);
    pad(
      t0,
      BAR,
      ch.pad.map((n) => n + 12),
      3200,
      0.45,
      0.4,
    );
    bassNote(t0, BAR - 0.1, ch.root, 1);
    kick(t0, 1.1);
    if (bar === 5) {
      kick(t0 + 1, 0.9);
      for (let s = 0; s < 8; s++) hat(t0 + 0.25 + s * 0.25, false, 0.5);
      [0.5, 1.5].forEach((o) => clap(t0 + o, 0.7));
    }
    const motif = bar === 4 ? [69, 72, 76, 79] : [77, 76, 72, 69];
    motif.forEach((m, k) => bell(t0 + k * BEAT, m, 0.7, k % 2 ? 0.3 : -0.3));
  }

  if (sec === "groove" || sec === "montage") {
    const hot = sec === "montage";
    pad(t0, BAR, ch.pad, hot ? 4200 : 2200, hot ? 0.9 : 0.75, 0.15);
    for (let b = 0; b < 4; b++) kick(t0 + b * BEAT);
    [1, 3].forEach((b) => clap(t0 + b * BEAT, hot ? 1 : 0.85));
    for (let e = 0; e < 8; e++) {
      const at = t0 + e * 0.25;
      if (e % 2 === 1) {
        hat(at, hot && e % 4 === 3, 0.9);
        bassNote(at, 0.2, ch.root + 12, 1);
      } else {
        hat(at, false, 0.35, -0.3);
      }
    }
    if (hot) for (let s = 0; s < 16; s++) hat(t0 + s * 0.125 + 0.0625, false, 0.18, 0.5);
    const arpNotes = [...ch.pad, ...ch.pad.map((n) => n + 12)];
    const pattern = [0, 2, 4, 1, 3, 5, 2, 6, 4, 7, 5, 3, 6, 4, 2, 1];
    const arpOn = !(bar >= 10 && bar <= 11);
    if (arpOn)
      pattern.forEach((p, s) =>
        pluck(
          t0 + s * 0.125,
          arpNotes[p] + (hot ? 12 : 0),
          s % 4 === 0 ? 1 : 0.7,
          s % 2 ? 0.35 : -0.35,
        ),
      );
    if (bar >= 10 && bar <= 11)
      [0, 0.75, 1.5].forEach((o, k) =>
        bell(t0 + o, ch.pad[(k + 1) % 4] + 12, 0.6, k % 2 ? 0.4 : -0.4),
      );
    if (bar === 6 || bar === 14 || bar === 18) crash(t0, bar === 18 ? 1 : 0.7);
    if (bar === 13 || bar === 17)
      [1.5, 1.625, 1.75, 1.875].forEach((o, k) => snareHit(t0 + o, 0.5 + k * 0.15));
    if (bar === 19) {
      riser(t0, 1.85, 0.8);
      [1, 1.125, 1.25, 1.375, 1.5, 1.5625, 1.625, 1.6875, 1.75, 1.8125].forEach((o, k) =>
        snareHit(t0 + o, 0.45 + k * 0.07),
      );
    }
  }

  if (sec === "close") {
    const fade = 1 - (bar - 20) / 4;
    pad(t0, BAR, ch.pad, 1800 - (bar - 20) * 300, 1.1 * fade + 0.2, bar === 20 ? 0.02 : 0.5);
    pad(
      t0,
      BAR,
      ch.pad.map((n) => n - 12),
      900,
      0.6 * fade,
      0.3,
    );
    if (bar === 20) {
      impact(t0, 1);
      crash(t0, 1.2);
      kick(t0, 1.2);
      bassNote(t0, 3.5, ch.root, 1);
    }
    if (bar === 21)
      [76, 72, 69].forEach((m, k) => bell(t0 + k * 0.75, m, 0.5 * fade + 0.2, k % 2 ? 0.3 : -0.3));
    if (bar === 22) bell(t0, 69, 0.5, 0);
  }
}

riser(4.0, 3.75, 1);
impact(8.0, 1);
crash(8.0, 1.1);

function sidechain(time) {
  let g = 1;
  for (const k of kicks) {
    const d = time - k;
    if (d >= 0 && d < 0.3) g = Math.min(g, 0.35 + 0.65 * (d / 0.3) ** 0.6);
  }
  return g;
}

const silenceGap = (time) => (time > 7.78 && time < 8 ? 0 : time > 39.8 && time < 40 ? 0.15 : 1);

const tonalWet = reverb(tonal, { room: 0.82, damp: 0.4 });
const fxWet = reverb(sendFx, { room: 0.9, damp: 0.3 });
const drumWet = reverb(drums, { room: 0.6, damp: 0.5 });

const master = buffer(LENGTH);
mixInto(master, tonal, 1, (t) => sidechain(t) * silenceGap(t));
mixInto(master, tonalWet, 0.55, silenceGap);
mixInto(master, drums, 1, silenceGap);
mixInto(master, drumWet, 0.12);
mixInto(master, fxWet, 0.9);
mixInto(master, dry, 1);

const lowcut = [biquad("hp"), biquad("hp")];
for (let ch = 0; ch < 2; ch++)
  for (let i = 0; i < master[ch].length; i++) master[ch][i] = lowcut[ch](master[ch][i], 28);

softClip(master, 1.3);
fadeOut(master, 2.5);
normalize(master, -1);
writeWav(new URL("../public/audio/music.wav", import.meta.url), master);
console.log("music.wav", (master[0].length / SR).toFixed(2), "s");
