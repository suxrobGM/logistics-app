// node preview.mjs 120 300 ... -> out/preview/f<N>.jpg, plus a contact sheet
import fs from "node:fs";
import path from "node:path";
import { bundle } from "@remotion/bundler";
import { renderStill, selectComposition } from "@remotion/renderer";

const frames = process.argv.slice(2).map(Number);
const serveUrl = await bundle({ entryPoint: path.resolve("src/index.ts") });
const composition = await selectComposition({ serveUrl, id: "Teaser" });
fs.mkdirSync("out/preview", { recursive: true });
for (const frame of frames) {
  await renderStill({
    serveUrl,
    composition,
    frame,
    output: `out/preview/f${frame}.jpg`,
    imageFormat: "jpeg",
    jpegQuality: 85,
    scale: 0.5,
  });
  console.log("f", frame);
}
