import { useCurrentFrame } from "remotion";
import { outBack, prog, sp } from "./motion";

const HEX = "M0,-72 L62,-36 L62,36 L0,72 L-62,36 L-62,-36 Z";
const HEX_LEN = 6 * 72;
const NODES: Array<[number, number, string]> = [
  [-32, -32, "#3B82F6"],
  [32, -32, "#0EA5E9"],
  [32, 32, "#06B6D4"],
  [-32, 32, "#0EA5E9"],
];

/** The LogisticsX mark (wwwroot/logo.svg), drawn stroke by stroke from frame `at`. */
export function Logo({ at, size = 200 }: { at: number; size?: number }) {
  const frame = useCurrentFrame();
  const hex = prog(frame, at, 34);
  const cross = prog(frame, at + 10, 26);
  const centre = sp(frame, at + 22, { damping: 11, stiffness: 160 });
  const crossLen = 90.5;
  return (
    <svg width={size} height={size} viewBox="-100 -100 200 200" style={{ overflow: "visible" }}>
      <defs>
        <linearGradient id="lx" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stopColor="#3B82F6" />
          <stop offset="100%" stopColor="#06B6D4" />
        </linearGradient>
      </defs>
      <path
        d={HEX}
        fill="none"
        stroke="url(#lx)"
        strokeWidth={3.5}
        strokeLinejoin="round"
        strokeDasharray={HEX_LEN}
        strokeDashoffset={HEX_LEN * (1 - hex)}
      />
      <line
        x1={-32}
        y1={-32}
        x2={32}
        y2={32}
        stroke="url(#lx)"
        strokeWidth={3.5}
        strokeLinecap="round"
        strokeDasharray={crossLen}
        strokeDashoffset={crossLen * (1 - cross)}
      />
      <line
        x1={32}
        y1={-32}
        x2={-32}
        y2={32}
        stroke="url(#lx)"
        strokeWidth={3.5}
        strokeLinecap="round"
        strokeDasharray={crossLen}
        strokeDashoffset={crossLen * (1 - cross)}
      />
      <path
        d="M-32,-32 Q-42,0 -32,32"
        fill="none"
        stroke="#3B82F6"
        strokeWidth={1.2}
        opacity={0.3 * cross}
      />
      <path
        d="M32,-32 Q42,0 32,32"
        fill="none"
        stroke="#06B6D4"
        strokeWidth={1.2}
        opacity={0.3 * cross}
      />
      {NODES.map(([x, y, c], i) => {
        const s = prog(frame, at + 18 + i * 3, 16, outBack);
        return <circle key={i} cx={x} cy={y} r={5 * s} fill={c} />;
      })}
      <circle cx={0} cy={0} r={8 * centre} fill="url(#lx)" />
      <circle cx={0} cy={0} r={3.5 * centre} fill="white" />
    </svg>
  );
}
