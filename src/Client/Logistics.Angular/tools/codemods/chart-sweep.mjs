/**
 * chart-sweep.mjs — <p-chart> -> <ui-chart> (27 sites / 10 files, all in tms-portal).
 *
 * `ui-chart` was built to `p-chart`'s binding surface on purpose, so the template half of this sweep
 * is a pure tag rename: `type` / `[type]`, `[data]`, `[options]`, `class`, `style` all carry over
 * byte-for-byte. The TS half swaps `ChartModule` (primeng/chart) for `UiChart` (@logistics/shared/ui)
 * in both the import statement and the component's `imports: [...]` array.
 *
 * WHAT IT REFUSES, and why each one matters:
 *   [plugins]        per-chart chart.js plugins. `ui-chart` registers a fixed, audited set and takes
 *                    no plugin array; forwarding one would need a real API.
 *   [responsive]     `ui-chart` has the input, but a call site that opts OUT is relying on the
 *                    p-chart -> chart.js option merge, which is exactly the subtle path worth a human.
 *   [width]/[height] p-chart used these to force `maintainAspectRatio: false`. No call site binds
 *                    them today (they size via `style="height: …"`), so `ui-chart` does not implement
 *                    that rule. If one appears, the layout contract changed — stop and look.
 *   (onDataSelect)   click-to-drill-down. No call site uses it; `ui-chart` does not emit it.
 *   [ariaLabelledBy] `ui-chart` exposes `ariaLabel` only.
 *
 * Census over the tree at the time of writing: 27 sites, 0 refusals.
 */

import { allAttrs, lineOf, parse, renameTag, Splicer, visitElements } from "./lib/html.mjs";
import { listFiles, parseMode, readText, relative, runPrettier, writeText } from "./lib/io.mjs";
import { addImport, addToComponentImports, removeImportSpecifier } from "./lib/ts.mjs";

const OLD_TAG = "p-chart";
const NEW_TAG = "ui-chart";
const OLD_SYMBOL = "ChartModule";
const NEW_SYMBOL = "UiChart";
const OLD_MODULE = "primeng/chart";
const NEW_MODULE = "@logistics/shared/ui";

/** Attributes `ui-chart` understands. Anything else is a refusal. */
const SUPPORTED = new Set(["type", "data", "options", "class", "style", "responsive", "ariaLabel"]);

const mode = parseMode();
const refusals = [];
const changedFiles = [];
/** Files this sweep would touch — counted in every mode, so --census reports what --apply will do. */
const touchedFiles = new Set();
let siteCount = 0;

// ---------------------------------------------------------------------------------------------
// Templates
// ---------------------------------------------------------------------------------------------

for (const file of listFiles({ ext: [".html"] })) {
  const source = readText(file);
  if (!source.includes(`<${OLD_TAG}`)) continue;

  const splicer = new Splicer(source, relative(file));
  let hits = 0;

  visitElements(parse(source, file), (el) => {
    if (el.name !== OLD_TAG) return;

    const unsupported = allAttrs(el).filter((a) => !SUPPORTED.has(a.name));
    if (unsupported.length > 0) {
      refusals.push(
        `${relative(file)}:${lineOf(source, el.startSourceSpan.start.offset)} — ` +
          `<${OLD_TAG}> uses ${unsupported.map((a) => a.name).join(", ")}, ` +
          `which ui-chart does not implement. Convert by hand.`,
      );
      return;
    }

    renameTag(splicer, el, NEW_TAG);
    hits++;
  });

  if (hits === 0) continue;
  siteCount += hits;
  touchedFiles.add(file);

  if (mode === "apply") {
    if (writeText(file, splicer.apply())) changedFiles.push(file);
  }
}

// ---------------------------------------------------------------------------------------------
// TypeScript — swap ChartModule for UiChart in the import statement and the imports array.
// ---------------------------------------------------------------------------------------------

/**
 * Replace one symbol in a standalone component's `imports: [...]`. `ts.mjs` can ADD to that array
 * but not remove from it, and leaving a dangling `ChartModule` there fails the build — so the
 * removal is done here, verified, and refuses on any shape it does not fully understand.
 */
function removeFromComponentImports(src, symbol) {
  const open = src.indexOf("imports:");
  if (open === -1) return { src, changed: false, reason: "no imports array" };

  const lb = src.indexOf("[", open);
  let depth = 0;
  let rb = -1;
  for (let i = lb; i < src.length; i++) {
    if (src[i] === "[") depth++;
    else if (src[i] === "]" && --depth === 0) {
      rb = i;
      break;
    }
  }
  if (lb === -1 || rb === -1) return { src, changed: false, reason: "unbalanced imports array" };

  const body = src.slice(lb + 1, rb);
  if (body.includes("...")) return { src, changed: false, reason: "imports array uses a spread" };

  const names = body
    .split(",")
    .map((s) => s.trim())
    .filter(Boolean);
  if (!names.includes(symbol)) return { src, changed: false, reason: `'${symbol}' not in imports` };

  const kept = names.filter((n) => n !== symbol);
  return { src: `${src.slice(0, lb + 1)}${kept.join(", ")}${src.slice(rb)}`, changed: true };
}

for (const file of listFiles({ ext: [".ts"] })) {
  let src = readText(file);
  if (!src.includes(OLD_MODULE)) continue;

  const dropped = removeFromComponentImports(src, OLD_SYMBOL);
  if (!dropped.changed) {
    refusals.push(`${relative(file)} — imports ${OLD_MODULE} but ${dropped.reason}.`);
    continue;
  }
  src = dropped.src;

  const unimported = removeImportSpecifier(src, OLD_MODULE, OLD_SYMBOL);
  if (!unimported.changed) {
    refusals.push(`${relative(file)} — ${unimported.reason}.`);
    continue;
  }
  src = unimported.src;

  // `addImport` is a no-op when UiChart is already imported, which keeps --apply idempotent.
  src = addImport(src, NEW_SYMBOL, NEW_MODULE).src;
  src = addToComponentImports(src, NEW_SYMBOL).src;
  touchedFiles.add(file);

  if (mode === "apply") {
    if (writeText(file, src)) changedFiles.push(file);
  }
}

// ---------------------------------------------------------------------------------------------
// Report
// ---------------------------------------------------------------------------------------------

if (mode === "apply" && changedFiles.length > 0) {
  runPrettier(changedFiles);
}

for (const refusal of refusals) {
  console.error(`REFUSED  ${refusal}`);
}

if (mode === "check") {
  const remaining = listFiles({ ext: [".html"] }).filter((f) =>
    readText(f).includes(`<${OLD_TAG}`),
  );
  const stragglers = listFiles({ ext: [".ts"] }).filter((f) => readText(f).includes(OLD_MODULE));

  for (const f of [...remaining, ...stragglers]) {
    console.error(`REMAINS  ${relative(f)}`);
  }
  if (remaining.length + stragglers.length > 0) {
    console.error(`\nchart-sweep --check: ${remaining.length + stragglers.length} file(s) remain.`);
    process.exit(1);
  }
  console.log("chart-sweep --check: clean — no <p-chart>, no primeng/chart import.");
  process.exit(0);
}

console.log(
  `chart-sweep --${mode}: ${siteCount} <${OLD_TAG}> site(s), ` +
    `${mode === "apply" ? changedFiles.length : touchedFiles.size} file(s) ` +
    `${mode === "apply" ? "changed" : "would change"}, ` +
    `${refusals.length} refusal(s).`,
);

if (mode === "apply" && refusals.length > 0) {
  process.exit(1);
}
