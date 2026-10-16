import { geoAlbersUsa, geoPath } from "d3-geo";
import { AbsoluteFill, interpolate, useCurrentFrame } from "remotion";
import { feature, mesh } from "topojson-client";
import type { GeometryCollection, Topology } from "topojson-specification";
import us from "us-atlas/states-albers-10m.json";
import { inExpo, inOutCubic, lerp, prog } from "../components/motion";
import { Stage } from "../components/Stage";
import { Label, Words } from "../components/Type";
import { C, MONO } from "../theme";

const topo = us as unknown as Topology<{ states: GeometryCollection; nation: GeometryCollection }>;
const path = geoPath();
const STATE_LINES = path(mesh(topo, topo.objects.states, (a, b) => a !== b)) ?? "";
const NATION = path(feature(topo, topo.objects.nation)) ?? "";
/** Matches the projection us-atlas used to pre-project its albers files. */
const project = geoAlbersUsa().scale(1300).translate([487.5, 305]);

const CITIES: Record<string, [number, number]> = {
  Minneapolis: [-93.27, 44.98],
  Chicago: [-87.63, 41.88],
  Denver: [-104.99, 39.74],
  "Salt Lake City": [-111.89, 40.76],
  Charlotte: [-80.84, 35.23],
  Richmond: [-77.44, 37.54],
  "Kansas City": [-94.58, 39.1],
  Omaha: [-95.94, 41.26],
  Atlanta: [-84.39, 33.75],
  Nashville: [-86.78, 36.16],
  Phoenix: [-112.07, 33.45],
  "Los Angeles": [-118.24, 34.05],
  Dallas: [-96.8, 32.78],
  Houston: [-95.37, 29.76],
  Indianapolis: [-86.16, 39.77],
  Memphis: [-90.05, 35.15],
  "St. Louis": [-90.2, 38.63],
  Columbus: [-83.0, 39.96],
  Detroit: [-83.05, 42.33],
  Birmingham: [-86.8, 33.52],
  Seattle: [-122.33, 47.61],
  Portland: [-122.68, 45.52],
};

/** Lanes the demo tenant actually runs. */
const ROUTES: Array<[string, string]> = [
  ["Minneapolis", "Chicago"],
  ["Phoenix", "Los Angeles"],
  ["Atlanta", "Nashville"],
  ["Denver", "Salt Lake City"],
  ["Dallas", "Houston"],
  ["Charlotte", "Richmond"],
  ["Kansas City", "Omaha"],
  ["Chicago", "Indianapolis"],
  ["Memphis", "St. Louis"],
  ["Columbus", "Detroit"],
  ["Birmingham", "Atlanta"],
  ["Portland", "Seattle"],
];

const pt = (name: string) => project(CITIES[name]) as [number, number];

function arc(a: [number, number], b: [number, number]) {
  const mx = (a[0] + b[0]) / 2;
  const my = (a[1] + b[1]) / 2;
  const dx = b[0] - a[0];
  const dy = b[1] - a[1];
  const c: [number, number] = [mx - dy * 0.3, my + dx * 0.3 - Math.hypot(dx, dy) * 0.15];
  return { c, d: `M${a[0]},${a[1]} Q${c[0]},${c[1]} ${b[0]},${b[1]}` };
}

function bezier(a: [number, number], c: [number, number], b: [number, number], t: number) {
  const u = 1 - t;
  return [
    u * u * a[0] + 2 * u * t * c[0] + t * t * b[0],
    u * u * a[1] + 2 * u * t * c[1] + t * t * b[1],
  ];
}

const MAP_SCALE = 1.75;

export function NetworkMap() {
  const frame = useCurrentFrame();
  const tilt = prog(frame, 0, 70, inOutCubic);
  const push = interpolate(frame, [0, 240], [0, 1]);
  const exit = prog(frame, 214, 26, inExpo);
  const usedCities = new Set(ROUTES.flat());

  return (
    <Stage grid={false}>
      <AbsoluteFill style={{ perspective: 1700, perspectiveOrigin: "50% 30%" }}>
        <AbsoluteFill
          style={{
            justifyContent: "center",
            alignItems: "center",
            transformStyle: "preserve-3d",
            transform: `translateY(${lerp(0, 190, tilt)}px) rotateX(${lerp(0, 42, tilt)}deg) rotateZ(${lerp(-3, 3, push)}deg) scale(${lerp(0.92, 1.12, push) + exit * 1.6})`,
            opacity: 1 - exit,
          }}
        >
          <svg
            width={975 * MAP_SCALE}
            height={610 * MAP_SCALE}
            viewBox="0 0 975 610"
            style={{ overflow: "visible" }}
          >
            <path d={NATION} fill="#12161D" stroke="rgba(255,255,255,0.28)" strokeWidth={0.8} />
            <path d={STATE_LINES} fill="none" stroke="rgba(255,255,255,0.09)" strokeWidth={0.6} />
            {ROUTES.map(([from, to], i) => {
              const a = pt(from);
              const b = pt(to);
              const { c, d } = arc(a, b);
              const start = 24 + i * 11;
              const draw = prog(frame, start, 40, inOutCubic);
              const travel = ((frame - start - 20) / 70) % 1;
              const [tx, ty] = bezier(a, c, b, Math.max(0, travel));
              const arrive = prog(frame, start + 30, 30);
              return (
                <g key={i}>
                  <path
                    d={d}
                    fill="none"
                    stroke="rgba(255,255,255,0.85)"
                    strokeWidth={1.3}
                    pathLength={1}
                    strokeDasharray={1}
                    strokeDashoffset={1 - draw}
                    strokeLinecap="round"
                  />
                  {draw > 0.98 && <circle cx={tx} cy={ty} r={3.2} fill={C.brand} />}
                  <circle
                    cx={b[0]}
                    cy={b[1]}
                    r={lerp(2, 16, arrive)}
                    fill="none"
                    stroke={C.brand}
                    strokeWidth={0.8}
                    opacity={(1 - arrive) * 0.9}
                  />
                </g>
              );
            })}
            {[...usedCities].map((name, i) => {
              const [x, y] = pt(name);
              const firstUse = ROUTES.findIndex((r) => r.includes(name));
              const on = prog(frame, 20 + firstUse * 11, 20);
              return (
                <g key={name} opacity={on}>
                  <circle cx={x} cy={y} r={2.6} fill={C.text} />
                  <text
                    x={x + 6}
                    y={y - 5}
                    fontFamily={MONO}
                    fontSize={8.5}
                    fill="rgba(255,255,255,0.6)"
                    letterSpacing={0.4}
                  >
                    {name.toUpperCase()}
                  </text>
                </g>
              );
            })}
          </svg>
        </AbsoluteFill>
      </AbsoluteFill>
      <AbsoluteFill style={{ padding: "96px 140px", opacity: 1 - exit }}>
        <Label text="Live tracking" at={34} />
        <Words
          text="Every truck. Every load. Live."
          at={40}
          stagger={5}
          size={84}
          style={{ marginTop: 22, maxWidth: 900 }}
          accent={["Live."]}
        />
      </AbsoluteFill>
    </Stage>
  );
}
