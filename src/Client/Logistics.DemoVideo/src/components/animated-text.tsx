import type { ReactElement } from "react";
import { useSlideUp } from "@/lib/animations";
import { fontFamily } from "@/lib/fonts";

interface AnimatedTextProps {
  text: string;
  delay?: number;
  durationFrames?: number;
  fontSize?: number;
  fontWeight?: number;
  color?: string;
  lineHeight?: number;
  textAlign?: "left" | "center" | "right";
}

export function AnimatedText(props: AnimatedTextProps): ReactElement {
  const {
    text,
    delay = 0,
    durationFrames = 20,
    fontSize = 24,
    fontWeight = 400,
    color = "var(--color-text-primary)",
    lineHeight = 1.4,
    textAlign = "center",
  } = props;

  const { opacity, translateY } = useSlideUp(delay, durationFrames);

  return (
    <div
      style={{
        fontFamily,
        fontSize,
        fontWeight,
        color,
        lineHeight,
        textAlign,
        opacity,
        transform: `translateY(${translateY}px)`,
      }}
    >
      {text}
    </div>
  );
}
