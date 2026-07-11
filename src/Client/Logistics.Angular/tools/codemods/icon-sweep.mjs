/**
 * icon-sweep.mjs — S3 of the PrimeNG -> spartan migration: move OUR icon call sites onto <ui-icon>.
 *
 *   node tools/codemods/icon-sweep.mjs --census   # report only; writes .icon-refusals.json
 *   node tools/codemods/icon-sweep.mjs --apply    # rewrite + prettier
 *   node tools/codemods/icon-sweep.mjs --check    # exit 1 if a rewritable site remains
 *
 * See lib/html.mjs for the codemod contract (parse to analyse, span-splice to mutate, refuse rather
 * than guess, idempotent).
 *
 * =============================================================================================
 * WHAT THIS DELIBERATELY DOES NOT TOUCH — and why. (Enumerated in the --census output.)
 * =============================================================================================
 * PrimeNG renders an `icon` input as a CSS CLASS STRING on an internal <span>, NOT as an element.
 * Rewriting `<p-button icon="pi pi-check">` to `icon="check"` while the host is still <p-button>
 * yields a BLANK icon — and the blank-icon detector looks for <ng-icon>, so it is structurally blind
 * to a `<span class="check">`. Those attributes migrate WITH THEIR HOST (S4 p-button, S5 p-tag/...).
 *
 * The same trap exists inside SOME OF OUR OWN components: they take an `icon` string and interpolate
 * it into a `pi ...` class string (or forward it to <p-button>). They are OURS but they are still
 * class-string consumers, so their call sites are LEFT until the component internals move to
 * <ui-icon>. The distinction is the CONSUMER, not the owner — see LEAVE_HOSTS vs UI_ICON_HOSTS below,
 * each entry traced by hand.
 *
 * S3 phase 2 converted the internals of `ui-dashboard-card`, `ui-empty-state`, `web-icon-circle` and
 * `web-page-hero[badgeIcon]` to <ui-icon>, so those moved LEAVE -> UI_ICON. What is left of our own is
 * only what still feeds a PrimeNG component: `ui-empty-state[actionIcon]`, `web-page-hero[ctaIcon]`
 * and `ui-action-menu` (MenuItem.icon).
 */

import path from "node:path";
import {
  listFiles,
  parseMode,
  readText,
  relative,
  runPrettier,
  WORKSPACE_ROOT,
  writeText,
} from "./lib/io.mjs";
import { allAttrs, lineOf, parse, Splicer, staticAttrs, visitElements } from "./lib/html.mjs";
import { addImport, addToComponentImports } from "./lib/ts.mjs";

const ICON_MAP = JSON.parse(readText(path.join(WORKSPACE_ROOT, "tools/codemods/icon-map.json"))).icons;
const REFUSALS_OUT = path.join(WORKSPACE_ROOT, "tools/codemods/.icon-refusals.json");
const ICON_COMPONENT = "projects/shared/src/lib/ui/content/icon/icon";

/**
 * primeicons class tokens that are NOT glyphs. `pi-spin` is a CSS KEYFRAME: it always rides along
 * with a real icon (`pi pi-spin pi-spinner`) and becomes the `spin` input. We TOKENIZE the class list
 * rather than regex-stripping a `pi-` prefix, because a naive strip turns "pi pi-spin pi-spinner" into
 * "spin pi-spinner" — the exact mangled literal S2 had to clean up.
 */
const PI_MODIFIERS = new Map([["pi-spin", "spin"]]);

/**
 * A tailwind FONT-SIZE class. Load-bearing: ui-icon puts its own size class (default `md` ->
 * `text-base`) directly on the inner <ng-icon>, which would WIN over a `text-4xl` carried onto the
 * host and silently shrink the glyph to 16px. `size="inherit"` emits no size class at all, so the
 * host's font-size governs — that escape hatch exists for exactly this. Colour classes
 * (`text-muted-foreground`, `text-danger`) are NOT font sizes and pass through untouched.
 */
