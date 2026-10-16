import { AbsoluteFill, interpolate, useCurrentFrame } from "remotion";
import { Crop } from "../components/Crop";
import { lerp, outExpo, prog, sp } from "../components/motion";
import { Stage } from "../components/Stage";
import { Label, Words } from "../components/Type";
import { C, MONO, SANS } from "../theme";

const KPIS = { x: 276, y: 82, w: 1308, h: 114 };
const PAID = 321479;

export function Money() {
  const frame = useCurrentFrame();
  const count = interpolate(frame, [8, 70], [0, PAID], {
    easing: outExpo,
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });
  const strip = sp(frame, 26, { damping: 20, stiffness: 120 });
  const numIn = prog(frame, 4, 24);
  return (
    <Stage>
      <AbsoluteFill style={{ padding: "96px 140px" }}>
        <Label text="Invoicing · Payroll · Payments" at={2} />
        <Words
          text="Invoiced the day it delivers."
          at={6}
          stagger={3}
          size={72}
          style={{ marginTop: 18 }}
        />
      </AbsoluteFill>
      <AbsoluteFill style={{ justifyContent: "center", alignItems: "center", paddingTop: 70 }}>
        <div
          style={{
            textAlign: "center",
            opacity: numIn,
            transform: `translateY(${lerp(40, 0, numIn)}px)`,
          }}
        >
          <div style={{ fontFamily: MONO, fontSize: 22, letterSpacing: "0.16em", color: C.muted }}>
            TOTAL PAID
          </div>
          <div
            style={{
              fontFamily: SANS,
              fontWeight: 600,
              fontSize: 200,
              letterSpacing: "-0.05em",
              color: C.text,
              fontVariantNumeric: "tabular-nums",
              lineHeight: 1.05,
            }}
          >
            ${Math.round(count).toLocaleString("en-US")}
          </div>
        </div>
      </AbsoluteFill>
      <AbsoluteFill style={{ perspective: 1400 }}>
        <div
          style={{
            position: "absolute",
            left: 960 - (KPIS.w * 1.1) / 2,
            top: 820,
            transform: `translateY(${lerp(300, 0, strip)}px) rotateX(${lerp(50, 22, strip)}deg)`,
            transformOrigin: "50% 0%",
            opacity: strip,
          }}
        >
          <Crop
            src="stills/financials.png"
            rect={KPIS}
            scale={1.1}
            radius={14}
            style={{ boxShadow: "0 30px 90px rgba(0,0,0,0.6)" }}
          />
        </div>
      </AbsoluteFill>
    </Stage>
  );
}
