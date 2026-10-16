import type { CSSProperties, ReactNode } from "react";
import { useCurrentFrame } from "remotion";
import { C, MONO, SANS } from "../theme";
import { inExpo, lerp, prog } from "./motion";

/** A line that rises out of a mask. `out` starts the exit, which drops it back under. */
export function Reveal({
  at,
  out,
  dur = 26,
  children,
  style,
}: {
  at: number;
  out?: number;
  dur?: number;
  children: ReactNode;
  style?: CSSProperties;
}) {
  const frame = useCurrentFrame();
  const p = prog(frame, at, dur);
  const q = out === undefined ? 0 : prog(frame, out, 18, inExpo);
  return (
    <div style={{ overflow: "hidden", paddingBottom: "0.12em", marginBottom: "-0.12em", ...style }}>
      <div
        style={{ transform: `translateY(${lerp(110, 0, p) - q * 110}%)`, willChange: "transform" }}
      >
        {children}
      </div>
    </div>
  );
}

/** Word-by-word reveal; each word rises on its own with a short stagger. */
export function Words({
  text,
  at,
  stagger = 4,
  out,
  style,
  accent,
  size = 96,
}: {
  text: string;
  at: number;
  stagger?: number;
  out?: number;
  style?: CSSProperties;
  accent?: string[];
  size?: number;
}) {
  const words = text.split(" ");
  return (
    <div
      style={{
        display: "flex",
        flexWrap: "wrap",
        columnGap: "0.26em",
        fontFamily: SANS,
        fontWeight: 600,
        fontSize: size,
        lineHeight: 1.04,
        letterSpacing: "-0.035em",
        color: C.text,
        ...style,
      }}
    >
      {words.map((w, i) => (
        <Reveal key={i} at={at + i * stagger} out={out === undefined ? undefined : out + i * 2}>
          <span style={{ color: accent?.includes(w) ? C.muted : undefined }}>{w}</span>
        </Reveal>
      ))}
    </div>
  );
}

export function Headline({
  children,
  size = 96,
  style,
}: {
  children: ReactNode;
  size?: number;
  style?: CSSProperties;
}) {
  return (
    <div
      style={{
        fontFamily: SANS,
        fontWeight: 600,
        fontSize: size,
        lineHeight: 1.04,
        letterSpacing: "-0.035em",
        color: C.text,
        ...style,
      }}
    >
      {children}
    </div>
  );
}

/** Small uppercase mono label that types itself in. */
export function Label({
  text,
  at,
  style,
  color = C.muted,
}: {
  text: string;
  at: number;
  style?: CSSProperties;
  color?: string;
}) {
  const frame = useCurrentFrame();
  const shown = Math.max(0, Math.min(text.length, Math.floor((frame - at) / 1.5)));
  const caretOn = frame >= at && (shown < text.length || Math.floor(frame / 15) % 2 === 0);
  return (
    <div
      style={{
        fontFamily: MONO,
        fontSize: 22,
        letterSpacing: "0.16em",
        textTransform: "uppercase",
        color,
        whiteSpace: "pre",
        ...style,
      }}
    >
      {text.slice(0, shown)}
      <span style={{ opacity: caretOn ? 1 : 0, color: C.text }}>▍</span>
    </div>
  );
}
