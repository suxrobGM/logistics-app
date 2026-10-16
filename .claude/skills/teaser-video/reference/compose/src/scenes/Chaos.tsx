import { AbsoluteFill, interpolate, useCurrentFrame } from "remotion";
import { Crop, type Rect } from "../components/Crop";
import { hash, inExpo, lerp, prog, sp } from "../components/motion";
import { Stage } from "../components/Stage";
import { Headline } from "../components/Type";
import { C } from "../theme";

interface Fragment {
  src: string;
  rect: Rect;
  scale: number;
  x: number;
  y: number;
  z: number;
  ry: number;
  rz: number;
}

/** Real screens from the demo tenant, picked for their red and amber states. */
const FRAGMENTS: Fragment[] = [
  {
    src: "stills/eld.png",
    rect: { x: 304, y: 796, w: 1252, h: 52 },
    scale: 0.85,
    x: -330,
    y: -330,
    z: -120,
    ry: 14,
    rz: -2,
  },
  {
    src: "stills/maintenance.png",
    rect: { x: 276, y: 58, w: 424, h: 96 },
    scale: 1,
    x: 560,
    y: -250,
    z: 40,
    ry: -18,
    rz: 3,
  },
  {
    src: "stills/invoices.png",
    rect: { x: 718, y: 58, w: 203, h: 132 },
    scale: 1.15,
    x: -640,
    y: 170,
    z: 80,
    ry: 22,
    rz: -4,
  },
  {
    src: "stills/home.png",
    rect: { x: 1154, y: 210, w: 430, h: 240 },
    scale: 0.95,
    x: 600,
    y: 220,
    z: -60,
    ry: -20,
    rz: 2,
  },
  {
    src: "stills/eld.png",
    rect: { x: 304, y: 584, w: 1252, h: 52 },
    scale: 0.75,
    x: 120,
    y: 380,
    z: -300,
    ry: -8,
    rz: 1.5,
  },
  {
    src: "stills/maintenance.png",
    rect: { x: 304, y: 268, w: 590, h: 225 },
    scale: 0.9,
    x: -620,
    y: -120,
    z: -420,
    ry: 26,
    rz: -1,
  },
  {
    src: "stills/eld.png",
    rect: { x: 1160, y: 82, w: 424, h: 114 },
    scale: 1,
    x: 200,
    y: -420,
    z: -200,
    ry: -6,
    rz: -3,
  },
  {
    src: "stills/invoices.png",
    rect: { x: 296, y: 430, w: 1268, h: 210 },
    scale: 0.7,
    x: -150,
    y: 300,
    z: -520,
    ry: 10,
    rz: 2,
  },
  {
    src: "stills/home.png",
    rect: { x: 276, y: 210, w: 868, h: 305 },
    scale: 0.8,
    x: 520,
    y: -20,
    z: -650,
    ry: -24,
    rz: -2,
  },
  {
    src: "stills/invoices.png",
    rect: { x: 1380, y: 58, w: 204, h: 132 },
    scale: 1.1,
    x: -420,
    y: -300,
    z: 160,
    ry: 18,
    rz: 4,
  },
  {
    src: "stills/maintenance.png",
    rect: { x: 276, y: 170, w: 646, h: 400 },
    scale: 0.8,
    x: -40,
    y: -60,
    z: -900,
    ry: 4,
    rz: -1,
  },
  {
    src: "stills/eld.png",
    rect: { x: 276, y: 82, w: 424, h: 114 },
    scale: 1,
    x: 700,
    y: 420,
    z: 120,
    ry: -26,
    rz: 3,
  },
];
const ARRIVALS = [0, 20, 38, 54, 68, 80, 91, 101, 110, 118, 125, 131];
const WORDS: Array<[number, string]> = [
  [4, "Spreadsheets."],
  [64, "Group chats."],
  [124, "Phone calls."],
  [168, "Missed hours."],
];
const COLLAPSE = 198;

export function Chaos() {
  const frame = useCurrentFrame();
  const collapse = prog(frame, COLLAPSE, 28, inExpo);
  const shake = frame > 150 && frame < COLLAPSE ? (frame - 150) / 48 : 0;
  const sx = (hash(frame) - 0.5) * 8 * shake;
  const sy = (hash(frame + 99) - 0.5) * 8 * shake;
  const camRy = interpolate(frame, [0, 200], [-7, 7]);
  const camScale = interpolate(frame, [0, 200], [0.92, 1.08]);
  const black = frame >= COLLAPSE + 28;

  if (black) return <AbsoluteFill style={{ backgroundColor: C.ink }} />;

  return (
    <Stage>
      <AbsoluteFill style={{ perspective: 1500, perspectiveOrigin: "50% 50%" }}>
        <AbsoluteFill
          style={{
            transformStyle: "preserve-3d",
            transform: `translate(${sx}px, ${sy}px) scale(${camScale}) rotateX(6deg) rotateY(${camRy}deg)`,
          }}
        >
          {FRAGMENTS.map((f, i) => {
            const t = ARRIVALS[i];
            if (frame < t) return null;
            const s = sp(frame, t, { damping: 16, stiffness: 120 });
            const newer = ARRIVALS.filter((a) => a <= frame).length - 1 - i;
            const dim = Math.min(0.68, 0.22 + newer * 0.14);
            const z = lerp(-2600, f.z, s);
            const x = lerp(f.x * 0.2, f.x, s);
            const y = lerp(f.y * 0.2, f.y, s);
            return (
              <div
                key={i}
                style={{
                  position: "absolute",
                  left: 960,
                  top: 540,
                  transformStyle: "preserve-3d",
                  transform: `translate(-50%, -50%) translate3d(${lerp(x, 0, collapse)}px, ${lerp(y, 0, collapse)}px, ${lerp(z, -1400, collapse)}px) rotateY(${f.ry * (1 + (1 - s) * 2)}deg) rotateZ(${f.rz + collapse * (i % 2 ? 40 : -40)}deg) scale(${1 - collapse * 0.8})`,
                  opacity: Math.min(1, s * 1.5) * (1 - collapse),
                  filter: `brightness(${1 - dim})`,
                }}
              >
                <Crop
                  src={f.src}
                  rect={f.rect}
                  scale={f.scale}
                  style={{ boxShadow: "0 30px 80px rgba(0,0,0,0.6)" }}
                />
              </div>
            );
          })}
        </AbsoluteFill>
      </AbsoluteFill>
      <AbsoluteFill style={{ justifyContent: "center", alignItems: "center" }}>
        {WORDS.map(([at, w], i) => {
          const next = WORDS[i + 1]?.[0] ?? COLLAPSE;
          if (frame < at || frame >= next) return null;
          const p = sp(frame, at, { damping: 14, stiffness: 220 });
          return (
            <Headline
              key={w}
              size={150}
              style={{
                position: "absolute",
                transform: `scale(${lerp(1.25, 1, p)})`,
                opacity: Math.min(1, p * 2),
                textShadow: "0 0 40px rgba(0,0,0,0.95), 0 8px 80px rgba(0,0,0,0.95)",
              }}
            >
              {w}
            </Headline>
          );
        })}
      </AbsoluteFill>
    </Stage>
  );
}
