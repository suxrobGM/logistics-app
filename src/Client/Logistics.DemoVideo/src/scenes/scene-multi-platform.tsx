import type { ReactElement } from "react";
import { AbsoluteFill, spring, useCurrentFrame, useVideoConfig } from "remotion";
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
          <DeviceFrame type="desktop" label="TMS Portal" delay={15} frame={frame} fps={fps}>
            <DesktopMockup />
          </DeviceFrame>
          <DeviceFrame type="tablet" label="Customer Portal" delay={25} frame={frame} fps={fps}>
            <TabletMockup />
          </DeviceFrame>
          <DeviceFrame type="phone" label="Driver App" delay={35} frame={frame} fps={fps}>
            <PhoneMockup />
          </DeviceFrame>
        </div>
      </AbsoluteFill>
    </AbsoluteFill>
  );
}

interface DeviceFrameProps {
  type: "desktop" | "tablet" | "phone";
  label: string;
  delay: number;
  frame: number;
  fps: number;
  children: React.ReactElement;
}

function DeviceFrame(props: DeviceFrameProps): React.ReactElement {
  const { type, label, delay, frame, fps, children } = props;

  const scale = spring({
    fps,
    frame: Math.max(0, frame - delay),
    config: { damping: 200, stiffness: 80 },
  });

  const sizes = {
    desktop: { width: 700, height: 440 },
    tablet: { width: 360, height: 480 },
    phone: { width: 220, height: 440 },
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
          borderRadius: type === "phone" ? 24 : 16,
          boxShadow: "0 25px 60px rgba(0,0,0,0.12), 0 0 40px rgba(8, 145, 178, 0.06)",
        }}
      >
        <div
          className="border-border-subtle bg-elevated flex items-center justify-center gap-1.5 border-b px-3"
          style={{ height: type === "phone" ? 32 : 28 }}
        >
          {type === "desktop" && (
            <>
              <div className="bg-accent-red size-2 rounded-full" />
              <div className="bg-accent-yellow size-2 rounded-full" />
              <div className="bg-accent-green size-2 rounded-full" />
            </>
          )}
        </div>
        <div className="flex-1 overflow-hidden">{children}</div>
      </div>
      <div className="text-text-secondary mt-4 text-sm font-medium" style={{ fontFamily }}>
        {label}
      </div>
    </div>
  );
}

function DesktopMockup(): React.ReactElement {
  return (
    <div className="bg-base flex h-full">
      <div className="border-border-subtle bg-sidebar flex w-10 flex-col items-center gap-1.5 border-r py-2">
        {Array.from({ length: 7 }).map((_, i) => (
          <div
            key={i}
            className="size-6 rounded-md"
            style={{
              backgroundColor: i === 1 ? "var(--color-primary-dim)" : "transparent",
              borderLeft: i === 1 ? "2px solid var(--color-primary)" : "2px solid transparent",
            }}
          />
        ))}
      </div>
      <div className="flex-1 p-3">
        <div className="mb-2.5 flex gap-2">
          {["accent-blue", "accent-green", "accent-green", "accent-purple"].map((c, i) => (
            <div
              key={i}
              className="border-border-subtle bg-surface flex flex-1 items-center gap-1.5 rounded-lg border px-2"
              style={{ height: 36 }}
            >
              <div
                className="size-5 rounded-full"
                style={{ backgroundColor: `var(--color-${c})15` }}
              />
              <div>
                <div className="bg-border-subtle mb-0.5 h-1 w-10 rounded-sm" />
                <div className="bg-text-muted h-1.5 w-7 rounded-sm opacity-40" />
              </div>
            </div>
          ))}
        </div>
        <div className="flex gap-2">
          {[1, 2, 3].map((i) => (
            <div
              key={i}
              className="border-border-subtle bg-surface h-30 flex-1 rounded-lg border"
            />
          ))}
        </div>
      </div>
    </div>
  );
}

function TabletMockup(): React.ReactElement {
  return (
    <div className="bg-base h-full p-3">
      <div className="mb-3 flex items-center gap-2">
        <div className="bg-text-muted h-2 w-15 rounded opacity-40" />
      </div>
      {[1, 2, 3].map((i) => (
        <div key={i} className="border-border-subtle bg-surface mb-2 rounded-lg border p-2.5">
          <div className="flex justify-between">
            <div className="bg-text-muted h-1.5 w-15 rounded opacity-40" />
            <div
              className="h-3.5 w-10 rounded"
              style={{
                backgroundColor: `var(--color-${["accent-green", "accent-blue", "accent-yellow"][i - 1]})15`,
              }}
            />
          </div>
          <div className="bg-border-subtle mt-2 h-1.5 w-30 rounded-sm" />
          <div className="bg-border-subtle mt-1.5 h-1.5 w-20 rounded-sm" />
        </div>
      ))}
    </div>
  );
}

function PhoneMockup(): React.ReactElement {
  return (
    <div className="bg-base relative h-full p-2.5">
      <div className="mb-3 flex justify-between">
        <div className="bg-text-muted h-1 w-7.5 rounded-sm opacity-40" />
        <div className="bg-text-muted h-1 w-5 rounded-sm opacity-40" />
      </div>
      <div className="border-border-subtle bg-surface mb-2 rounded-xl border p-2.5">
        <div className="bg-text-primary mb-1.5 h-1.5 w-17.5 rounded opacity-30" />
        <div className="border-primary/15 bg-primary/5 h-12.5 rounded-md border" />
      </div>
      <div className="border-border-subtle bg-surface mb-2 rounded-xl border p-2.5">
        <div className="bg-text-muted mb-2 h-1.5 w-12.5 rounded-sm opacity-40" />
        <div
          className="h-2 rounded"
          style={{
            background: `linear-gradient(90deg, var(--color-accent-green) 65%, var(--color-border-subtle) 65%)`,
          }}
        />
      </div>
      <div className="absolute inset-x-2.5 bottom-2 flex justify-around py-1.5">
        {[1, 2, 3, 4].map((i) => (
          <div
            key={i}
            className="border-border-subtle size-5 rounded-md border"
            style={{
              backgroundColor: i === 1 ? "var(--color-primary-dim)" : "var(--color-elevated)",
            }}
          />
        ))}
      </div>
    </div>
  );
}
