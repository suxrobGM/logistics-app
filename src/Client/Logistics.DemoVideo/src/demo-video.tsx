import type { ReactElement } from "react";
import { linearTiming, TransitionSeries } from "@remotion/transitions";
import { fade } from "@remotion/transitions/fade";
import { slide } from "@remotion/transitions/slide";
import { SCENE_DURATIONS, TRANSITION_FRAMES } from "@/lib/constants";
import {
  SceneAiDispatch,
  SceneBrandIntro,
  SceneCta,
  SceneDashboard,
  SceneInvoicing,
  SceneLoads,
  SceneMultiPlatform,
  SceneProblem,
} from "@/scenes";

const T = linearTiming({ durationInFrames: TRANSITION_FRAMES });

export function DemoVideo(): ReactElement {
  return (
    <TransitionSeries>
      {/* Scene 1: Brand Intro (5s) */}
      <TransitionSeries.Sequence durationInFrames={SCENE_DURATIONS.brandIntro}>
        <SceneBrandIntro />
      </TransitionSeries.Sequence>

      <TransitionSeries.Transition presentation={fade()} timing={T} />

      {/* Scene 2: Problem Statement (5s) */}
      <TransitionSeries.Sequence durationInFrames={SCENE_DURATIONS.problem}>
        <SceneProblem />
      </TransitionSeries.Sequence>

      <TransitionSeries.Transition presentation={slide({ direction: "from-right" })} timing={T} />

      {/* Scene 3: Dashboard (8s) */}
      <TransitionSeries.Sequence durationInFrames={SCENE_DURATIONS.dashboard}>
        <SceneDashboard />
      </TransitionSeries.Sequence>

      <TransitionSeries.Transition presentation={fade()} timing={T} />

      {/* Scene 4: Loads Management (8s) */}
      <TransitionSeries.Sequence durationInFrames={SCENE_DURATIONS.loads}>
        <SceneLoads />
      </TransitionSeries.Sequence>

      <TransitionSeries.Transition presentation={slide({ direction: "from-bottom" })} timing={T} />

      {/* Scene 5: AI Dispatch — Hero (12s) */}
      <TransitionSeries.Sequence durationInFrames={SCENE_DURATIONS.aiDispatch}>
        <SceneAiDispatch />
      </TransitionSeries.Sequence>

      <TransitionSeries.Transition presentation={fade()} timing={T} />

      {/* Scene 6: Invoicing (7s) */}
      <TransitionSeries.Sequence durationInFrames={SCENE_DURATIONS.invoicing}>
        <SceneInvoicing />
      </TransitionSeries.Sequence>

      <TransitionSeries.Transition presentation={fade()} timing={T} />

      {/* Scene 7: Multi-Platform (5s) */}
      <TransitionSeries.Sequence durationInFrames={SCENE_DURATIONS.multiPlatform}>
        <SceneMultiPlatform />
      </TransitionSeries.Sequence>

      <TransitionSeries.Transition presentation={fade()} timing={T} />

      {/* Scene 8: Call to Action (~13.5s) */}
      <TransitionSeries.Sequence durationInFrames={SCENE_DURATIONS.cta}>
        <SceneCta />
      </TransitionSeries.Sequence>
    </TransitionSeries>
  );
}
