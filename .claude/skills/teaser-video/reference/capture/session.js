import path from "node:path";
import { chromium } from "playwright";
export const BASE = "https://tms.logisticsx.app";
const PROFILE = path.resolve("profile");

/**
 * Signed-in page. The SPA keeps tokens in sessionStorage, but the identity server's cookie lives
 * in a persistent profile, so after the first login "Sign in" bounces straight back without the form
 * (the login page is rate-limited to 10 attempts per 15 minutes).
 */
export async function openSession({
  width = 1600,
  height = 900,
  dpr = 1,
  user = "owner@test.com",
  base = BASE,
  rootIsHome = false,
} = {}) {
  const context = await chromium.launchPersistentContext(
    path.join(PROFILE, user.replace(/\W/g, "_")),
    {
      channel: "chrome",
      headless: true,
      viewport: { width, height },
      deviceScaleFactor: dpr,
    },
  );
  const page = context.pages()[0] ?? (await context.newPage());
  await page.goto(base, { waitUntil: "networkidle" }).catch(() => {});
  const signIn = page.getByRole("button", { name: /sign in/i });
  if (await signIn.count()) await signIn.first().click();
  const email = page.locator("input[type=email], input[name*=Email i]").first();
  const landed = (u) =>
    u.origin === base &&
    !/login|callback|signin/i.test(u.pathname) &&
    !u.search.includes("code=") &&
    (rootIsHome || u.pathname !== "/");
  await Promise.race([
    email.waitFor({ timeout: 30000 }),
    page.waitForURL(landed, { timeout: 30000 }),
  ]);
  if (rootIsHome) await page.waitForTimeout(3000);
  if (await email.isVisible().catch(() => false)) {
    await email.fill(user);
    await page.locator("input[type=password]").first().fill("Test12345#");
    await page.locator("button[type=submit]").first().click();
  }
  await page.waitForURL(landed, { timeout: 60000 });
  await page.waitForLoadState("networkidle").catch(() => {});
  await page.waitForTimeout(1500);
  return { browser: context, context, page };
}

/** In-app navigation keeps the SPA's in-memory auth. */
export async function go(page, path, ms = 3500) {
  await page.evaluate((p) => {
    history.pushState({}, "", p);
    dispatchEvent(new PopStateEvent("popstate"));
  }, path);
  await page.waitForLoadState("networkidle").catch(() => {});
  await page.waitForTimeout(ms);
}