const FONT_SIZE_CLASS = /^text-(xs|sm|base|lg|xl|[2-9]xl)$/;

/**
 * Attributes that may ride from the <i> onto the <ui-icon> verbatim. <ui-icon> is a normal element
 * host, so ARIA / title / a tooltip directive keep working unchanged. Anything NOT listed here is a
 * REFUSAL rather than a guess — dropping an unknown binding is exactly the silent damage the contract
 * exists to prevent.
 */
const isPassthrough = (a) =>
  a.kind === "attribute" &&
  (/^(aria-|data-)/.test(a.name) || ["title", "id", "style", "tooltipPosition"].includes(a.name));
const isTooltipDirective = (a) => a.name === "pTooltip" || a.name === "tooltipPosition";

/**
 * Hosts whose icon-ish attribute is (or becomes) a CSS CLASS STRING. LEAVE — the attribute migrates
 * with its host. Each `why` was traced to the actual consumer, not pattern-matched.
 */
const LEAVE_HOSTS = new Map([
  ["p-button", { attrs: ["icon"], why: "PrimeNG: icon -> class string on an internal <span>. Migrates with the host in S4." }],
  ["p-tag", { attrs: ["icon"], why: "PrimeNG: icon -> class string. Migrates with the host in S5." }],
  ["p-inputicon", { attrs: ["class"], why: "PrimeNG: the icon IS the class on the component. Migrates with the host in S9." }],
  ["ui-empty-state", { attrs: ["actionIcon"], why: "actionIcon -> <p-button [icon]>, still a class string. Migrates with the button in S4. (`icon` is now <ui-icon>-backed — see UI_ICON_HOSTS.)" }],
  ["web-page-hero", { attrs: ["ctaIcon"], why: "ctaIcon -> <p-button [icon]>, still a class string. Migrates with the button in S4. (`badgeIcon` is now <ui-icon>-backed — see UI_ICON_HOSTS.)" }],
  ["ui-action-menu", { attrs: ["buttonIcon"], why: "OURS but a class-string consumer: items[].icon -> MenuItem.icon on <p-menu>; buttonIcon -> <p-button [icon]>. Migrates with the menu in S9." }],
]);

/**
 * Hosts whose icon input is typed `IconName` and flows into `<ui-icon [name]>`. A `pi-`-prefixed
 * spelling at these call sites is now a COMPILE ERROR (resolveNgIcon no longer strips the prefix), so
 * the codemod normalises them to the canonical bare name.
 *
 * S3 phase 2 moved `ui-dashboard-card`, `ui-empty-state[icon]`, `web-page-hero[badgeIcon]`,
 * `web-icon-circle` and `web-icon-card` here from LEAVE_HOSTS: their internals now render <ui-icon>
 * and their inputs are typed `IconName`. `ui-action-menu` stayed behind — it still feeds
 * PrimeNG MenuItem.icon.
 */
const UI_ICON_HOSTS = new Map([
  ["app-stat-card", ["icon"]],
  ["ui-dashboard-card", ["icon"]],
  ["ui-empty-state", ["icon"]],
  ["ui-data-container", ["emptyIcon"]],
  ["app-map-container", ["emptyIcon"]],
  ["web-page-hero", ["badgeIcon"]],
  ["web-icon-circle", ["icon"]],
  ["web-icon-card", ["icon"]],
]);

// -----------------------------------------------------------------------------------------------
// name resolution
// -----------------------------------------------------------------------------------------------

/** `pi pi-check` / `pi-check` / `check` -> `check`, but ONLY if the result is a canonical map key. */
function canonical(raw) {
  const last = (raw ?? "").trim().split(/\s+/).pop() ?? "";
  const bare = last.startsWith("pi-") ? last.slice(3) : last;
  return bare in ICON_MAP ? bare : null;
}

