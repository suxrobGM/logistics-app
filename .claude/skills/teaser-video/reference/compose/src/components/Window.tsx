import type { CSSProperties, ReactNode } from "react";
import { MONO } from "../theme";

export const BAR_H = 44;

/** Minimal browser chrome around 1600x900 page content, drawn at `scale`. */
export function Window({
  url,
  scale = 1,
  children,
  style,
}: {
  url: string;
  scale?: number;
  children: ReactNode;
  style?: CSSProperties;
}) {
  return (
    <div
      style={{
        width: 1600 * scale,
        height: (900 + BAR_H) * scale,
        borderRadius: 14 * scale,
        overflow: "hidden",
        backgroundColor: "#fff",
        boxShadow: "0 40px 120px rgba(0,0,0,0.55), 0 0 0 1px rgba(255,255,255,0.10)",
        ...style,
      }}
    >
      <div
        style={{
          height: BAR_H * scale,
          backgroundColor: "#E9ECF0",
          display: "flex",
          alignItems: "center",
          padding: `0 ${18 * scale}px`,
          gap: 8 * scale,
          borderBottom: "1px solid #D9DEE4",
        }}
      >
        {[0, 1, 2].map((i) => (
          <div
            key={i}
            style={{
              width: 12 * scale,
              height: 12 * scale,
              borderRadius: 99,
              backgroundColor: "#C3C9D1",
            }}
          />
        ))}
        <div
          style={{
            marginLeft: 24 * scale,
            flex: 1,
            maxWidth: 560 * scale,
            height: 26 * scale,
            borderRadius: 7 * scale,
            backgroundColor: "#fff",
            display: "flex",
            alignItems: "center",
            padding: `0 ${12 * scale}px`,
            fontFamily: MONO,
            fontSize: 13 * scale,
            color: "#5B6573",
          }}
        >
          {url}
        </div>
      </div>
      <div
        style={{
          width: 1600 * scale,
          height: 900 * scale,
          position: "relative",
          overflow: "hidden",
        }}
      >
        {children}
      </div>
    </div>
  );
}
