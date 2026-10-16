import {
  AbsoluteFill,
  interpolate,
  OffthreadVideo,
  Sequence,
  staticFile,
  useCurrentFrame,
} from "remotion";
import { Crop } from "../components/Crop";
import { inExpo, inOutCubic, lerp, prog, sp } from "../components/motion";
import { Stage } from "../components/Stage";
import { Label, Words } from "../components/Type";
import { BAR_H, Window } from "../components/Window";
import { C, MONO } from "../theme";
import KEYS from "./ai-keys.json";

/** Speed-ramped pieces of the 1x take (seconds in the take -> frames in this scene). */
const SEGMENTS = [
  { at: 0, len: 100, src: 0, speed: 1 },
  { at: 100, len: 130, src: 1.67, speed: 3.34 },
  { at: 230, len: 170, src: 8.9, speed: 6.953 },
  { at: 400, len: 70, src: 28.6, speed: 6.857 },
  { at: 560, len: 50, src: 38.1, speed: 3.4 },
];
const STILL = { from: 470, to: 560 };
const FINALE = 610;

/** Where the camera looks, in page CSS pixels; dy lowers the frame to leave room for a headline. */
const CAMERA: Array<{ f: number; s: number; cx: number; cy: number; dy: number }> = [
  { f: 0, s: 0.72, cx: 800, cy: 450, dy: 120 },
  { f: 100, s: 0.72, cx: 800, cy: 450, dy: 120 },
  { f: 128, s: 1.28, cx: 790, cy: 590, dy: 0 },
  { f: 226, s: 1.28, cx: 790, cy: 600, dy: 0 },
  { f: 254, s: 1.38, cx: 720, cy: 290, dy: 0 },
  { f: 396, s: 1.34, cx: 740, cy: 320, dy: 0 },
  { f: 426, s: 0.97, cx: 800, cy: 450, dy: 0 },
  { f: 470, s: 0.99, cx: 805, cy: 455, dy: 0 },
  { f: 560, s: 1.0, cx: 800, cy: 470, dy: 0 },
  { f: 610, s: 1.1, cx: 800, cy: 560, dy: 0 },
];

function camera(frame: number) {
  let i = 0;
  while (i < CAMERA.length - 2 && frame > CAMERA[i + 1].f) i++;
  const a = CAMERA[i];
  const b = CAMERA[i + 1];
  const t = interpolate(frame, [a.f, b.f], [0, 1], {
    easing: inOutCubic,
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });
  return {
    s: lerp(a.s, b.s, t),
    cx: lerp(a.cx, b.cx, t),
    cy: lerp(a.cy, b.cy, t),
    dy: lerp(a.dy, b.dy, t),
  };
}

const takeToScene = (t: number) => {
  const seg = SEGMENTS.find((s) => t >= s.src && t <= s.src + (s.len / 60) * s.speed);
  return seg ? seg.at + ((t - seg.src) / seg.speed) * 60 : null;
};

/** Cue times are the moment each sound peaks (see Teaser). */
export const AI_SFX: Array<{ at: number; sfx: string; volume: number }> = [
  { at: 12, sfx: "whoosh", volume: 0.4 },
  ...(KEYS as number[])
    .filter((_, i) => i % 3 === 0)
    .map((t) => ({ at: takeToScene(t) ?? -1, sfx: "key", volume: 0.35 }))
    .filter((c) => c.at >= 0),
  { at: takeToScene(2.26) ?? 110, sfx: "click", volume: 0.6 },
  { at: takeToScene(8.59) ?? 224, sfx: "click", volume: 0.6 },
  ...[244, 292, 340, 398].map((at) => ({ at, sfx: "pop", volume: 0.35 })),
  { at: STILL.from, sfx: "whoosh", volume: 0.35 },
  { at: STILL.to, sfx: "swoosh", volume: 0.3 },
  { at: FINALE - 2, sfx: "click", volume: 0.6 },
  { at: FINALE + 6, sfx: "chime", volume: 0.5 },
];

const PILLS: Array<[number, string]> = [
  [244, "Read 6 unassigned loads"],
  [292, "Checked 5 trucks"],
  [340, "Ran HOS feasibility"],
  [398, "Suggested 1 assignment"],
];