// -----------------------------------------------------------------------------------------------
// the sweep
// -----------------------------------------------------------------------------------------------

const refusals = [];
const refuse = (rule, file, line, reason, snippet) =>
  refusals.push({ rule, file: relative(file), line, reason, snippet: (snippet ?? "").trim().slice(0, 120) });

const counts = {};
const bump = (rule, key) => {
  counts[rule] ??= { rewritten: 0, refused: 0, left: 0 };
  counts[rule][key]++;
};

/** Build the `<ui-icon … />` replacement for a sole-purpose <i> icon carrier. Returns null on refusal. */
function buildUiIcon(el, source, file) {
  const line = lineOf(source, el.sourceSpan.start.offset);
  const snippet = source.slice(el.sourceSpan.start.offset, (el.endSourceSpan ?? el.startSourceSpan).end.offset);

  // Children => not a sole-purpose icon carrier.
  const kids = (el.children ?? []).filter(
    (c) => !(c.constructor?.name === "Text" && !c.value?.trim()),
  );
  if (kids.length > 0) {
    refuse("R1", file, line, "element has children — not a sole-purpose icon carrier", snippet);
    return null;
  }

  const attrs = allAttrs(el);
  const cls = attrs.find((a) => a.kind === "attribute" && a.name === "class");
  if (!cls?.value) {
    refuse("R1", file, line, "no static class to read the icon name from", snippet);
    return null;
  }

  // TOKENIZE — never regex-strip a `pi-` prefix off the whole string.
  const tokens = cls.value.split(/\s+/).filter(Boolean);
  const iconTokens = tokens.filter((t) => t.startsWith("pi-") && !PI_MODIFIERS.has(t));
  const modifiers = tokens.filter((t) => PI_MODIFIERS.has(t));
  const kept = tokens.filter((t) => t !== "pi" && !t.startsWith("pi-"));

  if (iconTokens.length === 0) {
    refuse("R1", file, line, `class has no icon token (only "${tokens.join(" ")}") — likely a [class.pi-*] conditional`, snippet);
    return null;
  }
  if (iconTokens.length > 1) {
    refuse("R1", file, line, `ambiguous: ${iconTokens.length} icon tokens (${iconTokens.join(", ")})`, snippet);
    return null;
  }

  const name = canonical(iconTokens[0]);
  if (!name) {
    refuse("R1", file, line, `"${iconTokens[0]}" is not a key in icon-map.json`, snippet);
    return null;
  }

  // Every remaining attribute must be one we understand.
  const carried = [];
  for (const a of attrs) {
    if (a === cls) continue;
    if (isPassthrough(a) || isTooltipDirective(a)) {
      carried.push(source.slice(a.sourceSpan.start.offset, a.sourceSpan.end.offset));
      continue;
    }
    const shown = a.kind === "attribute" ? a.name : `[${a.name}]`;
    refuse("R1", file, line, `unsupported attribute ${shown} — refusing to drop or guess it`, snippet);
    return null;
  }

  // A font-size class on the host would be overridden by ui-icon's own size class on <ng-icon>.
  const hasFontSize = kept.some((t) => FONT_SIZE_CLASS.test(t));

  const parts = [`name="${name}"`];
  if (hasFontSize) parts.push(`size="inherit"`);
  for (const m of modifiers) parts.push(PI_MODIFIERS.get(m));
  if (kept.length > 0) parts.push(`class="${kept.join(" ")}"`);
  parts.push(...carried);

  return `<ui-icon ${parts.join(" ")} />`;
}

