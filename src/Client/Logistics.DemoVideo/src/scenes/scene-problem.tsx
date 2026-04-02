import type { ReactElement } from "react";
import { AbsoluteFill } from "remotion";
import { AnimatedText, GradientBackground } from "@/components";

export function SceneProblem(): ReactElement {
  return (
    <AbsoluteFill>
      <GradientBackground />
      <AbsoluteFill className="flex flex-col items-center justify-center gap-5">
        <AnimatedText
          text="Managing a fleet shouldn't mean"
          delay={5}
          fontSize={36}
          fontWeight={400}
          color="var(--color-text-secondary)"
        />
        <AnimatedText
          text="drowning in spreadsheets."
          delay={15}
          fontSize={36}
          fontWeight={400}
          color="var(--color-text-secondary)"
        />

        <div className="h-8" />

        <div className="flex items-center gap-10">
          <AnimatedText
            text="Missed loads."
            delay={40}
            fontSize={28}
            fontWeight={600}
            color="var(--color-accent-red)"
          />
          <AnimatedText
            text="Compliance headaches."
            delay={55}
            fontSize={28}
            fontWeight={600}
            color="var(--color-accent-yellow)"
          />
          <AnimatedText
            text="Manual dispatch."
            delay={70}
            fontSize={28}
            fontWeight={600}
            color="var(--color-accent-orange)"
          />
        </div>

        <div className="h-6" />

        <AnimatedText
          text="There's a better way."
          delay={100}
          fontSize={44}
          fontWeight={700}
          color="var(--color-primary)"
        />
      </AbsoluteFill>
    </AbsoluteFill>
  );
}
