import { AbsoluteFill, useCurrentFrame } from "remotion";
import { Crop } from "../components/Crop";
import { inExpo, lerp, outExpo, prog } from "../components/motion";
import { Stage } from "../components/Stage";
import { Headline } from "../components/Type";
import { C, MONO } from "../theme";
import { BEAT } from "../timeline";

const TILE_W = 800;
const TILE_H = 450;
const GAP = 44;
const FULL = { x: 0, y: 0, w: 1600, h: 900 };

const GRID = [
  ["loads", "loadboard", "invoices", "containers"],
  ["expenses", "home", "payroll", "eld"],
  ["messages", "maintenance", "safety", "customer"],
];

const STOPS: Array<{ word: string; tile: string }> = [
  { word: "Load boards", tile: "loadboard" },
  { word: "Intermodal", tile: "containers" },
  { word: "Payroll", tile: "payroll" },
  { word: "Maintenance", tile: "maintenance" },
  { word: "Safety", tile: "safety" },
  { word: "Customer portal", tile: "customer" },
];
const PULL_BACK = 6 * BEAT;
const CUT = 228;

function tileCenter(name: string): [number, number] {
  for (let r = 0; r < GRID.length; r++) {
    const c = GRID[r].indexOf(name);
    if (c >= 0) return [c * (TILE_W + GAP) + TILE_W / 2, r * (TILE_H + GAP) + TILE_H / 2];
  }
  throw new Error(name);
}

const WALL_W = 4 * TILE_W + 3 * GAP;
const WALL_H = 3 * TILE_H + 2 * GAP;

export function Montage() {
  const frame = useCurrentFrame();
  if (frame >= CUT) return <AbsoluteFill style={{ backgroundColor: C.ink }} />;

  const idx = Math.min(STOPS.length - 1, Math.floor(frame / BEAT));
  const prev = Math.max(0, idx - 1);
  const move = idx === 0 ? 1 : prog(frame, idx * BEAT, 16, outExpo);
  const [ax, ay] = tileCenter(STOPS[prev].tile);
  const [bx, by] = tileCenter(STOPS[idx].tile);
  const pull = prog(frame, PULL_BACK, 36, outExpo);
  const tx = lerp(lerp(ax, bx, move), WALL_W / 2, pull);
  const ty = lerp(lerp(ay, by, move), WALL_H / 2, pull);
  const zoom = lerp(1.32, 0.52, pull) * (1 + prog(frame, 214, 14, inExpo) * 0.6);
  const focus = pull > 0 ? null : STOPS[idx].tile;
  const wordIn = prog(frame, idx * BEAT, 14, outExpo);

  return (
    <Stage>
      <AbsoluteFill style={{ perspective: 1900 }}>
        <div
          style={{
            position: "absolute",
            left: 960,
            top: 540,
            width: WALL_W,
            height: WALL_H,
            transformStyle: "preserve-3d",
            transformOrigin: "0 0",
            transform: `rotateX(${lerp(22, 14, pull)}deg) rotateZ(${lerp(-7, -4, pull)}deg) scale(${zoom}) translate(${-tx}px, ${-ty}px)`,
          }}
        >
          {GRID.flatMap((row, r) =>
            row.map((name, c) => {
              const lit = focus === null || focus === name;
              return (
                <div
                  key={name}
                  style={{
                    position: "absolute",
                    left: c * (TILE_W + GAP),
                    top: r * (TILE_H + GAP),
                    filter: `brightness(${focus === null ? lerp(1, 0.5, prog(frame, PULL_BACK + 6, 20)) : lit ? 1 : 0.32})`,
                  }}
                >
                  <Crop
                    src={`tiles/${name}.jpg`}
                    rect={FULL}
                    scale={0.5}
                    radius={12}
                    style={{ boxShadow: "0 30px 80px rgba(0,0,0,0.6)" }}
                  />
                </div>
              );
            }),
          )}
        </div>
      </AbsoluteFill>
      <AbsoluteFill style={{ justifyContent: "flex-end", padding: "0 140px 110px" }}>
        {pull === 0 ? (
          <div style={{ opacity: wordIn, transform: `translateY(${lerp(30, 0, wordIn)}px)` }}>
            <div
              style={{ fontFamily: MONO, fontSize: 22, letterSpacing: "0.16em", color: C.muted }}
            >
              {String(idx + 1).padStart(2, "0")} / {String(STOPS.length).padStart(2, "0")}
            </div>
            <Headline size={120} style={{ textShadow: "0 6px 50px rgba(0,0,0,0.95)" }}>
              {STOPS[idx].word}
            </Headline>
          </div>
        ) : (
          <AbsoluteFill style={{ justifyContent: "center", alignItems: "center" }}>
            <Headline
              size={150}
              style={{
                textShadow: "0 6px 60px rgba(0,0,0,0.95)",
                opacity: prog(frame, PULL_BACK + 6, 16),
                transform: `scale(${lerp(1.15, 1, prog(frame, PULL_BACK + 6, 30))})`,
              }}
            >
              One system.
            </Headline>
          </AbsoluteFill>
        )}
      </AbsoluteFill>
    </Stage>
  );
}