/** Rewrite one template. Returns { src, changed } and records refusals. */
function sweepTemplate(file, source) {
  const splicer = new Splicer(source, relative(file));
  let nodes;
  try {
    nodes = parse(source, file);
  } catch (e) {
    refuse("PARSE", file, 0, e.message, "");
    return { src: source, changed: false };
  }

  visitElements(nodes, (el, ctx) => {
    const tag = el.name ?? el.tagName;
    const attrs = allAttrs(el);
    const line = lineOf(source, el.sourceSpan.start.offset);
    const snippet = source.slice(el.sourceSpan.start.offset, Math.min(source.length, el.startSourceSpan.end.offset));

    // ---- R3: [class.pi-x]="expr" conditional bindings on ANY element — hand-handled in phase 2.
    const classBindings = attrs.filter((a) => a.kind === "input" && a.name.startsWith("pi-"));
    if (classBindings.length > 0) {
      bump("R3", "refused"); // one refusal per ELEMENT, not per binding
      refuse("R3", file, line, `[class.${classBindings.map((c) => c.name).join("] / [class.")}] conditional icon binding`, snippet);
      return;
    }

    // ---- LEAVE list: PrimeNG + our own class-string consumers.
    const leave = LEAVE_HOSTS.get(tag);
    if (leave) {
      for (const a of attrs) {
        if (!leave.attrs.includes(a.name)) continue;
        if (!/\bpi[- ]/.test(a.value ?? "")) continue;
        bump(`LEAVE <${tag}>`, "left");
      }
      return;
    }

    // ---- R4: <ui-icon name="LEGACY"> -> canonical key.
    if (tag === "ui-icon") {
      const nameAttr = attrs.find((a) => a.kind === "attribute" && a.name === "name");
      if (nameAttr?.value && /^pi[- ]|\s/.test(nameAttr.value)) {
        const c = canonical(nameAttr.value);
        if (!c) {
          bump("R4", "refused");
          refuse("R4", file, line, `<ui-icon name="${nameAttr.value}"> does not resolve to a map key`, snippet);
          return;
        }
        splicer.replace(nameAttr.valueSpan.start.offset, nameAttr.valueSpan.end.offset, c, "ui-icon name");
        bump("R4", "rewritten");
      }
      return;
    }

    // ---- R5: icon props on hosts whose input flows into <ui-icon [name]>.
    const uiHost = UI_ICON_HOSTS.get(tag);
    if (uiHost) {
      for (const a of attrs) {
        if (!uiHost.includes(a.name) || a.kind !== "attribute" || !a.valueSpan) continue;
        if (!/\bpi[- ]/.test(a.value ?? "")) continue;
        const c = canonical(a.value);
        if (!c) {
          bump("R5", "refused");
          refuse("R5", file, line, `<${tag} ${a.name}="${a.value}"> does not resolve to a map key`, snippet);
          continue;
        }
        splicer.replace(a.valueSpan.start.offset, a.valueSpan.end.offset, c, `${tag} ${a.name}`);
        bump("R5", "rewritten");
      }
      return;
    }

    // ---- R1 / R2: an element whose class list carries the icon.
    //
    // EVERY <i> in this repo is an icon carrier — verified: 180 <i> elements, zero used for italic
    // text. So an <i> is in scope even when its class is dynamic (`[class]="iconClasses()"`) or
    // interpolated (`class="pi {{ item.icon }}"`, which the parser reports as a BOUND attribute, not
    // a static one, and which a staticAttrs-only check would silently miss). Those cannot be resolved
    // to a name at build time, so they REFUSE — but they must still be SEEN, or phase 2 inherits a
    // worklist with holes in it.
    const cls = staticAttrs(el).find((a) => a.name === "class");
    const tokens = (cls?.value ?? "").split(/\s+/).filter(Boolean);
    const carriesPiClass = tokens.some((t) => t === "pi" || t.startsWith("pi-"));
    if (tag !== "i" && !carriesPiClass) return;

    const rule = tag === "i" ? "R1" : "R2";

    if (tag === "i" && !carriesPiClass) {
      const dyn = attrs.find((a) => a.kind === "input" && a.name === "class");
      bump(rule, "refused");
      refuse(rule, file, line,
        dyn
          ? `dynamic [class]="${(dyn.value ?? "").slice(0, 50)}" — icon name is not statically resolvable`
          : "interpolated/other class — icon name is not statically resolvable",
        snippet);
      return;
    }

    // A structural directive changes the shape of the emitted element — refuse.
    if (ctx.templateAttrs?.length > 0) {
      bump(rule, "refused");
      refuse(rule, file, line, `structural directive (${ctx.templateAttrs.map((a) => a.name).join(", ")}) on the icon carrier`, snippet);
      return;
    }

    const replacement = buildUiIcon(el, source, file);
    if (!replacement) {
      bump(rule, "refused");
      return;
    }

    const start = el.sourceSpan.start.offset;
    const end = (el.endSourceSpan ?? el.startSourceSpan).end.offset;
    splicer.replace(start, end, replacement, `${rule} <${tag}>`);
    bump(rule, "rewritten");
  });

  if (splicer.size === 0) return { src: source, changed: false };
  return { src: splicer.apply(), changed: true };
}

