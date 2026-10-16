// One-shot sound effects, placed on the timeline from Remotion.
import fs from "node:fs";
import {
  SR,
  add,
  biquad,
  buffer,
  mixInto,
  noise,
  normalize,
  reverb,
  sineOsc,
  writeWav,
} from "./dsp.mjs";

const OUT = new URL("../public/audio/", import.meta.url);
fs.mkdirSync(OUT, { recursive: true });

function render(name, seconds, draw, { wet = 0.25, room = 0.75, peak = -3 } = {}) {
  const dry = buffer(seconds);
  draw(dry);
  const out = buffer(seconds);
  mixInto(out, dry, 1);
  if (wet > 0) mixInto(out, reverb(dry, { room }), wet);
  normalize(out, peak);
  writeWav(new URL(`${name}.wav`, OUT), out);
  console.log(name);
}

/** Band-passed noise sweep, panned across the stereo field. */
function sweep(buf, at, len, f0, f1, { q = 1.2, panFrom = -0.6, panTo = 0.6, shape = 1.6 } = {}) {
  const bpL = biquad("bp"),
    bpR = biquad("bp");
  const n = Math.floor(len * SR);
  const s = Math.floor(at * SR);
  for (let i = 0; i < n; i++) {
    const p = i / n;
    const env = Math.sin(Math.PI * p ** (1 / shape)) ** 2;
    const f = f0 * (f1 / f0) ** p;
    const pan = panFrom + (panTo - panFrom) * p;
    const x = noise();
    const l = bpL(x, f, q) * env * Math.cos(((pan + 1) * Math.PI) / 4);
    const r = bpR(x, f, q) * env * Math.sin(((pan + 1) * Math.PI) / 4);
    buf[0][s + i] += l;
    buf[1][s + i] += r;
  }
}

render("whoosh", 1.2, (b) => sweep(b, 0, 0.75, 300, 4200, { q: 0.9 }));
render("whoosh-low", 1.6, (b) => {
  sweep(b, 0, 1.0, 140, 1600, { q: 0.8, panFrom: 0.5, panTo: -0.5 });
  const s = sineOsc();
  add(b, 0.55, 0.8, (t) => s(55 + 40 * Math.exp(-t * 6)) * Math.exp(-t * 4), 0.6);
});
render("swipe", 0.6, (b) => sweep(b, 0, 0.26, 900, 7000, { q: 1.4, shape: 0.8 }), { wet: 0.15 });
render(
  "swipe-back",
  0.6,
  (b) => sweep(b, 0, 0.26, 6000, 800, { q: 1.4, panFrom: 0.6, panTo: -0.6, shape: 0.8 }),
  { wet: 0.15 },
);

render(
  "click",
  0.25,
  (b) => {
    const hp = biquad("hp");
    const s = sineOsc();
    add(b, 0, 0.02, (t) => hp(noise(), 3000, 1) * Math.exp(-t * 400), 0.7);
    add(b, 0.002, 0.06, (t) => s(1800 - 900 * t * 10) * Math.exp(-t * 90), 0.35);
  },
  { wet: 0.08, peak: -6 },
);

for (let k = 0; k < 4; k++) {
  render(
    `key${k}`,
    0.12,
    (b) => {
      const bp = biquad("bp");
      const f = 2400 + k * 450;
      add(b, 0, 0.04, (t) => bp(noise(), f, 2.5) * Math.exp(-t * 180), 1);
      const s = sineOsc();
      add(b, 0, 0.03, (t) => s(f / 4) * Math.exp(-t * 160), 0.25);
    },
    { wet: 0, peak: -12 },
  );
}

render(
  "pop",
  0.4,
  (b) => {
    const s = sineOsc();
    add(b, 0, 0.12, (t) => s(520 + 900 * Math.min(1, t * 25)) * Math.exp(-t * 32), 1);
  },
  { wet: 0.2, peak: -9 },
);

render(
  "chime",
  2.2,
  (b) => {
    const note = (at, hz, g) =>
      add(
        b,
        at,
        2,
        (t) =>
          Math.sin(
            2 * Math.PI * hz * t + 1.2 * Math.sin(2 * Math.PI * hz * 2 * t) * Math.exp(-t * 6),
          ) * Math.exp(-t * 2.4),
        g,
      );
    note(0, 1318.5, 0.5);
    note(0.09, 1975.5, 0.45);
  },
  { wet: 0.35, peak: -8 },
);

render(
  "impact",
  3,
  (b) => {
    const s = sineOsc();
    const lp = biquad("lp");
    add(b, 0, 2.8, (t) => Math.tanh(s(32 + 90 * Math.exp(-t * 9)) * Math.exp(-t * 1.6) * 2.2), 0.9);
    add(b, 0, 1.2, (t) => lp(noise(), 200 + 5000 * Math.exp(-t * 10)) * Math.exp(-t * 4), 0.7);
  },
  { wet: 0.4, room: 0.88, peak: -1 },
);

render(
  "thud",
  1,
  (b) => {
    const s = sineOsc();
    add(b, 0, 0.6, (t) => Math.tanh(s(48 + 120 * Math.exp(-t * 25)) * Math.exp(-t * 8) * 2), 1);
    const lp = biquad("lp");
    add(b, 0, 0.1, (t) => lp(noise(), 1800) * Math.exp(-t * 60), 0.4);
  },
  { wet: 0.15, peak: -3 },
);

render(
  "glitch",
  0.5,
  (b) => {
    const s = sineOsc();
    add(
      b,
      0,
      0.32,
      (t, i) => {
        const step = Math.floor(t * 60);
        const f = [880, 220, 1760, 440, 3520, 330, 1320, 660][step % 8];
        const gate = Math.floor(t * 180) % 3 !== 1 ? 1 : 0;
        return (s(f) * 0.5 + noise() * 0.35) * gate * (1 - t / 0.32);
      },
      0.6,
    );
  },
  { wet: 0.1, peak: -9 },
);

render(
  "tick",
  0.15,
  (b) => {
    const s = sineOsc();
    add(b, 0, 0.05, (t) => s(3200) * Math.exp(-t * 140), 1);
  },
  { wet: 0.05, peak: -14 },
);

render(
  "shimmer",
  2.5,
  (b) => {
    [76, 81, 83, 88, 93].forEach((m, k) => {
      const hz = 440 * 2 ** ((m - 69) / 12);
      add(
        b,
        k * 0.06,
        2,
        (t) => Math.sin(2 * Math.PI * hz * t) * Math.exp(-t * 2.2),
        0.25,
        k % 2 ? 0.5 : -0.5,
      );
    });
    sweep(b, 0, 1.2, 3000, 12000, { q: 0.7 });
  },
  { wet: 0.5, room: 0.9, peak: -8 },
);
