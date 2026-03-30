import type { ReactElement } from "react";
import { AbsoluteFill, useCurrentFrame, useVideoConfig } from "remotion";

export function DemoVideo(): ReactElement {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  const seconds = Math.floor(frame / fps);

  return (
    <AbsoluteFill className="items-center justify-center bg-slate-900">
      <h1 className="text-6xl font-bold text-white">LogisticsX Demo</h1>
      <p className="mt-4 text-2xl text-slate-400">
        Frame {frame} — {seconds}s
      </p>
    </AbsoluteFill>
  );
}