// -----------------------------------------------------------------------------------------------
// TS bookkeeping — a template that renders <ui-icon> must import Icon.
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
  if (!new RegExp(`templateUrl:\\s*["'\`]\\./${base.replace(/\./g, "\\.")}["'\`]`).test(src)) return null;
  return { file: ts, src };
}

/** `@logistics/shared/ui` from an app; a relative path from inside the shared lib itself. */
function iconImportSpecifier(tsFile) {
  const rel = relative(tsFile);
  if (!rel.startsWith("projects/shared/")) return "@logistics/shared/ui";
  const from = path.dirname(path.join(WORKSPACE_ROOT, rel));
  const to = path.join(WORKSPACE_ROOT, ICON_COMPONENT);
  let spec = path.relative(from, to).split(path.sep).join("/");
  if (!spec.startsWith(".")) spec = `./${spec}`;
  return spec;
}

function wireIconImport(templateFile) {
  const owner = ownerComponent(templateFile);
  if (!owner) {
    refuse("TS", templateFile, 0, "cannot resolve the owning component (shared or renamed template) — <ui-icon> import not added", "");
    return null;
  }
  const module = iconImportSpecifier(owner.file);

  const added = addToComponentImports(owner.src, "Icon");
  if (!added.changed && added.reason !== "already in imports") {
    refuse("TS", owner.file, 0, `could not add Icon to @Component imports: ${added.reason}`, "");
    return null;
  }
  const imported = addImport(added.src, "Icon", module);
  return { file: owner.file, src: imported.src };
}

// -----------------------------------------------------------------------------------------------
// main
// -----------------------------------------------------------------------------------------------

const mode = parseMode();
const templates = listFiles({ dirs: ["projects"], ext: [".html"] });

const touchedHtml = [];
const touchedTs = new Map();

/**
 * The prefilter must not hide a site from the census. `<i[\s/>]` is load-bearing: a carrier whose class
 * is purely dynamic (`<i [class]="iconClasses()">`) contains NO literal `pi`, so a pi-only prefilter
 * skips its file entirely and the site never gets counted or refused. (`<img` does not match — the
 * character class requires a delimiter right after the `i`.)
 */
const MAY_CARRY_ICON = /\bpi[- ]|<ui-icon|<i[\s/>]/;

for (const file of templates) {
  const source = readText(file);
  if (!MAY_CARRY_ICON.test(source)) continue;

  const { src, changed } = sweepTemplate(file, source);
  if (!changed) continue;

  // Only a NEW <ui-icon> element needs the import; a name-only normalisation (R4/R5) does not.
  const needsImport = /<ui-icon/.test(src) && !/<ui-icon/.test(source);

  if (mode === "apply") {
    if (writeText(file, src)) touchedHtml.push(file);
    if (needsImport) {
      const ts = wireIconImport(file);
      if (ts) touchedTs.set(ts.file, ts.src);
    }
  } else {
    touchedHtml.push(file);
    if (needsImport) wireIconImport(file); // surfaces TS refusals during --census
  }
}

