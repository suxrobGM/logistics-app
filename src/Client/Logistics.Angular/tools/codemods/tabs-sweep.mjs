/**
 * tabs-sweep.mjs — S9. `<p-tabs>` -> `<ui-tabs>` across 7 files (31 tabs / 31 panels).
 *
 *   node tools/codemods/tabs-sweep.mjs --census | --apply | --check
 *
 *   <p-tabs [value]="activeTab()" (valueChange)="onTabChange($event)">   -> <ui-tabs …>   (unchanged bindings)
 *     <p-tablist>            -> <ui-tab-list>
 *       <p-tab [value]="0">  -> <ui-tab value="0">        *** binding -> static attribute ***
 *     <p-tabpanels>          -> <ui-tab-panels>
 *       <p-tabpanel [value]="0"> -> <ui-tab-panel value="0">
 *
 * =============================================================================================
 * WHY `[value]="0"` MUST BECOME `value="0"` ON THE TAB, BUT NOT ON `<ui-tabs>`
 * =============================================================================================
 * `ui-tab` / `ui-tab-panel` hand their key straight to brain (`BrnTabsTrigger.triggerFor`), which is a
 * REQUIRED `string` and which brain compares with `===`. A `hostDirectives` input cannot be run
 * through a transform, so the key has to arrive already a string — hence a static attribute. Leave it
 * as `[value]="0"` and every trigger's key is the NUMBER 0, `0 === "0"` is false, no tab is ever
 * selected, and the panel area renders blank. Green build, green tests, blank page.
 *
 * `<ui-tabs [value]>` is the opposite case and keeps its binding: it is a normal input, it accepts
 * `string | number`, and it stringifies internally — which is what lets the five `signal(0)` call
 * sites stay exactly as they are.
 *
 * Every value in the corpus is a literal (`[value]="0".."4"` or `value="claude-desktop"`); a DYNAMIC
 * tab key would need a different design, so this refuses on one rather than silently stringifying it.
 */
import { readText, writeText, listFiles, relative, runPrettier, parseMode } from "./lib/io.mjs";
import { addImport, addToComponentImports, removeImportSpecifier } from "./lib/ts.mjs";

const mode = parseMode();
const refusals = [];
const stats = { html: 0, ts: 0, tabs: 0, panels: 0 };

/** Longest first: `p-tabpanels` must not be eaten by the `p-tabpanel` rule, nor that by `p-tab`. */
const TAGS = [
  ["p-tabpanels", "ui-tab-panels"],
  ["p-tabpanel", "ui-tab-panel"],
  ["p-tablist", "ui-tab-list"],
  ["p-tabs", "ui-tabs"],
  ["p-tab", "ui-tab"],
];

function sweepTemplate(file) {
  let src = readText(file);
  if (!/<p-tabs\b/.test(src)) return false;
  const before = src;

  // 1. Numeric key bindings on the TAB and PANEL only -> static attributes. Done before the rename,
  //    while the tags still say `p-tab` / `p-tabpanel`, so `p-tabs`'s own [value] is never in scope.
  src = src.replace(
    /(<p-tabpanel|<p-tab)(\s+)\[value\]="(\d+)"/g,
    (_m, tag, ws, n) => `${tag}${ws}value="${n}"`,
  );

  // Any surviving [value] on a tab/panel is a dynamic key we are not prepared to stringify.
  for (const m of src.matchAll(/(<p-tabpanel|<p-tab)\s+\[value\]="([^"]*)"/g)) {
    refusals.push({
      file: relative(file),
      reason: `dynamic tab key [value]="${m[2]}" — ui-tab needs a static string key`,
    });
  }

  // 2. Tags.
  for (const [from, to] of TAGS) {
    src = src.replaceAll(`<${from}`, `<${to}`).replaceAll(`</${from}>`, `</${to}>`);
  }

  stats.tabs += (before.match(/<p-tab\s|<p-tab>/g) ?? []).length;
  stats.panels += (before.match(/<p-tabpanel\s|<p-tabpanel>/g) ?? []).length;
  if (src === before) return false;
  stats.html++;
  if (mode === "apply") writeText(file, src);
  return true;
}

function sweepTs(file) {
  let src = readText(file);
  if (!/from\s*["']primeng\/tabs["']/.test(src)) return false;
  const before = src;

  const r = removeImportSpecifier(src, "primeng/tabs", "TabsModule");
  if (r.changed) src = r.src;

  // Drop `TabsModule` from imports: [...] and add the five ui elements as one spread-free list.
  src = src.replace(/(\bimports:\s*\[)([\s\S]*?)(\])/, (whole, open, body, close) => {
    const names = body
      .split(",")
      .map((s) => s.trim())
      .filter(Boolean)
      .filter((n) => n !== "TabsModule");
    if (names.includes("UiTabsImports")) return whole;
    return `${open}${[...names, "UiTabsImports"].sort((a, b) => a.localeCompare(b)).join(", ")}${close}`;
  });

  const imp = addImport(src, "UiTabsImports", "@logistics/shared/ui");
  if (imp.changed) src = imp.src;

  if (src === before) return false;
  stats.ts++;
  if (mode === "apply") writeText(file, src);
  return true;
}

const written = [];
for (const f of listFiles({ dirs: ["projects"], ext: [".html"] })) if (sweepTemplate(f)) written.push(f);
for (const f of listFiles({ dirs: ["projects"], ext: [".ts"] })) if (sweepTs(f)) written.push(f);
if (mode === "apply" && written.length) runPrettier(written);

console.log(`\ntabs-sweep (${mode})`);
console.log(`  ${stats.tabs} tab(s) / ${stats.panels} panel(s) across ${stats.html} template(s); ${stats.ts} TS file(s).`);
console.log(`REFUSALS (${refusals.length})`);
for (const r of refusals) console.log(`  ${r.file}  ${r.reason}`);
if (!refusals.length) console.log("  none.");

if (mode === "check") {
  const bad = [];
  for (const f of listFiles({ dirs: ["projects"], ext: [".html"] })) {
    if (/<p-tabs\b|<p-tablist\b|<p-tabpanel/.test(readText(f).replace(/<!--[\s\S]*?-->/g, ""))) bad.push(relative(f));
  }
  for (const f of listFiles({ dirs: ["projects"], ext: [".ts"] })) {
    if (/from\s*["']primeng\/tabs["']/.test(readText(f))) bad.push(relative(f));
  }
  if (bad.length) {
    console.error(`\n--check FAILED: ${bad.join(", ")}`);
    process.exit(1);
  }
  console.log("\n--check OK: no p-tabs tags, no primeng/tabs import.");
}
if (refusals.length && mode === "apply") process.exit(1);
