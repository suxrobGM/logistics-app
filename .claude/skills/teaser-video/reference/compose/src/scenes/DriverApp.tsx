import { AbsoluteFill, Img, interpolate, staticFile, useCurrentFrame } from "remotion";
import { inExpo, inOutCubic, lerp, prog, sp } from "../components/motion";
import { Stage } from "../components/Stage";
import { Label, Words } from "../components/Type";
import { C, SANS } from "../theme";

const PHONE_H = 720;
const BEZEL = 11;
const SCREEN_W = Math.round(((PHONE_H - BEZEL * 2) * 1280) / 2856);
const PHONE_W = SCREEN_W + BEZEL * 2;

const PHONES = [
  { screens: ["load-detail", "documents"], x: -360, z: -220, ry: 24, at: 14 },
  { screens: ["today", "chat"], x: 0, z: 0, ry: 0, at: 0 },
  { screens: ["inspection", "earnings"], x: 360, z: -220, ry: -24, at: 28 },
];
const SWAP = 150;

function Phone({ screens, swap }: { screens: string[]; swap: number }) {
  return (
    <div
      style={{
        width: PHONE_W,
        height: PHONE_H,
        borderRadius: 58,
        backgroundColor: "#050506",
        padding: BEZEL,
        boxShadow: "0 50px 120px rgba(0,0,0,0.7), inset 0 0 0 1.5px rgba(255,255,255,0.16)",
      }}
    >
      <div
        style={{
          width: SCREEN_W,
          height: PHONE_H - BEZEL * 2,
          borderRadius: 48,
          overflow: "hidden",
          position: "relative",
          backgroundColor: "#f2f2f4",
        }}
      >
        {screens.map((s, i) => (
          <Img
            key={s}
            src={staticFile(`phones/${s}.png`)}
            style={{
              position: "absolute",
              width: SCREEN_W,
              height: "100%",
              left: (i - swap) * SCREEN_W * (i === 0 ? 0.3 : 1),
              opacity: i === 0 ? 1 - swap * 0.4 : 1,
            }}
          />
        ))}
        <div
          style={{
            position: "absolute",
            top: 10,
            left: "50%",
            width: 96,
            height: 28,
            marginLeft: -48,
            borderRadius: 20,
            backgroundColor: "#050506",
          }}
        />
      </div>
    </div>
  );
}

export function DriverApp() {
  const frame = useCurrentFrame();
  const orbit = interpolate(frame, [0, 240], [-16, -4]);
  const exit = prog(frame, 216, 24, inExpo);
  return (
    <Stage>
      <AbsoluteFill style={{ padding: "0 0 0 140px", justifyContent: "center", opacity: 1 - exit }}>
        <Label text="Driver app" at={4} />
        <Words
          text="Drivers run the day from their phone."
          at={12}
          stagger={4}
          size={80}
          style={{ marginTop: 22, width: 620 }}
        />
        <div
          style={{
            marginTop: 34,
            display: "flex",
            gap: 14,
            fontFamily: SANS,
            fontSize: 26,
            color: C.muted,
          }}
        >
          {["Loads", "Inspections", "Documents", "Chat", "Pay"].map((w, i) => {
            const p = prog(frame, 70 + i * 6, 20);
            return (
              <span key={w} style={{ opacity: p, transform: `translateY(${lerp(14, 0, p)}px)` }}>
                {w}
                {i < 4 && <span style={{ color: C.dim }}> ·</span>}
              </span>
            );
          })}
        </div>
      </AbsoluteFill>
      <AbsoluteFill style={{ perspective: 1800, perspectiveOrigin: "70% 50%" }}>
        <AbsoluteFill
          style={{
            transformStyle: "preserve-3d",
            transform: `translateX(${1340 - 960}px) translateY(${exit * -60}px) rotateY(${orbit}deg) scale(${1 - exit * 0.1})`,
            opacity: 1 - exit,
          }}
        >
          {PHONES.map((p, i) => {
            const rise = sp(frame, p.at, { damping: 17, stiffness: 90 });
            const swap = prog(frame, SWAP + i * 8, 34, inOutCubic);
            return (
              <div
                key={i}
                style={{
                  position: "absolute",
                  left: 960 - PHONE_W / 2,
                  top: 540 - PHONE_H / 2,
                  transform: `translate3d(${p.x}px, ${lerp(1100, 0, rise)}px, ${p.z}px) rotateY(${p.ry}deg)`,
                }}
              >
                <Phone screens={p.screens} swap={swap} />
              </div>
            );
          })}
        </AbsoluteFill>
      </AbsoluteFill>
    </Stage>
  );
}
