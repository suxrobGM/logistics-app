import type { ReactElement } from "react";
import { interpolate, useCurrentFrame } from "remotion";
import { fontFamily } from "@/lib/fonts";

interface TypewriterTextProps {
  text: string;
  delay?: number;
  frameDuration?: number;
  fontSize?: number;
  fontWeight?: number;
  color?: string;
  showCursor?: boolean;
}

export function TypewriterText(props: TypewriterTextProps): ReactElement {
  const {
    text,
    delay = 0,
    frameDuration = 60,
    fontSize = 48,
    fontWeight = 700,
    color = "var(--color-text-primary)",
    showCursor = true,
  } = props;

  const frame = useCurrentFrame();
  const charCount = Math.floor(
    interpolate(frame, [delay, delay + frameDuration], [0, text.length], {
      extrapolateLeft: "clamp",
      extrapolateRight: "clamp",
    }),
  );

  const displayText = text.slice(0, charCount);
  const cursorOpacity =
    showCursor && frame >= delay ? (Math.floor(frame / 8) % 2 === 0 ? 1 : 0) : 0;

  return (
    <div className="whitespace-pre-wrap" style={{ fontFamily, fontSize, fontWeight, color }}>
      {displayText}
      <span className="text-primary font-light" style={{ opacity: cursorOpacity }}>
        |
      </span>
    </div>
  );
}
