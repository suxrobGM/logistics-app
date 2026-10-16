import { Easing, interpolate, spring } from "remotion";
import { FPS } from "../timeline";

export const outExpo = Easing.bezier(0.16, 1, 0.3, 1);
export const inOutCubic = Easing.bezier(0.65, 0, 0.35, 1);
export const inExpo = Easing.bezier(0.7, 0, 0.84, 0);
export const outBack = Easing.bezier(0.34, 1.4, 0.64, 1);

/** 0..1 over [start, start+dur] with easing, clamped. */
export function prog(frame: number, start: number, dur: number, easing = outExpo) {
  return interpolate(frame, [start, start + dur], [0, 1], {
    easing,
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });
}

export function lerp(a: number, b: number, t: number) {
  return a + (b - a) * t;
}

export function sp(
  frame: number,
  start: number,
  config: { damping?: number; stiffness?: number; mass?: number } = {},
) {
  return spring({
    frame: frame - start,
    fps: FPS,
    config: { damping: 18, stiffness: 140, mass: 1, ...config },
  });
}

/** Deterministic pseudo-random in [0,1) from an integer seed. */
export function hash(n: number) {
  const x = Math.sin(n * 127.1 + 311.7) * 43758.5453;
  return x - Math.floor(x);
}
