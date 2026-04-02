import type { ReactElement } from "react";
import { AbsoluteFill, Sequence } from "remotion";
import { AnimatedText, ScreenshotFrame } from "@/components";

export function SceneDashboard(): ReactElement {
  return (
    <AbsoluteFill className="bg-base">
      <ScreenshotFrame
        src="screenshots/tms-dashboard.png"
        delay={5}
        startScale={1.06}
        endScale={1.0}
        panY={-8}
        durationFrames={230}
      />

      <AbsoluteFill className="justify-end bg-linear-to-t from-black/50 to-transparent px-15 pb-12">
        <Sequence from={20} layout="none">
          <AnimatedText
            text="One Dashboard. Total Visibility."
            delay={20}
            fontSize={36}
            fontWeight={700}
            color="#ffffff"
            textAlign="left"
          />
        </Sequence>
        <Sequence from={40} layout="none">
          <AnimatedText
            text="Revenue, fleet status, and team performance at a glance"
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
