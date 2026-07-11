/**
 * tooltip-sweep.mjs — S6 of the PrimeNG -> spartan migration: `pTooltip` -> `uiTooltip`.
 *
 *   node tools/codemods/tooltip-sweep.mjs --census   # report only; writes .tooltip-refusals.json
 *   node tools/codemods/tooltip-sweep.mjs --apply    # rewrite templates + TS imports, then prettier
 *   node tools/codemods/tooltip-sweep.mjs --check    # exit 1 if any pTooltip remains
 *
 * See lib/html.mjs for the codemod contract (parse to analyse, span-splice to mutate, refuse rather
 * than guess, idempotent).
 *
 * =============================================================================================
 * THIS IS AN ATTRIBUTE SWEEP, NOT AN ELEMENT SWEEP — WHICH CHANGES WHAT "UNKNOWN" MEANS.
 * =============================================================================================
 * button-sweep owned the whole `<p-button>` element, so it could refuse on ANY attribute it did not
 * recognise. Here the host is someone else's element — a `<ui-button>`, a `<span>`, a `<th>` — and
 * almost every attribute on it (`icon`, `severity`, `class`, `(click)`, `routerLink`) has nothing to
 * do with the tooltip. Refusing on those would refuse all 124 sites.
 *
 * So the rule is inverted: we only look at the TOOLTIP NAMESPACE (the inputs of PrimeNG's Tooltip
 * directive). Three of those are mapped; every OTHER one refuses the element. `disabled` is the trap
 * that proves the rule — on `<ui-button disabled pTooltip="…">` it is the BUTTON's disabled, not the
 * tooltip's, and treating it as a tooltip input would refuse a pile of perfectly convertible sites.
 * PrimeNG spells the tooltip's own version `[tooltipDisabled]`.
 *
 * =============================================================================================
 * WHAT IS NOT MAPPED, AND WHY IT IS LEFT FOR A HUMAN (3 sites)
 * =============================================================================================
 *   [tooltipOptions] (2) — an options BAG. `{ showDelay: 300 }` happens to be expressible, but the
 *     bag is an arbitrary expression and a codemod that reaches into one is guessing. Both sites are
 *     hand-converted to [uiTooltipDelay].
 *   [escape]="false" (1) — means "this tooltip is raw HTML", set via innerHTML. `uiTooltip` has no
 *     such input and never will; the one site is hand-converted to a TemplateRef (which also closes
 *     the injection it was carrying — it was interpolating tenant address data into an HTML string).
 */

import path from "node:path";
import { allAttrs, lineOf, parse, renameAttr, Splicer, visitElements } from "./lib/html.mjs";
import {
  listFiles,
  parseMode,
  readText,
  relative,
  runPrettier,
  WORKSPACE_ROOT,
  writeText,
} from "./lib/io.mjs";
import { addImport, addToComponentImports, removeImportSpecifier } from "./lib/ts.mjs";

const REFUSALS_OUT = path.join(WORKSPACE_ROOT, "tools/codemods/.tooltip-refusals.json");
const TOOLTIP_DIRECTIVE = "projects/shared/src/lib/ui/feedback/tooltip/tooltip";

// -----------------------------------------------------------------------------------------------
// THE MAPPING TABLE. Anything in the tooltip namespace and not in here is a REFUSAL.
// -----------------------------------------------------------------------------------------------

/** pTooltip -> uiTooltip. Static and bound forms both just get renamed; the value is untouched. */
const RENAME = new Map([
  ["pTooltip", "uiTooltip"],
  ["tooltipPosition", "uiTooltipPosition"],
  ["showDelay", "uiTooltipDelay"],
]);

/**
 * Every input of PrimeNG's Tooltip directive. An attribute in this set that is not in RENAME refuses
 * the element — it is tooltip behaviour we would otherwise DROP ON THE FLOOR, silently, and a dropped
 * tooltip option looks identical to a working one in review.
 *
 * `disabled` is deliberately NOT here: see the header.
 */
const TOOLTIP_NAMESPACE = new Set([
  "pTooltip",
  "tooltipPosition",
  "showDelay",
  "hideDelay",
  "tooltipEvent",
  "tooltipOptions",
  "escape",
  "tooltipDisabled",
  "tooltipStyleClass",
  "tooltipZIndex",
  "positionStyle",
  "positionTop",
  "positionLeft",
  "autoHide",
  "life",
  "showOnEllipsis",
  "hideOnEscape",
  "fitContent",
  "appendTo",
]);

