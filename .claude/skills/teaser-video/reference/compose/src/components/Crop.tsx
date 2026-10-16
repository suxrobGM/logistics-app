import type { CSSProperties } from "react";
import { Img, staticFile } from "remotion";

export interface Rect {
  x: number;
  y: number;
  w: number;
  h: number;
}

/** Full CSS viewport the stills were captured at (2x device pixels on disk). */
export const VIEW = { w: 1600, h: 900 };

/**
 * A region of a captured screen, given in the page's CSS pixels, drawn at `scale`.
 * Stills are 2x, so scale up to 2 stays sharp.
 */
export function Crop({
  src,
  rect,
  scale = 1,
  radius = 10,
  style,
}: {
  src: string;
  rect: Rect;
  scale?: number;
  radius?: number;
  style?: CSSProperties;
}) {
  return (
    <div
      style={{
        width: rect.w * scale,
        height: rect.h * scale,
        overflow: "hidden",
        borderRadius: radius,
        position: "relative",
        backgroundColor: "#fff",
        ...style,
      }}
    >
      <Img
        src={staticFile(src)}
        style={{
          position: "absolute",
          width: VIEW.w * scale,
          height: VIEW.h * scale,
          left: -rect.x * scale,
          top: -rect.y * scale,
          maxWidth: "none",
        }}
      />
    </div>
  );
}
