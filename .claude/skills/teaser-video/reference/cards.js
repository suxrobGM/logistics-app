// Animated title cards, filmed in the browser.
//
// Injected into a blank document on the app's own origin so its web fonts and
// CSS variables resolve - cards then share the product's typography instead of
// looking bolted on.
//
//   node cards.js
import { assemble, BASE, launchBrowser, startScreencast } from "./rig.js";

// Pull from the app's own palette.
const C = {
  bg: "#0B0B0A",
  card: "#151413",
  text: "#F4F2EE",
  dim: "#A7A49D",
  accent: "#FF6A3D",
  accent2: "#3B82F6",
};

function shell(inner, extraCss = "") {
  return `
  <style>
    * { margin: 0; box-sizing: border-box; }
    html, body { height: 100%; }
    body {
      background: ${C.bg}; color: ${C.text};
      display: grid; place-items: center; overflow: hidden;
      font-family: var(--film-font, system-ui, sans-serif);
      -webkit-font-smoothing: antialiased;
    }
    .stage { text-align: center; position: relative; }
    .eyebrow {
      font-family: var(--film-mono, monospace); font-size: 15px; letter-spacing: .35em;
      text-transform: uppercase; color: ${C.accent}; margin-bottom: 28px;
      opacity: 0; animation: rise 640ms cubic-bezier(.2,.7,.2,1) 200ms forwards;
    }
    .line {
      font-size: 84px; font-weight: 700; line-height: 1.08; letter-spacing: -.02em;
      opacity: 0; transform: translateY(26px);
      animation: rise 720ms cubic-bezier(.2,.7,.2,1) forwards;
    }
    .line.l2 { animation-delay: 340ms; }
    .sub {
      font-size: 30px; color: ${C.dim}; margin-top: 30px;
      opacity: 0; animation: rise 640ms cubic-bezier(.2,.7,.2,1) 700ms forwards;
    }
    @keyframes rise { to { opacity: 1; transform: translateY(0); } }
    ${extraCss}
  </style>
  <div class="stage">${inner}</div>`;
}

// A card with no looping animation stops repainting once it settles, so its
// clip ends early - build.js pads the tail with tpad. Cards that loop (the orb,
// the crawling bar) keep emitting frames for their whole duration.
const CARDS = {
  "c1-hook": shell(
    `<div class="line">The problem, stated</div>
     <div class="line l2">in the user's words.</div>
     <div class="bar"></div>`,
    `.line { color: ${C.dim}; } .line.l2 { color: ${C.text}; }
     .bar { height: 3px; width: 520px; margin: 44px auto 0; background: #21201C;
       border-radius: 2px; overflow: hidden; position: relative;
       opacity: 0; animation: rise 500ms 900ms forwards; }
     .bar::after { content: ""; position: absolute; inset: 0; width: 34%;
       background: #3a3835; border-radius: 2px;
       animation: crawl 2.2s cubic-bezier(.4,0,.6,1) 1.1s infinite; }
     @keyframes crawl { 0% { left: -35%; } 100% { left: 100%; } }`,
  ),

  "c2-meet": shell(
    `<div class="eyebrow">Category &nbsp;·&nbsp; Differentiator</div>
     <div class="line">Meet <span class="brand">Product</span>.</div>
     <div class="sub">One sentence on what it does, by itself.</div>`,
    `.brand { background: linear-gradient(100deg, ${C.accent}, ${C.accent2} 70%);
       -webkit-background-clip: text; background-clip: text; color: transparent; }
     .line { position: relative; display: inline-block; }
     .line::after { content: ""; position: absolute; left: 0; right: 100%; bottom: -18px;
       height: 4px; border-radius: 2px;
       background: linear-gradient(90deg, ${C.accent}, ${C.accent2});
       animation: sweep 900ms cubic-bezier(.2,.7,.2,1) 800ms forwards; }
     @keyframes sweep { to { right: 0; } }`,
  ),

  "c3-close": shell(
    `<div class="orb"></div>
     <div class="line wordmark">Product</div>
     <div class="sub url">example.com &nbsp;·&nbsp; free &amp; open source</div>`,
    `.orb { width: 240px; height: 240px; border-radius: 50%; margin: 0 auto 42px; position: relative;
       background: conic-gradient(from 0deg, ${C.accent}, ${C.accent2}, ${C.accent});
       -webkit-mask: radial-gradient(circle, transparent 52%, black 56%);
       mask: radial-gradient(circle, transparent 52%, black 56%);
       animation: spin 7s linear infinite, breathe 2.6s ease-in-out infinite; }
     .orb::after { content: ""; position: absolute; inset: -60px; border-radius: 50%;
       background: radial-gradient(circle, ${C.accent}47, transparent 65%); }
     @keyframes spin { to { transform: rotate(360deg); } }
     @keyframes breathe { 0%,100% { scale: 1; } 50% { scale: 1.045; } }
     .wordmark { font-size: 96px; }
     .url { font-family: var(--film-mono, monospace); font-size: 24px; letter-spacing: .06em; }`,
  ),
};

async function main() {
  const { browser, page } = await launchBrowser({ auth: false });

  // Load the site once so its font files are cached and its display face is readable.
  await page.goto(BASE, { waitUntil: "networkidle", timeout: 45000 }).catch(() => {});
  const fonts = await page.evaluate(() => ({
    display: getComputedStyle(document.querySelector("h1") || document.body).fontFamily,
    // Sniffing a mono face off the DOM is unreliable (it finds prose elements
    // and returns a serif) - pin a known-good stack instead.
    mono: '"JetBrains Mono", "Cascadia Mono", Consolas, "Courier New", monospace',
  }));
  console.log("display font:", fonts.display);

  for (const [name, html] of Object.entries(CARDS)) {
    await page.evaluate(
      ({ html, fonts }) => {
        document.open();
        document.write(`<!doctype html><html><body>${html}</body></html>`);
        document.close();
        document.documentElement.style.setProperty("--film-font", fonts.display);
        document.documentElement.style.setProperty("--film-mono", fonts.mono);
      },
      { html, fonts },
    );

    await page.waitForTimeout(300);
    const stop = await startScreencast(page, name);
    await page.waitForTimeout(5200);
    const cast = await stop();
    console.log(name, "->", await assemble(cast, name), `(${cast.fps.toFixed(0)}fps)`);
  }

  await browser.close();
}

main().catch((err) => {
  console.error("cards failed:", err.message);
  process.exit(1);
});