const VALID_POSITIONS = new Set(["top", "bottom", "left", "right"]);

// -----------------------------------------------------------------------------------------------
// bookkeeping
// -----------------------------------------------------------------------------------------------

const refusals = [];
const refuse = (file, line, reason, snippet) =>
  refusals.push({
    file: relative(file),
    line,
    reason,
    snippet: (snippet ?? "").replace(/\s+/g, " ").trim().slice(0, 110),
  });

const stats = { seen: 0, rewritten: 0, refused: 0, position: 0, delay: 0, bound: 0, static: 0 };
const positionCount = new Map();
const hostTags = new Map();
const bump = (map, key) => map.set(key, (map.get(key) ?? 0) + 1);

// -----------------------------------------------------------------------------------------------
// one tooltip host
// -----------------------------------------------------------------------------------------------

/** Plan the edits for one element carrying `pTooltip`. Returns false (and queues NOTHING) to refuse. */
function convertTooltip(el, ctx, source, file, splicer) {
  const line = lineOf(source, el.sourceSpan.start.offset);
  const openTag = source.slice(el.sourceSpan.start.offset, el.startSourceSpan.end.offset);
  const no = (reason) => {
    refuse(file, line, reason, openTag);
    return false;
  };

  const attrs = allAttrs(el);
  const at = (name) => attrs.find((a) => a.name === name) ?? null;

  // ---- unmapped tooltip behaviour refuses the element. See the header.
  for (const a of attrs) {
    if (!TOOLTIP_NAMESPACE.has(a.name) || RENAME.has(a.name)) continue;
    return no(`${a.kind === "input" ? `[${a.name}]` : a.name} has no uiTooltip equivalent`);
  }

  const tooltip = at("pTooltip");
  if (!tooltip) return no("pTooltip vanished between the scan and the rewrite"); // unreachable

  // A structural directive re-shapes the element and moves the attribute onto the <ng-template>.
  // Nothing in this repo hits it, but guessing here is exactly what the contract forbids.
  if (ctx.templateAttrs?.length > 0) {
    return no(`structural directive (${ctx.templateAttrs.map((a) => a.name).join(", ")})`);
  }

  // ---- position must be one of the four sides. A bound one would be a `string` flowing into a union
  // input under strictTemplates; there are zero of them, and inventing a cast would be a guess.
  const position = at("tooltipPosition");
  if (position) {
    if (position.kind !== "attribute") {
      return no(
        `[tooltipPosition]="${position.value}" is bound — hand-convert to [uiTooltipPosition]`,
      );
    }
    if (!VALID_POSITIONS.has(position.value)) {
      return no(`tooltipPosition="${position.value}" is not one of top/bottom/left/right`);
    }
  }

  // =============================================================================================
  // Nothing below refuses. Queue the edits.
  // =============================================================================================
  for (const [from, to] of RENAME) {
    const a = at(from);
    if (!a) continue;
    renameAttr(splicer, a, to);
  }

  if (position) {
    stats.position++;
    bump(positionCount, position.value);
  }
  if (at("showDelay")) stats.delay++;
  if (tooltip.kind === "input") stats.bound++;
  else stats.static++;
  bump(hostTags, el.name ?? el.tagName);
  return true;
}

// -----------------------------------------------------------------------------------------------
// one template
// -----------------------------------------------------------------------------------------------

/**
 * The conservation check. A tooltip that silently loses its text is a button with no hint and no
 * error — so the number of tooltip hosts must be identical before and after, every time.
 */
function conserveTooltips(file, before, after) {
  const count = (src) => (src.match(/(^|\s)\[?pTooltip\]?=|(^|\s)\[?uiTooltip\]?=/g) ?? []).length;
  const b = count(before);
  const a = count(after);
  if (b !== a) {
    console.error(
      `\nFATAL: ${relative(file)} — tooltip count changed ${b} -> ${a}.\n` +
        `A dropped tooltip is a silent loss of a hint. Refusing to write this file.`,
    );
    process.exit(1);
  }
}

