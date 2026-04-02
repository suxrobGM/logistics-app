import type { ReactElement } from "react";
import { spring, useCurrentFrame, useVideoConfig } from "remotion";
import { fontFamily } from "@/lib/fonts";

interface FeaturePillProps {
  label: string;
  delay?: number;
  color?: string;
}

export function FeaturePill(props: FeaturePillProps): ReactElement {
  const { label, delay = 0, color = "var(--color-primary)" } = props;
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();

  const scale = spring({
    fps,
    frame: Math.max(0, frame - delay),
    config: { damping: 200, stiffness: 100 },
  });

  return (
    <div
      className="mr-3 mb-2 inline-flex items-center rounded-full px-5 py-2 text-lg font-medium"
      style={{
        fontFamily,
        backgroundColor: `${color}15`,
        border: `1px solid ${color}30`,
        color,
        transform: `scale(${scale})`,
        opacity: scale,
      }}
    >
      {label}
    </div>
  );
}
