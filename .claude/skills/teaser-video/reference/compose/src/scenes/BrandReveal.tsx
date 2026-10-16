import { AbsoluteFill, interpolate, useCurrentFrame } from "remotion";
import { Logo } from "../components/Logo";
import { inExpo, inOutCubic, lerp, prog } from "../components/motion";
import { Stage } from "../components/Stage";
import { Headline, Reveal } from "../components/Type";
import { C, SANS } from "../theme";

export function BrandReveal({
  tagline = "Fleet management for trucking companies.",
  exitAt = 212,
}: {
  tagline?: string;
  exitAt?: number;
}) {
  const frame = useCurrentFrame();
  const slide = prog(frame, 52, 40, inOutCubic);
  const exit = prog(frame, exitAt, 28, inExpo);
  return (
    <Stage grid={false}>
      <Rings />
      <AbsoluteFill
        style={{
          justifyContent: "center",
          alignItems: "center",
          transform: `scale(${interpolate(frame, [60, 212], [1, 1.05], { extrapolateLeft: "clamp", extrapolateRight: "clamp" }) + exit * 0.5})`,
          opacity: 1 - exit,
        }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: lerp(0, 36, slide) }}>
          <div style={{ transform: `scale(${lerp(1.35, 1, slide)})` }}>
            <Logo at={0} size={190} />
          </div>
          <div style={{ width: lerp(0, 760, slide), overflow: "hidden", whiteSpace: "nowrap" }}>
            <Wordmark at={62} />
          </div>
        </div>
        <Reveal at={124} style={{ marginTop: 34 }}>
          <Headline
            size={46}
            style={{ color: C.muted, fontWeight: 500, letterSpacing: "-0.015em" }}
          >
            {tagline}
          </Headline>
        </Reveal>
      </AbsoluteFill>
    </Stage>
  );
}

export function Wordmark({ at, size = 150 }: { at: number; size?: number }) {
  const frame = useCurrentFrame();
  return (
    <div
      style={{
        display: "flex",
        fontFamily: SANS,
        fontWeight: 700,
        fontSize: size,
        letterSpacing: "-0.045em",
        color: C.text,
        lineHeight: 1,
      }}
    >
      {"LogisticsX".split("").map((ch, i) => {
        const p = prog(frame, at + i * 2.2, 26);
        return (
          <div key={i} style={{ overflow: "hidden", paddingBottom: "0.1em" }}>
            <div style={{ transform: `translateY(${lerp(105, 0, p)}%)` }}>{ch}</div>
          </div>
        );
      })}
    </div>
  );
}

/** Hairline rings expanding from the mark on the impact. */
function Rings() {
  const frame = useCurrentFrame();
  return (
    <AbsoluteFill style={{ justifyContent: "center", alignItems: "center" }}>
      {[0, 7, 14].map((d) => {
        const p = prog(frame, d, 90);
        return (
          <div
            key={d}
            style={{
              position: "absolute",
              width: lerp(160, 1900, p),
              height: lerp(160, 1900, p),
              borderRadius: "50%",
              border: "1px solid rgba(255,255,255,0.5)",
              opacity: interpolate(p, [0, 0.1, 1], [0, 0.6, 0]),
            }}
          />
        );
      })}
    </AbsoluteFill>
  );
}
