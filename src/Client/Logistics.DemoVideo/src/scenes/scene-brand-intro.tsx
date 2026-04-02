import type { ReactElement } from "react";
import { AbsoluteFill, Sequence } from "remotion";
import { AnimatedText, GradientBackground, Logo, TypewriterText } from "@/components";

export function SceneBrandIntro(): ReactElement {
  return (
    <AbsoluteFill>
      <GradientBackground />
      <AbsoluteFill className="flex flex-col items-center justify-center">
        <Sequence from={0} layout="none">
          <Logo size={140} animateDrawOn delay={0} />
        </Sequence>

        <div className="mt-7">
          <AnimatedText text="LogisticsX" delay={25} fontSize={64} fontWeight={700} />
        </div>

        <div className="mt-4">
          <TypewriterText
            text="Your Fleet, Dispatched by AI"
            delay={50}
            frameDuration={50}
            fontSize={28}
            fontWeight={400}
            color="var(--color-text-secondary)"
          />
        </div>
      </AbsoluteFill>
    </AbsoluteFill>
  );
}
