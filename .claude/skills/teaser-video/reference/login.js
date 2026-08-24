// One-time headed login. The user types their credentials once; every later
// take replays the saved session. Run with TEASER_BASE_URL set.
//
//   node login.js [/login]
//
// Sessions expire (often ~1 day) - re-run when takes start bouncing to /login.
import path from "node:path";
import { fileURLToPath } from "node:url";
import { chromium } from "playwright";

const HERE = path.dirname(fileURLToPath(import.meta.url));
const BASE = process.env.TEASER_BASE_URL || "http://localhost:3000";
const STATE = process.env.TEASER_STATE || path.join(HERE, "storageState.json");

// Paths that mean "not signed in yet". Adjust per app.
const AUTH_PATHS = /\/(login|register|signin|sign-in|forgot-password|reset-password)/;

// UI preferences worth pinning so every take looks identical. Adjust per app.
const UI_PREFS = {};

async function main() {
  const loginPath = process.argv[2] || "/login";
  const browser = await chromium.launch({ channel: "chrome", headless: false });
  const context = await browser.newContext({ viewport: null });
  const page = await context.newPage();
  await page.goto(BASE + loginPath);

  console.log("Sign in in the browser window. Waiting up to 10 minutes...");
  await page.waitForURL(
    (url) => url.origin === new URL(BASE).origin && !AUTH_PATHS.test(url.pathname),
    { timeout: 10 * 60 * 1000 },
  );
  await page.waitForLoadState("networkidle").catch(() => {});

  if (Object.keys(UI_PREFS).length > 0) {
    await page.evaluate((prefs) => {
      for (const [k, v] of Object.entries(prefs)) localStorage.setItem(k, v);
    }, UI_PREFS);
  }

  await context.storageState({ path: STATE });
  console.log("saved:", STATE, "| landed on:", page.url());
  await browser.close();
}

main().catch((err) => {
  console.error("login failed:", err.message);
  process.exit(1);
});
