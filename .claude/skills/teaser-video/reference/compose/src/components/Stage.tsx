import type { ReactNode } from "react";
import { AbsoluteFill, getInputProps, useCurrentFrame } from "remotion";
import { C } from "../theme";

/** The README GIF render drops grain and grid drift: per-frame noise defeats GIF compression. */
const GIF = Boolean(getInputProps().gif);

/** Flat ink backdrop with a faint drifting dot grid, vignette and film grain. */
export function Stage({ children, grid = true }: { children?: ReactNode; grid?: boolean }) {
  const frame = useCurrentFrame();
  const drift = GIF ? 0 : (frame * 0.15) % 48;
  return (
    <AbsoluteFill style={{ backgroundColor: C.ink, overflow: "hidden" }}>
      {grid && (
        <AbsoluteFill
          style={{
            backgroundImage: "radial-gradient(rgba(255,255,255,0.07) 1px, transparent 1.4px)",
            backgroundSize: "48px 48px",
            backgroundPosition: `${drift}px ${drift * 0.5}px`,
          }}
        />
      )}
      {children}
    </AbsoluteFill>
  );
}

/** Applied once over the whole cut so every scene shares the same lens. */
export function Lens() {
  const frame = useCurrentFrame();
  if (GIF) return null;
  return (
    <AbsoluteFill style={{ pointerEvents: "none" }}>
      <AbsoluteFill
        style={{
          background: "radial-gradient(ellipse at center, transparent 55%, rgba(0,0,0,0.55) 100%)",
        }}
      />
      <AbsoluteFill style={{ opacity: 0.06, mixBlendMode: "overlay" }}>
        <svg width="1920" height="1080">
          <filter id="grain">
            <feTurbulence
              type="fractalNoise"
              baseFrequency="0.9"
              numOctaves="2"
              seed={frame % 24}
              stitchTiles="stitch"
            />
            <feColorMatrix type="saturate" values="0" />
          </filter>
          <rect width="1920" height="1080" filter="url(#grain)" />
        </svg>
      </AbsoluteFill>
    </AbsoluteFill>
  );
}
