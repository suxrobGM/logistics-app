import type { ReactElement } from "react";
import { AbsoluteFill, interpolate, useCurrentFrame } from "remotion";

export function GradientBackground(): ReactElement {
  const frame = useCurrentFrame();

  const orb1X = interpolate(frame, [0, 1800], [20, 80]);
  const orb1Y = interpolate(frame, [0, 1800], [30, 60]);
  const orb2X = interpolate(frame, [0, 1800], [70, 25]);
  const orb2Y = interpolate(frame, [0, 1800], [60, 35]);

  return (
    <AbsoluteFill className="bg-base">
      <div
        className="absolute size-150 rounded-full"
        style={{
          left: `${orb1X}%`,
          top: `${orb1Y}%`,
          background: "var(--color-gradient-start)",
          opacity: 0.06,
          filter: "blur(120px)",
          transform: "translate(-50%, -50%)",
        }}
      />
      <div
        className="absolute size-125 rounded-full"
        style={{
          left: `${orb2X}%`,
          top: `${orb2Y}%`,
          background: "var(--color-gradient-end)",
          opacity: 0.05,
          filter: "blur(100px)",
          transform: "translate(-50%, -50%)",
        }}
      />
    </AbsoluteFill>
  );
}
