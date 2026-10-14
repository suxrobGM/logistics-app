import type { ReactElement } from "react";
import { interpolate, spring, useCurrentFrame, useVideoConfig } from "remotion";
import { fontFamily } from "@/lib/fonts";

interface GlowButtonProps {
  label: string;
  delay?: number;
}

export function GlowButton(props: GlowButtonProps): ReactElement {
  const { label, delay = 0 } = props;
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();

  const scale = spring({
    fps,
    frame: Math.max(0, frame - delay),
    config: { damping: 200, stiffness: 80 },
  });

  const glowIntensity = interpolate(Math.sin((frame - delay) * 0.08), [-1, 1], [0.3, 0.8]);

  return (
    <div
      className="inline-flex items-center justify-center rounded-xl bg-linear-to-br from-gradient-start to-gradient-end px-12 py-4.5 font-semibold text-[22px] text-white"
      style={{
        fontFamily,
        transform: `scale(${scale})`,
        opacity: scale,
        boxShadow: `0 0 ${40 * glowIntensity}px rgba(8, 145, 178, ${glowIntensity * 0.4})`,
      }}
    >
      {label}
    </div>
  );
}
