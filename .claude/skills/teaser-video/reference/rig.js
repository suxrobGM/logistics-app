// Filming rig: Playwright drives the page, Chrome DevTools Protocol films it.
//
// Screencast beats screen recording here - it captures the page off-screen at
// ~100fps, so nothing else on the user's desktop lands in frame and the machine
// stays usable while filming.
//
// Config via env: TEASER_BASE_URL, TEASER_STATE, TEASER_W, TEASER_H.
import { execFile, spawn } from "node:child_process";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import ffmpegPath from "ffmpeg-static";
import { chromium } from "playwright";

const HERE = path.dirname(fileURLToPath(import.meta.url));

export const BASE = process.env.TEASER_BASE_URL || "http://localhost:3000";
export const STATE = process.env.TEASER_STATE || path.join(HERE, "storageState.json");
export const TAKES = path.join(HERE, "takes");
fs.mkdirSync(TAKES, { recursive: true });

// Film wider than the delivery frame when the app has a persistent sidebar you
// intend to crop off: VIEW.width = 1920 + railWidth.
export const VIEW = {
  width: Number(process.env.TEASER_W || 1920),
  height: Number(process.env.TEASER_H || 1080),
};

const CHROME_ARGS = [
  // A deployed origin may not reach a service on localhost without this.
  "--disable-features=LocalNetworkAccessChecks,PrivateNetworkAccessChecks,BlockInsecurePrivateNetworkRequests",
];

/**
 * @param {object} [opts]
 * @param {boolean} [opts.headless] Keep true: screencast works headless and stays off-screen.
 * @param {boolean} [opts.auth] Load the saved storageState.
 * @param {Record<string,string>} [opts.localStorage] Seeded before any page script runs.
 */
export async function launchBrowser(opts = {}) {
  const { headless = true, auth = true, localStorage: seed } = opts;
  const browser = await chromium.launch({ channel: "chrome", headless, args: CHROME_ARGS });
  const context = await browser.newContext({
    storageState: auth && fs.existsSync(STATE) ? STATE : undefined,
    viewport: VIEW,
    deviceScaleFactor: 1,
  });
  if (seed) {
    await context.addInitScript((entries) => {
      for (const [k, v] of Object.entries(entries)) localStorage.setItem(k, v);
    }, seed);
  }
  const page = await context.newPage();
  return { browser, context, page };
}

/** Navigate and let the page settle. Never film a spinner. */
export async function settle(page, url, ms = 4000) {
  await page
    .goto(url.startsWith("http") ? url : BASE + url, { waitUntil: "networkidle", timeout: 45000 })
    .catch(() => {
      // networkidle never fires on pages that poll; the dwell below covers it.
    });
  await page.waitForTimeout(ms);
}

/** Start filming. Returns stop() -> { dir, frames, fps, span }. */
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

  await client.send("Page.startScreencast", {
    format: "jpeg",
    quality: 92,
    maxWidth: VIEW.width,
    maxHeight: VIEW.height,
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
 * Assemble timestamped frames into constant-rate video. Frames arrive whenever
 * the page repaints, so honour each frame's real duration - assuming a fixed
 * interval produces judder.
 */
export async function assemble(cast, name, fps = 60) {
  const listFile = path.join(cast.dir, "list.txt");
  const lines = [];
  for (let i = 0; i < cast.frames.length; i++) {
    const dur = i < cast.frames.length - 1 ? cast.frames[i + 1].ts - cast.frames[i].ts : 1 / 30;
    lines.push(`file '${cast.frames[i].file.replaceAll("\\", "/")}'`);
    lines.push(`duration ${Math.max(dur, 0.001).toFixed(5)}`);
  }
  lines.push(`file '${cast.frames.at(-1).file.replaceAll("\\", "/")}'`);
  fs.writeFileSync(listFile, lines.join("\n"));

  const out = path.join(TAKES, `${name}.mp4`);
  await ffmpeg(
    [
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
      String(fps),
      "-c:v",
      "libx264",
      "-preset",
      "medium",
      "-crf",
      "16",
      "-pix_fmt",
      "yuv420p",
      out,
    ],
    `assemble ${name}`,
  );
  return out;
}

/** The camera move: an eased scroll inside the page. Never a video filter. */
export async function cameraScroll(page, toY, ms) {
  await page.evaluate(
    ({ toY, ms }) =>
      new Promise((done) => {
        const fromY = window.scrollY;
        const start = performance.now();
        const ease = (t) => (t < 0.5 ? 4 * t * t * t : 1 - (-2 * t + 2) ** 3 / 2);
        const step = (now) => {
          const t = Math.min(1, (now - start) / ms);
          window.scrollTo(0, fromY + (toY - fromY) * ease(t));
          if (t < 1) requestAnimationFrame(step);
          else done();
        };
        requestAnimationFrame(step);
      }),
    { toY, ms },
  );
}

/** Inject a cursor that moves with intent. The OS cursor is not in frame. */
export async function attachCursor(page) {
  await page.evaluate(() => {
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
      opacity: "0",
      transition: "opacity 300ms",
    });
    document.body.appendChild(c);
    window.__cursorPos = { x: 0, y: 0 };
  });
}

