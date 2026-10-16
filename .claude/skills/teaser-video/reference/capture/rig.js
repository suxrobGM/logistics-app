// Filming rig: Playwright drives the page, the Chrome DevTools Protocol films it.
//
// Screencast beats screen recording: it captures the page off-screen, so nothing else on the
// desktop lands in frame. Frames come in CSS pixels whatever the deviceScaleFactor, so a
// 1600x900 viewport films at 1600x900 - use 2x stills (stills.js) for close-ups.
import { execFile } from "node:child_process";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import ffmpegPath from "ffmpeg-static";

const HERE = path.dirname(fileURLToPath(import.meta.url));
export const TAKES = path.join(HERE, "takes");
fs.mkdirSync(TAKES, { recursive: true });

/** Start filming. Returns stop() -> { dir, frames: [{file, ts}], fps, span }. */
export async function startScreencast(page, name) {
  const dir = path.join(TAKES, name);
  fs.rmSync(dir, { recursive: true, force: true });
  fs.mkdirSync(dir, { recursive: true });

  const client = await page.context().newCDPSession(page);
  const frames = [];
  const writes = [];
  let i = 0;

  client.on("Page.screencastFrame", (ev) => {
    const file = path.join(dir, `f${String(i++).padStart(5, "0")}.jpg`);
    writes.push(fs.promises.writeFile(file, Buffer.from(ev.data, "base64")));
    frames.push({ file, ts: ev.metadata.timestamp });
    client.send("Page.screencastFrameAck", { sessionId: ev.sessionId }).catch(() => {});
  });

  const { width, height } = page.viewportSize();
  await client.send("Page.startScreencast", {
    format: "jpeg",
    quality: 92,
    maxWidth: width,
    maxHeight: height,
    everyNthFrame: 1,
  });

  return async function stop() {
    await client.send("Page.stopScreencast").catch(() => {});
    await Promise.all(writes);
    await client.detach().catch(() => {});
    if (frames.length < 2) throw new Error(`${name}: screencast produced ${frames.length} frames`);
    const span = frames.at(-1).ts - frames[0].ts;
    return { dir, frames, span, fps: (frames.length - 1) / span };
  };
}

/**
 * Frames arrive whenever the page repaints, so honour each frame's real duration and resample to
 * constant 60fps - assuming a fixed interval produces judder.
 */
export async function assemble(cast, name) {
  const listFile = path.join(cast.dir, "list.txt");
  const lines = [];
  for (let i = 0; i < cast.frames.length; i++) {
    const dur = i < cast.frames.length - 1 ? cast.frames[i + 1].ts - cast.frames[i].ts : 1 / 30;
    lines.push(
      `file '${cast.frames[i].file.replaceAll("\\", "/")}'`,
      `duration ${Math.max(dur, 0.001).toFixed(5)}`,
    );
  }
  lines.push(`file '${cast.frames.at(-1).file.replaceAll("\\", "/")}'`);
  fs.writeFileSync(listFile, lines.join("\n"));

  const out = path.join(TAKES, `${name}.mp4`);
  await ffmpeg([
    "-y",
    "-f",
    "concat",
    "-safe",
    "0",
    "-i",
    listFile,
    "-fps_mode",
    "cfr",
    "-r",
    "60",
    "-c:v",
    "libx264",
    "-preset",
    "slow",
    "-crf",
    "12",
    "-pix_fmt",
    "yuv420p",
    out,
  ]);
  return out;
}

/** An injected cursor that moves with intent; the OS cursor is never in frame. */
export async function attachCursor(page, start = { x: 1200, y: 620 }) {
  await page.evaluate((start) => {
    if (document.getElementById("__filmCursor")) return;
    const c = document.createElement("div");
    c.id = "__filmCursor";
    c.innerHTML =
      '<svg width="26" height="26" viewBox="0 0 24 24"><path d="M4 2 L20 12 L12.5 13.8 L9 21 Z" fill="#fff" stroke="#000" stroke-width="1.4"/></svg>';
    Object.assign(c.style, {
      position: "fixed",
      left: "0",
      top: "0",
      zIndex: "2147483647",
      pointerEvents: "none",
    });
    c.style.transform = `translate(${start.x}px, ${start.y}px)`;
    document.body.appendChild(c);
    window.__cursorPos = { ...start };
  }, start);
}

export async function cursorGlide(page, x, y, ms) {
  await page.evaluate(
    ({ x, y, ms }) =>
      new Promise((done) => {
        const c = document.getElementById("__filmCursor");
        const from = { ...window.__cursorPos };
        const start = performance.now();
        const step = (now) => {
          const t = Math.min(1, (now - start) / ms);
          const e = 1 - (1 - t) ** 3;
          const cx = from.x + (x - from.x) * e;
          const cy = from.y + (y - from.y) * e;
          c.style.transform = `translate(${cx}px, ${cy}px)`;
          window.__cursorPos = { x: cx, y: cy };
          if (t < 1) requestAnimationFrame(step);
          else done();
        };
        requestAnimationFrame(step);
      }),
    { x, y, ms },
  );
}

/** Glide to the centre of a locator, dip on press, then really click it. */
export async function cursorClick(page, locator, { glideMs = 900 } = {}) {
  const box = await locator.first().boundingBox();
  if (!box) throw new Error("cursorClick: target has no bounding box");
  const x = box.x + box.width / 2;
  const y = box.y + box.height / 2;
  await cursorGlide(page, x - 4, y - 2, glideMs);
  await page.evaluate(() => {
    const c = document.getElementById("__filmCursor");
    c.style.scale = "0.85";
    setTimeout(() => (c.style.scale = "1"), 110);
  });
  await page.waitForTimeout(60);
  await page.mouse.click(x, y);
}

export function ffmpeg(args) {
  return new Promise((resolve, reject) => {
    execFile(ffmpegPath, args, { maxBuffer: 1 << 26 }, (err, _stdout, stderr) => {
      if (err) reject(new Error(`ffmpeg failed:\n${(stderr || "").slice(-1200)}`));
      else resolve();
    });
  });
}