if (mode === "apply") {
  for (const [file, src] of touchedTs) writeText(file, src);
  runPrettier([...touchedHtml, ...touchedTs.keys()]);
}

// ---- CSS (R7): primeicons selectors cannot be expressed against <ui-icon> — phase 2.
for (const file of listFiles({ dirs: ["projects"], ext: [".css"] })) {
  const src = readText(file);
  src.split("\n").forEach((l, i) => {
    if (/\bi\.pi-|\.pi-[a-z]/.test(l)) {
      bump("R7", "refused");
      refuse("R7", file, i + 1, "CSS selector targets a primeicons class — rewrite against <ui-icon> by hand", l);
    }
  });
}

// ---- TS literals (R5-TS): every one traced to a class-string / PrimeNG consumer. Enumerated, not rewritten.
for (const file of listFiles({ dirs: ["projects"], ext: [".ts"] })) {
  if (file.endsWith(".generated.ts")) continue;
  const src = readText(file);
  for (const m of src.matchAll(/\bicon\s*[:=]\s*["'][^"']*\bpi[- ][^"']*["']/g)) {
    bump("LEAVE ts-literal", "left");
  }
}

// -----------------------------------------------------------------------------------------------
// report
// -----------------------------------------------------------------------------------------------

const RULES = {
  R1: "<i class=\"pi pi-x\">            -> <ui-icon name=\"x\" />",
  R2: "class=\"pi pi-x\" on a non-<i>   -> <ui-icon name=\"x\" />",
  R3: "[class.pi-x]=\"expr\"            -> REFUSE (phase 2)",
  R4: "<ui-icon name=\"LEGACY\">        -> canonical key",
  R5: "our ui-icon-backed host props   -> canonical key",
  R7: "primeicons CSS selectors        -> REFUSE (phase 2)",
};

console.log(`\nicon-sweep --${mode}\n${"=".repeat(78)}`);
console.log("rule  rewritten  refused  what");
for (const [rule, label] of Object.entries(RULES)) {
  const c = counts[rule] ?? { rewritten: 0, refused: 0 };
  console.log(`${rule.padEnd(5)} ${String(c.rewritten).padStart(9)} ${String(c.refused).padStart(8)}  ${label}`);
}

console.log(`\nDELIBERATELY LEFT (migrates with its host — see THE ONE RULE):`);
for (const [rule, c] of Object.entries(counts).filter(([r]) => r.startsWith("LEAVE"))) {
  const why = LEAVE_HOSTS.get(rule.replace(/^LEAVE <|>$/g, ""))?.why ?? "traced to a PrimeNG / class-string consumer";
  console.log(`  ${String(c.left).padStart(4)}  ${rule.padEnd(24)} ${why}`);
}

const tsRefusals = refusals.filter((r) => r.rule === "TS");
console.log(`\nfiles: ${touchedHtml.length} template(s) ${mode === "apply" ? "rewritten" : "would change"}`);
console.log(`REFUSALS: ${refusals.length}${tsRefusals.length ? ` (incl. ${tsRefusals.length} TS import)` : ""}`);
for (const r of refusals) console.log(`  [${r.rule}] ${r.file}:${r.line}  ${r.reason}`);

writeText(REFUSALS_OUT, `${JSON.stringify({ generatedBy: "tools/codemods/icon-sweep.mjs", mode, refusals }, null, 2)}\n`);
console.log(`\nrefusal list -> ${relative(REFUSALS_OUT)}`);

if (mode === "check") {
  const rewritable = Object.entries(counts)
    .filter(([r]) => RULES[r])
    .reduce((n, [, c]) => n + c.rewritten, 0);
  if (rewritable > 0) {
    console.error(`\n--check FAILED: ${rewritable} rewritable icon site(s) remain. Run --apply.`);
    process.exit(1);
  }
  console.log("\n--check OK: no rewritable icon site remains.");
}
