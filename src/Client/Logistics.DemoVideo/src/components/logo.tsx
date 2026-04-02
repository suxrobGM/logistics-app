import type { ReactElement } from "react";
import { interpolate, spring, useCurrentFrame, useVideoConfig } from "remotion";

interface LogoProps {
  size?: number;
  animateDrawOn?: boolean;
  delay?: number;
}

export function Logo(props: LogoProps): ReactElement {
  const { size = 200, animateDrawOn = true, delay = 0 } = props;
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();
  const delayedFrame = Math.max(0, frame - delay);

  // Hexagon path total length ≈ 432, cross lines ≈ 181
  const hexLength = 432;
  const lineLength = 91;

  const drawProgress = animateDrawOn
    ? interpolate(delayedFrame, [0, 40], [0, 1], {
        extrapolateLeft: "clamp",
        extrapolateRight: "clamp",
      })
    : 1;

  const nodeScale = animateDrawOn
    ? spring({
        fps,
        frame: Math.max(0, delayedFrame - 30),
        config: { damping: 200, stiffness: 120 },
      })
    : 1;

  const centerScale = animateDrawOn
    ? spring({
        fps,
        frame: Math.max(0, delayedFrame - 25),
        config: { damping: 200, stiffness: 120 },
      })
    : 1;

  return (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 200 200" width={size} height={size}>
      <defs>
        <linearGradient id="sGrad" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" style={{ stopColor: "#3B82F6" }} />
          <stop offset="100%" style={{ stopColor: "#06B6D4" }} />
        </linearGradient>
      </defs>

      <g transform="translate(100, 100)">
        {/* Hexagon */}
        <path
          d="M0,-72 L62,-36 L62,36 L0,72 L-62,36 L-62,-36 Z"
          fill="none"
          stroke="url(#sGrad)"
          strokeWidth="3.5"
          strokeLinejoin="round"
          strokeDasharray={hexLength}
          strokeDashoffset={hexLength * (1 - drawProgress)}
        />

        {/* Cross lines */}
        <line
          x1="-32"
          y1="-32"
          x2="32"
          y2="32"
          stroke="url(#sGrad)"
          strokeWidth="3.5"
          strokeLinecap="round"
          strokeDasharray={lineLength}
          strokeDashoffset={lineLength * (1 - drawProgress)}
        />
        <line
          x1="32"
          y1="-32"
          x2="-32"
          y2="32"
          stroke="url(#sGrad)"
          strokeWidth="3.5"
          strokeLinecap="round"
          strokeDasharray={lineLength}
          strokeDashoffset={lineLength * (1 - drawProgress)}
        />

        {/* Center node */}
        <circle cx="0" cy="0" r="8" fill="url(#sGrad)" transform={`scale(${centerScale})`} />
        <circle cx="0" cy="0" r="3.5" fill="white" transform={`scale(${centerScale})`} />

        {/* Corner nodes */}
        {[
          { cx: -32, cy: -32, fill: "#3B82F6" },
          { cx: 32, cy: -32, fill: "#0EA5E9" },
          { cx: 32, cy: 32, fill: "#06B6D4" },
          { cx: -32, cy: 32, fill: "#0EA5E9" },
        ].map((node, i) => (
          <circle
            key={i}
            cx={node.cx}
            cy={node.cy}
            r={5}
            fill={node.fill}
            transform={`translate(${node.cx * (1 - nodeScale)}, ${node.cy * (1 - nodeScale)}) scale(${nodeScale})`}
            style={{ transformOrigin: `${node.cx}px ${node.cy}px` }}
          />
        ))}

        {/* Decorative arcs */}
        <path
          d="M-32,-32 Q-42,0 -32,32"
          fill="none"
          stroke="#3B82F6"
          strokeWidth="1.2"
          opacity={0.3 * drawProgress}
        />
        <path
          d="M32,-32 Q42,0 32,32"
          fill="none"
          stroke="#06B6D4"
          strokeWidth="1.2"
          opacity={0.3 * drawProgress}
        />
      </g>
    </svg>
  );
}
