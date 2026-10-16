import { Composition } from "remotion";
import { Teaser } from "./Teaser";
import { FPS, TOTAL } from "./timeline";

export function Root() {
  return (
    <Composition
      id="Teaser"
      component={Teaser}
      durationInFrames={TOTAL}
      fps={FPS}
      width={1920}
      height={1080}
    />
  );
}
