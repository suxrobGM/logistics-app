import { loadFont as loadMono } from "@remotion/google-fonts/DMMono";
import { loadFont as loadSans } from "@remotion/google-fonts/DMSans";

export const SANS = loadSans("normal", {
  weights: ["400", "500", "600", "700"],
  subsets: ["latin"],
}).fontFamily;
export const MONO = loadMono("normal", { weights: ["400", "500"], subsets: ["latin"] }).fontFamily;

export const C = {
  ink: "#0A0C10",
  ink2: "#11141A",
  line: "rgba(255,255,255,0.08)",
  text: "#F4F5F7",
  muted: "#8B93A1",
  dim: "#4A515C",
  brand: "#0EA5E9",
  ok: "#22C55E",
  bad: "#EF4444",
  warn: "#F59E0B",
};
