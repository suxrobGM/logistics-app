import { Audio } from "@remotion/media";
import { AbsoluteFill, Sequence, staticFile } from "remotion";
import { Lens } from "./components/Stage";
import { AI_SFX, AIDispatch } from "./scenes/AIDispatch";
import { BrandReveal } from "./scenes/BrandReveal";
import { Chaos } from "./scenes/Chaos";
import { Close } from "./scenes/Close";
import { ColdOpen } from "./scenes/ColdOpen";
import { Compliance } from "./scenes/Compliance";
import { DriverApp } from "./scenes/DriverApp";
import { Money } from "./scenes/Money";
import { Montage } from "./scenes/Montage";
import { NetworkMap } from "./scenes/NetworkMap";
import { SCENES as S } from "./timeline";

export interface Cue {
  at: number;
  sfx: string;
  volume: number;
}

const CUES: Cue[] = [
  ...[0, 20, 38, 54, 68, 80].map((a) => ({ at: S.chaos.from + a, sfx: "swipe", volume: 0.18 })),
  ...[4, 64, 124, 168].map((a) => ({ at: S.chaos.from + a, sfx: "glitch", volume: 0.35 })),
  { at: S.chaos.from + 196, sfx: "whoosh-low", volume: 0.6 },
  { at: S.reveal.from + 18, sfx: "shimmer", volume: 0.45 },
  { at: S.reveal.from + 60, sfx: "whoosh", volume: 0.25 },
  ...AI_SFX.map((c) => ({ ...c, at: S.ai.from + c.at })),
  { at: S.map.from - 8, sfx: "whoosh", volume: 0.5 },
  ...Array.from({ length: 12 }, (_, i) => ({
    at: S.map.from + 24 + i * 11,
    sfx: "tick",
    volume: 0.5,
  })),
  { at: S.driver.from - 10, sfx: "whoosh-low", volume: 0.5 },
  ...[0, 14, 28].map((a) => ({ at: S.driver.from + a + 6, sfx: "thud", volume: 0.25 })),
  ...[0, 8, 16].map((a) => ({ at: S.driver.from + 150 + a, sfx: "swipe", volume: 0.25 })),
  { at: S.compliance.from - 8, sfx: "whoosh", volume: 0.45 },
  { at: S.compliance.from + 62, sfx: "thud", volume: 0.5 },
  { at: S.compliance.from + 72, sfx: "glitch", volume: 0.22 },
  { at: S.money.from - 8, sfx: "whoosh", volume: 0.45 },
  ...Array.from({ length: 14 }, (_, i) => ({
    at: S.money.from + 8 + i * 4,
    sfx: "tick",
    volume: 0.45 - i * 0.02,
  })),
  { at: S.money.from + 26, sfx: "thud", volume: 0.3 },
  ...Array.from({ length: 6 }, (_, i) => ({
    at: S.montage.from + i * 30,
    sfx: "swipe",
    volume: 0.3,
  })),
  { at: S.montage.from + 180, sfx: "whoosh-low", volume: 0.55 },
  { at: S.close.from + 16, sfx: "shimmer", volume: 0.4 },
];

const SCENE_LIST = [
  { key: "coldOpen", C: ColdOpen },
  { key: "chaos", C: Chaos },
  { key: "reveal", C: BrandReveal },
  { key: "ai", C: AIDispatch },
  { key: "map", C: NetworkMap },
  { key: "driver", C: DriverApp },
  { key: "compliance", C: Compliance },
  { key: "money", C: Money },
  { key: "montage", C: Montage },
  { key: "close", C: Close },
] as const;

export function Teaser() {
  return (
    <AbsoluteFill style={{ backgroundColor: "#000" }}>
      {SCENE_LIST.map(({ key, C }) => (
        <Sequence key={key} name={key} from={S[key].from} durationInFrames={S[key].dur}>
          <C />
        </Sequence>
      ))}
      <Lens />
      <Audio src={staticFile("audio/music.wav")} volume={0.9} />
      {CUES.map((c, i) => (
        <Sequence key={i} from={Math.max(0, Math.round(c.at))} durationInFrames={180} layout="none">
          <Audio src={staticFile(`audio/${c.sfx}.wav`)} volume={c.volume} />
        </Sequence>
      ))}
    </AbsoluteFill>
  );
}