export async function cursorGlide(page, x, y, ms) {
  await page.evaluate(
    ({ x, y, ms }) =>
      new Promise((done) => {
        const c = document.getElementById("__filmCursor");
        if (!c) {
          done();
          return;
        }
        c.style.opacity = "1";
        const from = { ...window.__cursorPos };
        const start = performance.now();
        const ease = (t) => 1 - (1 - t) ** 3;
        const step = (now) => {
          const t = Math.min(1, (now - start) / ms);
          const e = ease(t);
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

/** Glide to a selector or point, dip on press, then really click it. */
export async function cursorClick(page, target, { glideMs = 900 } = {}) {
  let point = target;
  if (typeof target === "string") {
    const box = await page.locator(target).first().boundingBox();
    if (!box) throw new Error(`no bounding box for ${target}`);
    point = { x: box.x + box.width / 2, y: box.y + box.height / 2 };
  }
  await cursorGlide(page, point.x - 4, point.y - 2, glideMs);
  await page.evaluate(() => {
    const c = document.getElementById("__filmCursor");
    if (!c) return;
    c.style.scale = "0.85";
    setTimeout(() => {
      c.style.scale = "1";
    }, 110);
  });
  await page.waitForTimeout(60);
  await page.mouse.click(point.x, point.y);
}

/** Hide elements whose text matches, before rolling. */
export async function hideMatching(page, pattern) {
  await page.evaluate(
    (src) => {
      const re = new RegExp(src, "i");
      for (const el of document.querySelectorAll("*")) {
        if (el.childElementCount > 3) continue;
        if (re.test(el.textContent || "")) {
          const box = el.closest("[class*=Card], [class*=Paper], [class*=Stack], li, tr") || el;
          box.style.visibility = "hidden";
        }
      }
    },
    pattern.source ?? String(pattern),
  );
}

/**
 * OS-level capture, for an external window you cannot reach with CDP.
 * WARNING: records the entire screen, including whatever else is open. Warn the
 * user, then review the result frame by frame and delete it if it caught
 * anything private.
 */
export function recordDesktop(name, { fps = 30 } = {}) {
  const out = path.join(TAKES, `${name}.mkv`);
  const input =
    process.platform === "win32"
      ? ["-f", "gdigrab", "-i", "desktop"]
      : process.platform === "darwin"
        ? ["-f", "avfoundation", "-i", "1:none"]
        : ["-f", "x11grab", "-i", process.env.DISPLAY || ":0.0"];

  const proc = spawn(
    ffmpegPath,
    [
      "-y",
      ...input.slice(0, 2),
      "-draw_mouse",
      "0",
      "-framerate",
      String(fps),
      ...input.slice(2),
      "-c:v",
      "libx264",
      "-preset",
      "veryfast",
      "-crf",
      "19",
      "-pix_fmt",
      "yuv420p",
      out,
    ],
    { stdio: ["pipe", "ignore", "pipe"] },
  );

  return async function stop() {
    try {
      proc.stdin.write("q");
    } catch {
      // already exited
    }
    await new Promise((resolve) => {
      proc.on("exit", resolve);
      setTimeout(() => {
        proc.kill();
        resolve();
      }, 8000);
    });
    if (!fs.existsSync(out)) return null;
    // A killed recording leaves no container index; remux to make it seekable.
    const fixed = path.join(TAKES, `${name}-fixed.mkv`);
    await ffmpeg(["-y", "-i", out, "-c", "copy", fixed], "remux").catch(() => {});
    return fs.existsSync(fixed) ? fixed : out;
  };
}

export function ffmpeg(args, label = "ffmpeg") {
  return new Promise((resolve, reject) => {
    execFile(ffmpegPath, args, { maxBuffer: 1 << 26 }, (err, _stdout, stderr) => {
      if (err) reject(new Error(`${label} failed:\n${(stderr || "").slice(-1200)}`));
      else resolve();
    });
  });
}

/** ffprobe is not in ffmpeg-static - read what we need out of ffmpeg's stderr. */
export function probe(file) {
  return new Promise((resolve) => {
    execFile(ffmpegPath, ["-hide_banner", "-i", file], (_err, _stdout, stderr) => {
      const out = stderr || "";
      const d = out.match(/Duration: (\d+):(\d+):([\d.]+)/);
      const s = out.match(/, (\d+x\d+)/);
      resolve({
        seconds: d ? +d[1] * 3600 + +d[2] * 60 + +d[3] : null,
        size: s ? s[1] : null,
      });
    });
  });
}
