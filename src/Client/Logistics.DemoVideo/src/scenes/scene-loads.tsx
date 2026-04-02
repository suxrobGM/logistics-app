import type { ReactElement } from "react";
import { AbsoluteFill, Sequence } from "remotion";
import { AnimatedText, ScreenshotFrame } from "@/components";

export function SceneLoads(): ReactElement {
  return (
    <AbsoluteFill className="bg-base">
      <ScreenshotFrame
        src="screenshots/tms-loads.png"
        delay={5}
        startScale={1.05}
        endScale={1.02}
        panX={10}
        durationFrames={200}
      />

      <AbsoluteFill className="justify-end bg-linear-to-t from-black/50 to-transparent px-15 pb-12">
        <Sequence from={20} layout="none">
          <AnimatedText
            text="Track Every Load. Pickup to Delivery."
            delay={20}
            fontSize={36}
            fontWeight={700}
            color="#ffffff"
            textAlign="left"
          />
        </Sequence>
        <Sequence from={40} layout="none">
          <AnimatedText
            text="Filter, sort, and manage loads across your entire fleet"
            delay={40}
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
