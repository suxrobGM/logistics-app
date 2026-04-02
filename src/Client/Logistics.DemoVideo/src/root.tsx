import "./index.css";
import type { ReactElement } from "react";
import { Composition } from "remotion";
import { DemoVideo } from "./demo-video";

export function RemotionRoot(): ReactElement {
  return (
    <Composition
      id="DemoVideo"
      component={DemoVideo}
      durationInFrames={1800}
      fps={30}
      width={1920}
      height={1080}
    />
  );
}
