import { AbsoluteFill, interpolate, useCurrentFrame } from "remotion";
import { Logo } from "../components/Logo";
import { lerp, prog } from "../components/motion";
import { Stage } from "../components/Stage";
import { Headline, Reveal } from "../components/Type";
import { C, MONO } from "../theme";
import { Wordmark } from "./BrandReveal";

export function Close() {
  const frame = useCurrentFrame();
  const fade = interpolate(frame, [400, 470], [1, 0], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });
  const drift = interpolate(frame, [0, 480], [1, 1.05]);
  const url = prog(frame, 120, 30);
  return (
    <Stage grid={false}>
      <AbsoluteFill
        style={{
          justifyContent: "center",
          alignItems: "center",
          opacity: fade,
          transform: `scale(${drift})`,
        }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: 36 }}>
          <Logo at={0} size={170} />
          <Wordmark at={8} size={140} />
        </div>
        <Reveal at={56} style={{ marginTop: 30 }}>
          <Headline
            size={46}
            style={{ color: C.muted, fontWeight: 500, letterSpacing: "-0.015em" }}
          >
            Fleet management for trucking companies.
          </Headline>
        </Reveal>
        <div
          style={{
            marginTop: 70,
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            gap: 16,
          }}
        >
          <div
            style={{
              fontFamily: MONO,
              fontSize: 40,
              color: C.text,
              letterSpacing: "0.02em",
              opacity: url,
              transform: `translateY(${lerp(16, 0, url)}px)`,
            }}
          >
            logisticsx.app
          </div>
          <div
            style={{
              width: lerp(0, 330, prog(frame, 136, 40)),
              height: 2,
              backgroundColor: C.brand,
            }}
          />
          <div
            style={{
              fontFamily: MONO,
              fontSize: 20,
              letterSpacing: "0.16em",
              color: C.muted,
              opacity: prog(frame, 168, 30),
            }}
          >
            LIVE DEMO · NO SIGNUP
          </div>
        </div>
      </AbsoluteFill>
    </Stage>
  );
}
