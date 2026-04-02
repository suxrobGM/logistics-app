import type { ReactElement } from "react";
import { AbsoluteFill, Sequence } from "remotion";
import {
  AnimatedText,
  FeaturePill,
  GlowButton,
  GradientBackground,
  Logo,
  TypewriterText,
} from "@/components";

export function SceneCta(): ReactElement {
  return (
    <AbsoluteFill>
      <GradientBackground />
      <AbsoluteFill className="flex flex-col items-center justify-center">
        <Sequence from={0} layout="none">
          <Logo size={100} animateDrawOn={false} />
        </Sequence>

        <div className="mt-8">
          <TypewriterText
            text="Ready to modernize your fleet?"
            delay={10}
            frameDuration={40}
            fontSize={48}
            fontWeight={700}
          />
        </div>

        <div className="mt-9 flex flex-wrap justify-center">
          <FeaturePill label="AI Dispatch" delay={60} color="var(--color-primary)" />
          <FeaturePill label="Real-Time Tracking" delay={66} color="var(--color-accent-blue)" />
          <FeaturePill label="Fleet Compliance" delay={72} color="var(--color-accent-green)" />
          <FeaturePill label="Smart Invoicing" delay={78} color="var(--color-accent-purple)" />
        </div>

        <div className="mt-11">
          <GlowButton label="Request a Demo" delay={95} />
        </div>

        <div className="mt-7">
          <AnimatedText
            text="logisticsx.app"
            delay={120}
            fontSize={22}
            fontWeight={500}
            color="var(--color-text-muted)"
          />
        </div>
      </AbsoluteFill>
    </AbsoluteFill>
  );
}
