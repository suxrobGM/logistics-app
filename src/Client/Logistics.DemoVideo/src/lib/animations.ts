import { interpolate, spring, useCurrentFrame, useVideoConfig } from "remotion";

/** Fade in from 0 to 1 over `durationFrames`, starting at `delay` */
export function useFadeIn(delay = 0, durationFrames = 15): number {
  const frame = useCurrentFrame();
  return interpolate(frame, [delay, delay + durationFrames], [0, 1], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });
}

/** Slide up from `distance` px to 0, starting at `delay` */
export function useSlideUp(delay = 0, durationFrames = 20, distance = 30) {
  const frame = useCurrentFrame();
  const progress = interpolate(frame, [delay, delay + durationFrames], [0, 1], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });
  const eased = easeOut(progress);
  return {
    opacity: progress,
    translateY: (1 - eased) * distance,
  };
}

/** Spring-based scale animation */
export function useSpringScale(delay = 0) {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  const delayedFrame = Math.max(0, frame - delay);
  return spring({
    fps,
    frame: delayedFrame,
    config: { damping: 200, stiffness: 100 },
  });
}

/** Count up from 0 to `target` over `durationFrames` starting at `delay` */
export function useCountUp(target: number, delay = 0, durationFrames = 30, decimals = 0): string {
  const frame = useCurrentFrame();
  const value = interpolate(frame, [delay, delay + durationFrames], [0, target], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });

  if (decimals > 0) {
    return value.toFixed(decimals);
  }
  return Math.round(value).toLocaleString("en-US");
}

/** Format a number as currency */
export function formatCurrency(value: number): string {
  return "$" + value.toLocaleString("en-US");
}

/** Ease out quad */
function easeOut(t: number): number {
  return 1 - (1 - t) * (1 - t);
}
