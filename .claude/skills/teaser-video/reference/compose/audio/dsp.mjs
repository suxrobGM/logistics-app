// Small offline DSP kit: everything renders into Float32 stereo buffers.
import fs from "node:fs";

export const SR = 44100;

export function buffer(seconds) {
  const n = Math.ceil(seconds * SR);
  return [new Float32Array(n), new Float32Array(n)];
}

let seed = 1234567;
export function rand() {
  seed = (seed * 1664525 + 1013904223) >>> 0;
  return seed / 4294967296;
}
export const noise = () => rand() * 2 - 1;

export const midiHz = (m) => 440 * 2 ** ((m - 69) / 12);

/** Band-limited saw (polyBLEP). */
export function sawOsc() {
  let phase = rand();
  return (hz) => {
    const dt = hz / SR;
    phase += dt;
    if (phase >= 1) phase -= 1;
    let v = 2 * phase - 1;
    if (phase < dt) {
      const t = phase / dt;
      v -= t + t - t * t - 1;
    } else if (phase > 1 - dt) {
      const t = (phase - 1) / dt;
      v -= t * t + t + t + 1;
    }
    return v;
  };
}

export function sineOsc(start = 0) {
  let phase = start;
  return (hz) => {
    phase += hz / SR;
    if (phase >= 1) phase -= 1;
    return Math.sin(2 * Math.PI * phase);
  };
}

/** RBJ biquad with per-sample settable cutoff (coefficients refreshed every 16 samples). */
export function biquad(type = "lp") {
  let x1 = 0,
    x2 = 0,
    y1 = 0,
    y2 = 0;
  let b0 = 1,
    b1 = 0,
    b2 = 0,
    a1 = 0,
    a2 = 0;
  let count = 0;
  let lastF = -1,
    lastQ = -1;
  const set = (f, q) => {
    f = Math.min(Math.max(f, 20), SR * 0.45);
    const w = (2 * Math.PI * f) / SR;
    const cs = Math.cos(w),
      sn = Math.sin(w);
    const alpha = sn / (2 * q);
    let nb0, nb1, nb2;
    if (type === "lp") {
      nb0 = (1 - cs) / 2;
      nb1 = 1 - cs;
      nb2 = (1 - cs) / 2;
    } else if (type === "hp") {
      nb0 = (1 + cs) / 2;
      nb1 = -(1 + cs);
      nb2 = (1 + cs) / 2;
    } else {
      nb0 = alpha;
      nb1 = 0;
      nb2 = -alpha;
    } // band-pass, constant peak
    const a0 = 1 + alpha;
    b0 = nb0 / a0;
    b1 = nb1 / a0;
    b2 = nb2 / a0;
    a1 = (-2 * cs) / a0;
    a2 = (1 - alpha) / a0;
  };
  return (x, f, q = 0.707) => {
    if (count++ % 16 === 0 && (f !== lastF || q !== lastQ)) {
      set(f, q);
      lastF = f;
      lastQ = q;
    }
    const y = b0 * x + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
    x2 = x1;
    x1 = x;
    y2 = y1;
    y1 = y;
    return y;
  };
}

export function adsr(t, dur, a, d, s, r) {
  if (t < 0) return 0;
  if (t < a) return t / a;
  if (t < a + d) return 1 - (1 - s) * ((t - a) / d);
  if (t < dur) return s;
  const rt = t - dur;
  return rt < r ? s * (1 - rt / r) : 0;
}

/** Add mono signal fn(tLocal, i) at time `at` for `len` seconds, panned. */
export function add(buf, at, len, fn, gain = 1, pan = 0) {
  const start = Math.floor(at * SR);
  const n = Math.floor(len * SR);
  const gl = gain * Math.cos(((pan + 1) * Math.PI) / 4);
  const gr = gain * Math.sin(((pan + 1) * Math.PI) / 4);
  for (let i = 0; i < n; i++) {
    const j = start + i;
    if (j < 0 || j >= buf[0].length) continue;
    const v = fn(i / SR, i);
    buf[0][j] += v * gl;
    buf[1][j] += v * gr;
  }
}

