import type { ReactElement } from "react";
import { AbsoluteFill, interpolate, Sequence, useCurrentFrame } from "remotion";
import { AnimatedText, ScreenshotFrame } from "@/components";

// AI Dispatch hero scene - 510 frames (17s), split into 3 parts:
//   Part 1 (0-169):     Sessions list with map + pending decisions
//   Part 2 (170-339):   Session #4 detail - summary, assignments, issues
//   Part 3 (340-509):   Agent timeline - tool calls, reasoning, approvals
const PART2_START = 170;
const PART3_START = 340;

export function SceneAiDispatch(): ReactElement {
  const frame = useCurrentFrame();

  return (
    <AbsoluteFill className="bg-base">
      <Part
        screenshotSrc="screenshots/tms-ai-dispatch.png"
        headline="AI That Thinks, Decides, and Dispatches."
        subline="Review AI suggestions or let it run autonomously"
        startFrame={0}
        endFrame={PART2_START}
        kenBurns={{ startScale: 1.06, endScale: 1.0, panY: -5 }}
        frame={frame}
      />

      <Part
        screenshotSrc="screenshots/tms-ai-session-top.png"
        headline="Full Dispatch Summary with HOS Compliance"
        subline="Assignments, issues, and recommendations - all in one view"
        startFrame={PART2_START}
        endFrame={PART3_START}
        kenBurns={{ startScale: 1.04, endScale: 1.0, panY: -3 }}
        frame={frame}
      />

      <Part
        screenshotSrc="screenshots/tms-ai-session-timeline.png"
        headline="Every Decision Traced - Full Agent Timeline"
        subline="Tool calls, feasibility checks, and load assignments step by step"
        startFrame={PART3_START}
        endFrame={510}
        kenBurns={{ startScale: 1.04, endScale: 1.0, panY: 5 }}
        frame={frame}
      />
    </AbsoluteFill>
  );
}

interface KenBurnsConfig {
  startScale: number;
  endScale: number;
  panX?: number;
  panY?: number;
}

interface PartProps {
  screenshotSrc: string;
  headline: string;
  subline: string;
  startFrame: number;
  endFrame: number;
  kenBurns: KenBurnsConfig;
  frame: number;
}

function Part(props: PartProps): ReactElement {
  const { screenshotSrc, headline, subline, startFrame, endFrame, kenBurns, frame } = props;

  const fadeIn = interpolate(frame, [startFrame, startFrame + 15], [0, 1], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });
  const fadeOut = interpolate(frame, [endFrame - 15, endFrame], [1, 0], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });
  const opacity = Math.min(fadeIn, fadeOut);

  if (frame < startFrame - 1 || frame > endFrame + 1) return <></>;

  return (
    <AbsoluteFill style={{ opacity }}>
      <ScreenshotFrame
        src={screenshotSrc}
        delay={startFrame}
        startScale={kenBurns.startScale}
        endScale={kenBurns.endScale}
        panX={kenBurns.panX ?? 0}
        panY={kenBurns.panY ?? 0}
        durationFrames={endFrame - startFrame}
      />

      <AbsoluteFill className="justify-end bg-linear-to-t from-black/55 to-transparent px-15 pb-12">
        <Sequence from={startFrame + 20} layout="none">
          <AnimatedText
            text={headline}
            delay={startFrame + 20}
            fontSize={36}
            fontWeight={700}
            color="#ffffff"
            textAlign="left"
          />
        </Sequence>
        <Sequence from={startFrame + 40} layout="none">
          <AnimatedText
            text={subline}
            delay={startFrame + 40}
            fontSize={20}
            fontWeight={400}
            color="rgba(255,255,255,0.8)"
            textAlign="left"
          />
        </Sequence>
      </AbsoluteFill>
    </AbsoluteFill>
  );
}
