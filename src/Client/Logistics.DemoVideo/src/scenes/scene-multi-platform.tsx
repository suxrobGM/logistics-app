import type { ReactElement } from "react";
import { AbsoluteFill, Img, spring, staticFile, useCurrentFrame, useVideoConfig } from "remotion";
import { AnimatedText, GradientBackground } from "@/components";
import { fontFamily } from "@/lib/fonts";

export function SceneMultiPlatform(): ReactElement {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();

  return (
    <AbsoluteFill>
      <GradientBackground />
      <AbsoluteFill className="flex flex-col items-center justify-center">
        <AnimatedText text="One Platform. Every Device." delay={0} fontSize={40} fontWeight={700} />

        <div className="mt-12 flex items-end gap-10">
          {/* Desktop - TMS Portal in browser frame */}
          <DeviceFrame type="desktop" label="TMS Portal" delay={15} frame={frame} fps={fps}>
            <Img
              src={staticFile("screenshots/tms-dashboard.png")}
              className="block size-full object-cover object-top"
            />
          </DeviceFrame>

          {/* Tablet - Customer Portal (landscape screenshot, shown in wider frame) */}
          <DeviceFrame type="tablet" label="Customer Portal" delay={25} frame={frame} fps={fps}>
            <Img
              src={staticFile("screenshots/customer-portal.png")}
              className="block size-full object-cover object-top-left"
            />
          </DeviceFrame>

          {/* Phone - Driver App (screenshot already includes phone bezel) */}
          <PhoneScreenshot delay={35} frame={frame} fps={fps} />
        </div>
      </AbsoluteFill>
    </AbsoluteFill>
  );
}

interface DeviceFrameProps {
  type: "desktop" | "tablet";
  label: string;
  delay: number;
  frame: number;
  fps: number;
  children: ReactElement;
}

function DeviceFrame(props: DeviceFrameProps): ReactElement {
  const { type, label, delay, frame, fps, children } = props;

  const scale = spring({
    fps,
    frame: Math.max(0, frame - delay),
    config: { damping: 200, stiffness: 80 },
  });

  const sizes = {
    desktop: { width: 680, height: 440 },
    tablet: { width: 400, height: 300 },
  };

  const { width, height } = sizes[type];

  return (
    <div
      className="flex flex-col items-center"
      style={{ transform: `scale(${scale})`, opacity: scale }}
    >
      <div
        className="border-border-default bg-surface overflow-hidden border-2"
        style={{
          width,
          height,
          borderRadius: 16,
          boxShadow: "0 25px 60px rgba(0,0,0,0.12), 0 0 40px rgba(8, 145, 178, 0.06)",
        }}
      >
        {/* Browser chrome */}
        <div className="border-border-subtle bg-elevated flex h-7 items-center gap-1.5 border-b px-3">
          <div className="bg-accent-red size-2 rounded-full" />
          <div className="bg-accent-yellow size-2 rounded-full" />
          <div className="bg-accent-green size-2 rounded-full" />
        </div>
        {/* Content */}
        <div style={{ height: height - 28, overflow: "hidden" }}>{children}</div>
      </div>
      <div className="text-text-secondary mt-4 text-sm font-medium" style={{ fontFamily }}>
        {label}
      </div>
    </div>
  );
}

interface PhoneScreenshotProps {
  delay: number;
  frame: number;
  fps: number;
}

function PhoneScreenshot(props: PhoneScreenshotProps): ReactElement {
  const { delay, frame, fps } = props;

  const scale = spring({
    fps,
    frame: Math.max(0, frame - delay),
    config: { damping: 200, stiffness: 80 },
  });

  return (
    <div
      className="flex flex-col items-center"
      style={{ transform: `scale(${scale})`, opacity: scale }}
    >
      <Img
        src={staticFile("screenshots/driver-app.png")}
        style={{
          height: 440,
          width: "auto",
          filter: "drop-shadow(0 25px 60px rgba(0,0,0,0.15))",
        }}
      />
      <div className="text-text-secondary mt-4 text-sm font-medium" style={{ fontFamily }}>
        Driver App
      </div>
    </div>
  );
}