function sweepTemplate(file, source) {
  let nodes;
  try {
    nodes = parse(source, file);
  } catch (e) {
    refuse(file, 0, `template parse failed: ${e.message}`, "");
    return { src: source, changed: false };
  }

  const splicer = new Splicer(source, relative(file));

  visitElements(nodes, (el, ctx) => {
    const attrs = allAttrs(el);
    if (!attrs.some((a) => a.name === "pTooltip")) return;
    stats.seen++;
    if (convertTooltip(el, ctx, source, file, splicer)) stats.rewritten++;
    else stats.refused++;
  });

  if (splicer.size === 0) return { src: source, changed: false };
  const out = splicer.apply();
  conserveTooltips(file, source, out);
  return { src: out, changed: true };
}

// -----------------------------------------------------------------------------------------------
// TS bookkeeping
// -----------------------------------------------------------------------------------------------

/** The component that owns `template.html`, or null if it is ambiguous / shared. */
function ownerComponent(templateFile) {
  const ts = templateFile.replace(/\.html$/, ".ts");
  let src;
  try {
    src = readText(ts);
  } catch {
    return null;
  }
  const base = path.basename(templateFile);
  if (!new RegExp(`templateUrl:\\s*["'\`]\\./${base.replace(/\./g, "\\.")}["'\`]`).test(src))
    return null;
  return { file: ts, src };
}

/** `@logistics/shared/ui` from an app; a relative path from inside the shared lib itself (no cycle). */
function tooltipImportSpecifier(tsFile) {
  const rel = relative(tsFile);
  if (!rel.startsWith("projects/shared/")) return "@logistics/shared/ui";
  const from = path.dirname(path.join(WORKSPACE_ROOT, rel));
  const to = path.join(WORKSPACE_ROOT, TOOLTIP_DIRECTIVE);
  let spec = path.relative(from, to).split(path.sep).join("/");
  if (!spec.startsWith(".")) spec = `./${spec}`;
  return spec;
}

const dropTooltipModule = (src) => {
  let out = removeImportSpecifier(src, "primeng/tooltip", "TooltipModule").src;
  out = removeImportSpecifier(out, "primeng/tooltip", "Tooltip").src;
  return out.replace(/(\bimports:\s*\[)([^\]]*)\]/, (_m, head, body) => {
    const kept = body
      .split(",")
      .map((s) => s.trim())
      .filter(Boolean)
      .filter((n) => n !== "TooltipModule" && n !== "Tooltip");
    return `${head}${kept.join(", ")}]`;
  });
};

/**
 * Wire the TS for a template that now uses `uiTooltip`:
 *   + UiTooltip in @Component.imports (and its import statement)
 *   - TooltipModule from 'primeng/tooltip', but ONLY once nothing in the template still needs it.
 *
 * `stillNeedsPrimeTooltip` is the load-bearing half: a file with one REFUSED pTooltip left in it
 * still needs the PrimeNG import, and stripping it would turn a visible, listed refusal into a
 * template that silently renders no tooltip at all.
 */
function wireTs(templateFile, finalHtml) {
  const owner = ownerComponent(templateFile);
  if (!owner) {
    refuse(templateFile, 0, "cannot resolve the owning component — UiTooltip import not added", "");
    return null;
  }
  let src = owner.src;

  const added = addToComponentImports(src, "UiTooltip");
  if (!added.changed && added.reason !== "already in imports") {
    refuse(owner.file, 0, `could not add UiTooltip to @Component imports: ${added.reason}`, "");
    return null;
  }
  src = addImport(added.src, "UiTooltip", tooltipImportSpecifier(owner.file)).src;

  if (!/\bpTooltip\b/.test(finalHtml)) src = dropTooltipModule(src);
  return { file: owner.file, src };
}

/**
 * Components that import TooltipModule and never render a tooltip. Invisible: no error, no warning,
 * just a PrimeNG module in the bundle graph of a component that does not use it. A template-driven
 * sweep never reaches them, so they get their own pass.
 */