export function AIDispatch() {
  const frame = useCurrentFrame();
  if (frame >= FINALE) return <Finale />;
  if (frame >= STILL.from && frame < STILL.to) return <Reasoning />;

  const cam = camera(frame);
  const rise = sp(frame, 0, { damping: 22, stiffness: 70 });
  const tilt = lerp(34, 0, rise);
  const tx = 960 - cam.s * cam.cx;
  const ty = 540 + cam.dy - cam.s * (cam.cy + BAR_H);
  return (
    <Stage>
      <AbsoluteFill style={{ perspective: 2000 }}>
        <div
          style={{
            position: "absolute",
            left: 0,
            top: 0,
            transformOrigin: "0 0",
            transform: `translate(${tx}px, ${ty + (1 - rise) * 700}px) scale(${cam.s}) rotateX(${tilt}deg)`,
          }}
        >
          <Window url="tms.logisticsx.app/ai-dispatch">
            {SEGMENTS.map((s) => (
              <Sequence key={s.at} from={s.at} durationInFrames={s.len} layout="none">
                <OffthreadVideo
                  src={staticFile("footage/ai.mp4")}
                  trimBefore={Math.round(s.src * 60)}
                  playbackRate={s.speed}
                  muted
                  style={{ width: 1600, height: 900 }}
                />
              </Sequence>
            ))}
          </Window>
        </div>
      </AbsoluteFill>
      <AbsoluteFill
        style={{ alignItems: "center", paddingTop: 70, opacity: 1 - prog(frame, 92, 12) }}
      >
        <Label text="AI dispatch" at={4} />
        <Words
          text="Meet your AI dispatcher."
          at={10}
          stagger={4}
          out={92}
          size={84}
          style={{ marginTop: 16, justifyContent: "center" }}
        />
      </AbsoluteFill>
      <AbsoluteFill style={{ justifyContent: "flex-end", padding: "0 0 110px 120px", gap: 14 }}>
        {PILLS.map(([at, text]) => {
          const p = sp(frame, at, { damping: 18, stiffness: 180 });
          const out = prog(frame, 420, 14, inExpo);
          if (frame < at) return null;
          return (
            <div
              key={text}
              style={{
                alignSelf: "flex-start",
                display: "flex",
                alignItems: "center",
                gap: 14,
                padding: "14px 22px",
                borderRadius: 10,
                backgroundColor: "rgba(10,12,16,0.94)",
                border: "1px solid rgba(255,255,255,0.12)",
                fontFamily: MONO,
                fontSize: 24,
                color: C.text,
                transform: `translateX(${lerp(-60, 0, p) - out * 80}px)`,
                opacity: Math.min(p * 1.5, 1) * (1 - out),
                boxShadow: "0 16px 40px rgba(0,0,0,0.5)",
              }}
            >
              <span style={{ color: C.ok }}>✓</span>
              {text}
            </div>
          );
        })}
      </AbsoluteFill>
    </Stage>
  );
}

/** Close-up of the agent's own reasoning from the finished session (2x still). */
function Reasoning() {
  const frame = useCurrentFrame() - STILL.from;
  const p = sp(frame, 0, { damping: 20, stiffness: 120 });
  const drift = interpolate(frame, [0, 90], [0, 1]);
  return (
    <Stage>
      <AbsoluteFill style={{ perspective: 1600, justifyContent: "center", alignItems: "center" }}>
        <div
          style={{
            transform: `translateY(${lerp(80, -40, p)}px) rotateY(${lerp(-10, 3, drift)}deg) rotateX(${lerp(12, 4, drift)}deg) scale(${lerp(0.9, 1.04, drift)})`,
          }}
        >
          <Crop
            src="stills/ai-final.png"
            rect={{ x: 398, y: 420, w: 694, h: 116 }}
            scale={2.2}
            radius={16}
            style={{ boxShadow: "0 40px 120px rgba(0,0,0,0.65)", opacity: Math.min(1, p * 1.4) }}
          />
        </div>
      </AbsoluteFill>
      <AbsoluteFill
        style={{ justifyContent: "flex-end", alignItems: "center", paddingBottom: 150 }}
      >
        <Words
          text="Every suggestion shows its reasoning."
          at={8}
          stagger={3}
          size={64}
          style={{ justifyContent: "center" }}
        />
      </AbsoluteFill>
      <AbsoluteFill style={{ alignItems: "center", paddingTop: 150 }}>
        <Label text="Agent reasoning" at={2} />
      </AbsoluteFill>
    </Stage>
  );
}

/** The decision, approved by the dispatcher, then the line. */
function Finale() {
  const frame = useCurrentFrame() - FINALE;
  const pop = sp(frame, 0, { damping: 12, stiffness: 170 });
  const exit = prog(frame, 92, 18, inExpo);
  return (
    <Stage>
      <AbsoluteFill
        style={{
          justifyContent: "center",
          alignItems: "center",
          gap: 70,
          opacity: 1 - exit,
          transform: `scale(${1 + exit * 0.3})`,
        }}
      >
        <div style={{ transform: `scale(${lerp(0.6, 1, pop)})`, opacity: Math.min(1, pop * 2) }}>
          <Crop
            src="stills/ai-final.png"
            rect={{ x: 378, y: 212, w: 316, h: 48 }}
            scale={3.2}
            radius={14}
            style={{ boxShadow: "0 30px 90px rgba(0,0,0,0.6)" }}
          />
        </div>
        <div style={{ display: "flex", gap: 30 }}>
          <Words text="It plans." at={10} stagger={4} size={110} />
          <Words text="You approve." at={26} stagger={4} size={110} style={{ color: C.muted }} />
        </div>
      </AbsoluteFill>
    </Stage>
  );
}
