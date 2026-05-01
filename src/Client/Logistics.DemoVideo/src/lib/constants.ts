// Video config
export const FPS = 30;
export const TRANSITION_FRAMES = 15;

// Scene durations (raw frames, before transition overlap)
// Total raw: 1905 - minus 7 transitions × 15 = 105 → net 1800 frames = 60s
export const SCENE_DURATIONS = {
  brandIntro: 150, //  5.0s
  problem: 150, //  5.0s
  dashboard: 240, //  8.0s
  loads: 210, //  7.0s
  aiDispatch: 510, // 17.0s - hero feature, 3 screenshots
  invoicing: 180, //  6.0s
  multiPlatform: 150, //  5.0s
  cta: 315, // 10.5s
} as const;
