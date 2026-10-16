import { AbsoluteFill, interpolate, useCurrentFrame } from "remotion";
import { prog } from "../components/motion";
import { Stage } from "../components/Stage";
import { Headline, Label, Reveal } from "../components/Type";
import { C } from "../theme";
import { BEAT } from "../timeline";

const LINES: Array<{ at: number; num: string; rest: string }> = [
  { at: 30, num: "12", rest: " loads to cover." },
  { at: 90, num: "9", rest: " drivers on the clock." },
  { at: 150, num: "1", rest: " dispatcher." },
];

export function ColdOpen() {
  const frame = useCurrentFrame();
  const fadeIn = interpolate(frame, [0, 20], [0, 1], { extrapolateRight: "clamp" });
  return (
    <Stage>
      <AbsoluteFill style={{ opacity: fadeIn, padding: "0 200px", justifyContent: "center" }}>
        <Label text="Monday  06:58 AM" at={4} style={{ marginBottom: 40 }} />
        {LINES.map((l, i) => {
          const next = LINES[i + 1]?.at ?? 999;
          const dimmed = prog(frame, next, 20);
          const color = interpolate(dimmed, [0, 1], [1, 0.32]);
          return (
            <Reveal key={i} at={l.at} out={204 + i * 4}>
              <Headline size={124} style={{ opacity: color }}>
                <span style={{ fontVariantNumeric: "tabular-nums" }}>{l.num}</span>
                <span style={{ color: i === 2 ? C.text : C.muted }}>{l.rest}</span>
              </Headline>
            </Reveal>
          );
        })}
      </AbsoluteFill>
      <BeatTicks />
    </Stage>
  );
}

/** Eight ticks along the bottom, one lighting per beat, like a clock running down. */
function BeatTicks() {
  const frame = useCurrentFrame();
  const beat = Math.floor(frame / BEAT);
  return (
    <AbsoluteFill style={{ justifyContent: "flex-end", alignItems: "center", paddingBottom: 90 }}>
      <div style={{ display: "flex", gap: 18 }}>
        {Array.from({ length: 8 }, (_, i) => {
          const lit = i <= beat;
          const pulse = i === beat ? 1 - (frame % BEAT) / BEAT : 0;
          return (
            <div
              key={i}
              style={{
                width: 44,
                height: 3,
                backgroundColor: lit ? C.text : C.dim,
                opacity: lit ? 0.5 + pulse * 0.5 : 0.35,
              }}
            />
          );
        })}
      </div>
    </AbsoluteFill>
  );
}