function pruneUnusedTooltipImports(mode, touchedTs) {
  const pruned = [];
  for (const ts of listFiles({ dirs: ["projects"], ext: [".ts"] })) {
    const src = touchedTs.get(ts) ?? readText(ts);
    if (!/from ["']primeng\/tooltip["']/.test(src)) continue;

    let template = "";
    try {
      template = readText(ts.replace(/\.ts$/, ".html"));
    } catch {
      // no sibling template (a directive, a service) — inline templates are not allowed in this repo
    }
    if (/\bpTooltip\b/.test(template)) continue;

    const out = dropTooltipModule(src);
    if (out === src) continue;
    pruned.push(relative(ts));
    if (mode === "apply") touchedTs.set(ts, out);
  }
  return pruned;
}

// -----------------------------------------------------------------------------------------------
// main
// -----------------------------------------------------------------------------------------------

const mode = parseMode();
const touchedHtml = [];
const touchedTs = new Map();

for (const file of listFiles({ dirs: ["projects"], ext: [".html"] })) {
  const source = readText(file);
  if (!/\bpTooltip\b/.test(source)) continue;

  const { src, changed } = sweepTemplate(file, source);
  if (!changed) continue;

  const ts = wireTs(file, src);
  touchedHtml.push(file);
  if (mode === "apply") {
    writeText(file, src);
    if (ts) touchedTs.set(ts.file, ts.src);
  }
}

const prunedUnused = pruneUnusedTooltipImports(mode, touchedTs);

if (mode === "apply") {
  for (const [file, src] of touchedTs) writeText(file, src);
  runPrettier([...touchedHtml, ...touchedTs.keys()]);
}

// -----------------------------------------------------------------------------------------------
// report
// -----------------------------------------------------------------------------------------------

const line = "=".repeat(86);
console.log(`\ntooltip-sweep --${mode}\n${line}`);
console.log(`pTooltip hosts seen    ${String(stats.seen).padStart(4)}`);
console.log(
  `  rewritten            ${String(stats.rewritten).padStart(4)}   (${stats.static} static, ${stats.bound} bound)`,
);
console.log(`  REFUSED              ${String(stats.refused).padStart(4)}`);
console.log(`\nfiles: ${touchedHtml.length} template(s), ${touchedTs.size || "?"} component(s)`);

console.log(`\nmappings applied`);
console.log(`  pTooltip -> uiTooltip                 ${String(stats.rewritten).padStart(4)}`);
console.log(`  tooltipPosition -> uiTooltipPosition  ${String(stats.position).padStart(4)}`);
console.log(`  [showDelay] -> [uiTooltipDelay]       ${String(stats.delay).padStart(4)}`);

console.log(`\npositions (absent => "right", PrimeNG's default, which uiTooltip also defaults to)`);
for (const [k, v] of [...positionCount.entries()].sort((a, b) => b[1] - a[1])) {
  console.log(`  ${k.padEnd(10)}${String(v).padStart(4)}`);
}

console.log(`\nhost elements`);
for (const [k, v] of [...hostTags.entries()].sort((a, b) => b[1] - a[1])) {
  console.log(`  <${k}>`.padEnd(28) + String(v).padStart(4));
}

console.log(`\n${line}\nREFUSALS (${refusals.length}) — left as pTooltip for a human`);
for (const r of refusals)
  console.log(`  ${r.file}:${r.line}\n      ${r.reason}\n      ${r.snippet}`);

if (prunedUnused.length > 0) {
  console.log(`\n${line}\nUNUSED primeng/tooltip IMPORTS PRUNED (${prunedUnused.length})`);
  for (const f of prunedUnused) console.log(`  ${f}`);
}

writeText(
  REFUSALS_OUT,
  `${JSON.stringify({ generatedBy: "tools/codemods/tooltip-sweep.mjs", mode, stats, refusals }, null, 2)}\n`,
);
console.log(`\nrefusal list -> ${relative(REFUSALS_OUT)}`);

if (mode === "check") {
  const remaining = [];
  for (const file of listFiles({ dirs: ["projects"], ext: [".html"] })) {
    // Strip HTML comments first: a commented-out attribute renders nothing, so counting it would fail
    // the gate on dead code — and invite someone to "fix" it by deleting a comment.
    const src = readText(file).replace(/<!--[\s\S]*?-->/g, "");
    const n = (src.match(/\bpTooltip\b/g) ?? []).length;
    if (n > 0) remaining.push(`${relative(file)}  ${n} pTooltip`);
  }
  if (remaining.length > 0) {
    console.error(`\n--check FAILED: ${remaining.length} file(s) still carry pTooltip:`);
    for (const r of remaining) console.error(`  ${r}`);
    process.exit(1);
  }
  console.log("\n--check OK: no pTooltip remains.");
}
