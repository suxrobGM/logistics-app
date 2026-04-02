import type { ReactElement } from "react";
import { Img, interpolate, staticFile, useCurrentFrame } from "remotion";

interface ScreenshotFrameProps {
  src: string;
  delay?: number;
  startScale?: number;
  endScale?: number;
  panX?: number;
  panY?: number;
  durationFrames?: number;
  borderRadius?: number;
}

export function ScreenshotFrame(props: ScreenshotFrameProps): ReactElement {
  const {
    src,
    delay = 0,
    startScale = 1.05,
    endScale = 1.0,
    panX = 0,
    panY = 0,
    durationFrames = 240,
    borderRadius = 12,
  } = props;

  const frame = useCurrentFrame();

  const opacity = interpolate(frame, [delay, delay + 20], [0, 1], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });

  const translateY = interpolate(frame, [delay, delay + 25], [30, 0], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });

  const scale = interpolate(frame, [delay, delay + durationFrames], [startScale, endScale], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });

  const translateXKB = interpolate(frame, [delay, delay + durationFrames], [0, panX], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });

  const translateYKB = interpolate(frame, [delay, delay + durationFrames], [0, panY], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });

  return (
    <div
      className="size-full overflow-hidden shadow-2xl"
      style={{
        borderRadius,
        opacity,
        transform: `translateY(${translateY}px)`,
      }}
    >
      <Img
        src={staticFile(src)}
        className="size-full object-cover"
        style={{
          transform: `scale(${scale}) translate(${translateXKB}px, ${translateYKB}px)`,
        }}
      />
    </div>
  );
}