export function mixInto(dst, src, gain = 1, gainFn = null) {
  for (let i = 0; i < dst[0].length && i < src[0].length; i++) {
    const g = gain * (gainFn ? gainFn(i / SR) : 1);
    dst[0][i] += src[0][i] * g;
    dst[1][i] += src[1][i] * g;
  }
}

/** Freeverb-style stereo reverb, returns wet buffer. */
export function reverb(src, { room = 0.86, damp = 0.35, width = 1, preDelay = 0.012 } = {}) {
  const combs = [1116, 1188, 1277, 1356, 1422, 1491, 1557, 1617];
  const aps = [556, 441, 341, 225];
  const spread = 23;
  const out = [new Float32Array(src[0].length), new Float32Array(src[0].length)];
  const pd = Math.floor(preDelay * SR);
  for (let ch = 0; ch < 2; ch++) {
    const cb = combs.map((l) => ({ b: new Float32Array(l + ch * spread), i: 0, f: 0 }));
    const ab = aps.map((l) => ({ b: new Float32Array(l + ch * spread), i: 0 }));
    const inp = src[0].map((_, i) => (src[0][i] + src[1][i]) * 0.5);
    for (let i = 0; i < inp.length; i++) {
      const x = (i - pd >= 0 ? inp[i - pd] : 0) * 0.015;
      let y = 0;
      for (const c of cb) {
        const o = c.b[c.i];
        c.f = o * (1 - damp) + c.f * damp;
        c.b[c.i] = x + c.f * room;
        c.i = (c.i + 1) % c.b.length;
        y += o;
      }
      for (const a of ab) {
        const o = a.b[a.i];
        a.b[a.i] = y + o * 0.5;
        a.i = (a.i + 1) % a.b.length;
        y = o - y;
      }
      out[ch][i] = y;
    }
  }
  if (width < 1) {
    for (let i = 0; i < out[0].length; i++) {
      const m = (out[0][i] + out[1][i]) / 2;
      out[0][i] = m + (out[0][i] - m) * width;
      out[1][i] = m + (out[1][i] - m) * width;
    }
  }
  return out;
}

export function normalize(buf, peakDb = -1) {
  let peak = 0;
  for (const ch of buf) for (const v of ch) peak = Math.max(peak, Math.abs(v));
  const target = 10 ** (peakDb / 20);
  const g = peak > 0 ? target / peak : 1;
  for (const ch of buf) for (let i = 0; i < ch.length; i++) ch[i] *= g;
  return buf;
}

export function softClip(buf, drive = 1.2) {
  const k = Math.tanh(drive);
  for (const ch of buf) for (let i = 0; i < ch.length; i++) ch[i] = Math.tanh(ch[i] * drive) / k;
  return buf;
}

export function fadeOut(buf, seconds) {
  const n = Math.floor(seconds * SR);
  const len = buf[0].length;
  for (let i = 0; i < n; i++) {
    const g = (i / n) ** 2;
    buf[0][len - 1 - i] *= g;
    buf[1][len - 1 - i] *= g;
  }
}

export function writeWav(file, buf) {
  const n = buf[0].length;
  const data = Buffer.alloc(44 + n * 4);
  data.write("RIFF", 0);
  data.writeUInt32LE(36 + n * 4, 4);
  data.write("WAVEfmt ", 8);
  data.writeUInt32LE(16, 16);
  data.writeUInt16LE(1, 20);
  data.writeUInt16LE(2, 22);
  data.writeUInt32LE(SR, 24);
  data.writeUInt32LE(SR * 4, 28);
  data.writeUInt16LE(4, 32);
  data.writeUInt16LE(16, 34);
  data.write("data", 36);
  data.writeUInt32LE(n * 4, 40);
  for (let i = 0; i < n; i++) {
    for (let ch = 0; ch < 2; ch++) {
      const v = Math.max(-1, Math.min(1, buf[ch][i]));
      data.writeInt16LE(Math.round(v * 32767), 44 + i * 4 + ch * 2);
    }
  }
  fs.writeFileSync(file, data);
}
