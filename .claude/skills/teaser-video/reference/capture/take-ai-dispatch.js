// One AI dispatch take: type a request, let the agent work, open the Approve dialog and decline it.
// The edit cuts on the Approve click, so nothing is written. Outputs:
//   takes/ai-run.mp4               constant 60fps master -> ../compose/public/footage/ai.mp4
//   takes/ai-run.json              event times in seconds from the first frame (drives SEGMENTS)
//   ../compose/src/scenes/ai-keys.json   keystroke times (drive the typing SFX)
//
// A long request makes the agent's LLM calls slow; keep it to one load (see SKILL.md).
import fs from "node:fs";
import { assemble, attachCursor, cursorClick, cursorGlide, startScreencast } from "./rig.js";
import { go, openSession } from "./session.js";

const PROMPT =
  process.env.PROMPT ??
  "Find a truck for the Phoenix to Los Angeles load. Check the driver's hours first.";

const { browser, page } = await openSession();
await go(page, "/ai-dispatch", 4000);
await page.getByRole("button", { name: "New chat" }).click();
await page.waitForTimeout(2500);
await page.mouse.move(1599, 899);
await attachCursor(page);

const events = [];
let t0 = 0;
const mark = (name, extra = {}) =>
  events.push({ name, t: performance.now() / 1000 - t0, ...extra });

const stop = await startScreencast(page, "ai-run");
t0 = performance.now() / 1000;
await page.waitForTimeout(1200);

await cursorClick(page, page.getByPlaceholder(/Ask the dispatcher agent/), { glideMs: 1000 });
mark("click-input");
await page.waitForTimeout(400);
for (const ch of PROMPT) {
  await page.keyboard.type(ch);
  mark("key");
  await page.waitForTimeout(28 + Math.random() * 40);
}
await page.waitForTimeout(500);
await cursorClick(page, page.getByRole("button", { name: "Send message" }), { glideMs: 700 });
mark("send");

await cursorGlide(page, 1500, 860, 900);
const deadline = Date.now() + 4 * 60 * 1000;
let seen = 0;
while (Date.now() < deadline) {
  const pending = await page.getByRole("button", { name: /^Approve/ }).count();
  if (pending !== seen) {
    mark("decisions", { count: pending });
    seen = pending;
  }
  if (await page.getByText(/Dispatch summary|allowed timeout/).count()) {
    mark("summary");
    break;
  }
  await page.waitForTimeout(500);
}
await page.waitForTimeout(2500);

const approve = page.getByRole("button", { name: /^Approve/ }).first();
if (await approve.count()) {
  await cursorClick(page, approve, { glideMs: 1100 });
  mark("approve-click");
  const dialog = page.getByRole("alertdialog").or(page.getByRole("dialog")).last();
  await dialog.waitFor({ timeout: 8000 });
  await page.waitForTimeout(600);
  await dialog
    .getByRole("button", { name: /^no|cancel/i })
    .first()
    .click();
}
await page.waitForTimeout(1000);

const cast = await stop();
// CDP timestamps are wall-clock; t0 is close enough to the first frame for cue placement.
await assemble(cast, "ai-run");
fs.writeFileSync(
  "takes/ai-run.json",
  JSON.stringify({ events, fps: cast.fps, span: cast.span }, null, 2),
);
fs.writeFileSync(
  "../compose/src/scenes/ai-keys.json",
  JSON.stringify(events.filter((e) => e.name === "key").map((e) => +e.t.toFixed(3))),
);
console.log(
  events
    .filter((e) => e.name !== "key")
    .map((e) => `${e.name}@${e.t.toFixed(2)}`)
    .join(" "),
);
await browser.close();
