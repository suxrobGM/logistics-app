import { AbsoluteFill, interpolate, useCurrentFrame } from "remotion";
import { Crop } from "../components/Crop";
import { inOutCubic, lerp, prog, sp } from "../components/motion";
import { Stage } from "../components/Stage";
import { Label, Words } from "../components/Type";
import { C, MONO } from "../theme";

const TABLE = { x: 276, y: 220, w: 1308, h: 680 };
const ROW = { x: 304, y: 796, w: 1252, h: 52 };
const SCALE = 1.05;

export function Compliance() {
  const frame = useCurrentFrame();
  const rise = sp(frame, 0, { damping: 20, stiffness: 110 });
  const scan = prog(frame, 14, 56, inOutCubic);
  const lift = sp(frame, 62, { damping: 13, stiffness: 160 });
  const drift = interpolate(frame, [0, 120], [0, 1]);
  const rowTop = (ROW.y - TABLE.y) * SCALE;
  return (
    <Stage>
      <AbsoluteFill style={{ perspective: 1600 }}>
        <div
          style={{
            position: "absolute",
            left: 960 - (TABLE.w * SCALE) / 2,
            top: 300,
            transformStyle: "preserve-3d",
            transformOrigin: "50% 0%",
            transform: `translateY(${lerp(500, 0, rise)}px) rotateX(${lerp(24, 18, drift)}deg) rotateY(${lerp(-8, -3, drift)}deg) translateZ(${lerp(-80, 0, drift)}px)`,
          }}
        >
          <Crop
            src="stills/eld.png"
            rect={TABLE}
            scale={SCALE}
            radius={14}
            style={{
              boxShadow: "0 40px 120px rgba(0,0,0,0.6)",
              filter: `brightness(${1 - lift * 0.35})`,
            }}
          />
          <div
            style={{
              position: "absolute",
              left: 0,
              right: 0,
              top: scan * TABLE.h * SCALE,
              height: 2,
              backgroundColor: C.brand,
              opacity: scan > 0 && scan < 1 ? 0.9 : 0,
            }}
          />
          <div
            style={{
              position: "absolute",
              left: (ROW.x - TABLE.x) * SCALE,
              top: rowTop,
              transform: `translateZ(${lift * 90}px) scale(${1 + lift * 0.04})`,
              transformOrigin: "50% 50%",
              opacity: lift > 0.01 ? 1 : 0,
            }}
          >
            <Crop
              src="stills/eld.png"
              rect={ROW}
              scale={SCALE}
              radius={8}
              style={{ boxShadow: `0 20px 60px rgba(0,0,0,0.55), 0 0 0 2px ${C.bad}` }}
            />
            <div
              style={{
                position: "absolute",
                right: 0,
                top: -54,
                fontFamily: MONO,
                fontSize: 22,
                letterSpacing: "0.08em",
                color: "#fff",
                backgroundColor: C.bad,
                padding: "6px 12px",
                borderRadius: 6,
                opacity: prog(frame, 72, 12),
              }}
            >
              OUT OF HOURS · UNAVAILABLE
            </div>
          </div>
        </div>
      </AbsoluteFill>
      <AbsoluteFill style={{ padding: "96px 140px" }}>
        <Label text="ELD · Hours of service" at={2} />
        <Words
          text="Driver hours, checked before dispatch."
          at={6}
          stagger={3}
          size={72}
          style={{ marginTop: 18 }}
        />
      </AbsoluteFill>
    </Stage>
  );
}
