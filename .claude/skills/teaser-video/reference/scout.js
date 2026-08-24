// Location scout: authenticated screenshots of every candidate screen, so you
// storyboard from what the app actually looks like with real data - and spot
// which screens carry PII before a camera rolls.
//
//   node scout.js
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { launchBrowser, settle } from "./rig.js";

const HERE = path.dirname(fileURLToPath(import.meta.url));
const OUT = path.join(HERE, "scout");

// Edit for the app under test.
const SHOTS = [
  { name: "home", url: "/" },
  { name: "dashboard", url: "/dashboard" },
];

async function main() {
  fs.mkdirSync(OUT, { recursive: true });
  const { browser, page } = await launchBrowser();

  for (const shot of SHOTS) {
    await settle(page, shot.url, 3000);
    await page.screenshot({ path: path.join(OUT, `${shot.name}.png`) });
    console.log(shot.name.padEnd(20), "->", page.url());
  }

  await browser.close();
  console.log(`\n${SHOTS.length} screenshots in ${OUT}`);
  console.log("Look at all of them before writing the storyboard.");
}

main().catch((err) => {
  console.error("scout failed:", err.message);
  process.exit(1);
});
