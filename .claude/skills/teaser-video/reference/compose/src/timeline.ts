/** 120 BPM at 60fps: a beat is 30 frames, a bar 120. The music edits in audio/build-audio.mjs assume it. */
export const FPS = 60;
export const BEAT = 30;
export const BAR = 120;

export const SCENES = {
  coldOpen: { from: 0, dur: 240 },
  chaos: { from: 240, dur: 240 },
  reveal: { from: 480, dur: 240 },
  ai: { from: 720, dur: 720 },
  map: { from: 1440, dur: 240 },
  driver: { from: 1680, dur: 240 },
  compliance: { from: 1920, dur: 120 },
  money: { from: 2040, dur: 120 },
  montage: { from: 2160, dur: 240 },
  close: { from: 2400, dur: 480 },
} as const;

export const TOTAL = 2880;
