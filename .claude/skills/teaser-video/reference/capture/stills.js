// 2x stills (3200x1800 of a 1600x900 page) - most of the teaser is built from these.
//
//   node stills.js home=/home loads=/loads eld=/eld
//   TEASER_USER=customer1@test.com TEASER_BASE=https://customer.logisticsx.app TEASER_ROOT_IS_HOME=1 \
//     node stills.js customer=/shipments
//
// Writes to ../compose/public/stills/<name>.png. Run with MSYS_NO_PATHCONV=1 under Git Bash.
import fs from "node:fs";
import { BASE, go, openSession } from "./session.js";

const OUT = "../compose/public/stills";
fs.mkdirSync(OUT, { recursive: true });

const { browser, page } = await openSession({
  dpr: 2,
  user: process.env.TEASER_USER ?? "owner@test.com",
  base: process.env.TEASER_BASE ?? BASE,
  rootIsHome: Boolean(process.env.TEASER_ROOT_IS_HOME),
});

const reject = page.getByRole("button", { name: "Reject all" });
if (await reject.count()) await reject.first().click();

for (const arg of process.argv.slice(2)) {
  const [name, path] = arg.split("=");
  await go(page, path, 5000);
  await page.mouse.move(1599, 899);
  await page.screenshot({ path: `${OUT}/${name}.png` });
  console.log(name, page.url());
}
await browser.close();
