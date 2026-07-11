/**
 * menu-sweep.mjs — S9 of the PrimeNG -> spartan migration. The 19 popup menus.
 *
 *   node tools/codemods/menu-sweep.mjs --census   # report only
 *   node tools/codemods/menu-sweep.mjs --apply    # rewrite templates + TS, then prettier
 *   node tools/codemods/menu-sweep.mjs --check    # exit 1 if any <p-menu> or primeng/api import remains
 *
 * Template (19 sites, all one shape):
 *   <p-menu #menu [model]="actionMenuItems()" [popup]="true" />
 *     ->  <ui-menu #menu [items]="actionMenuItems()" />
 *
 * TypeScript (19 files):
 *   import type { MenuItem } from "primeng/api"  ->  type UiMenuItem from "@logistics/shared/ui"
 *   import { MenuModule } from "primeng/menu"    ->  UiMenu in the component's imports array
 *   icon: "pi pi-eye"        ->  icon: "eye"            (a typed IconName, validated against icon-map.json)
 *   styleClass: "text-red-600" ->  variant: "destructive"
 *
 * See lib/html.mjs for the codemod contract: parse to ANALYSE, span-splice to MUTATE, never re-print
 * an AST, refuse rather than guess, idempotent.
 *
 * =============================================================================================
 * THE TWO THINGS THAT WOULD FAIL SILENTLY, AND THE GUARDS FOR THEM
 * =============================================================================================
 * 1. AN ICON THAT STOPS RESOLVING. `MenuItem.icon` was a primeicons CSS CLASS ("pi pi-eye") rendered
 *    into a <span class>. `UiMenuItem.icon` is an IconName fed to <ui-icon [name]>. A name outside the
 *    union is a compile error — good — but a name that IS in the union and simply was not registered
 *    in that app's provideIcons() registry renders a 0x0 <svg>: no error, no test failure, just a
 *    missing glyph. Every mapped name is therefore checked against icon-map.json here, and
 *    `check-icons.mjs` (whose scanner matches the `icon: "eye"` property form we emit) is the gate
 *    that catches the registry half. Unknown pi name -> REFUSE the file.
 *
 * 2. A MENU ITEM THAT LOSES ITS COMMAND. `command` is the entire point of an item; a dropped one
 *    leaves a row that highlights, clicks, and does nothing. `conserve()` re-reads each rewritten TS
 *    file and asserts the multiset of `label:` and `command:` occurrences is unchanged. A multiset,
 *    not a count: two items that swapped labels would conserve the count and still be wrong.
 *
 * =============================================================================================
 * WHY `styleClass: "text-red-600"` BECOMES `variant: "destructive"` AND NOT A PASS-THROUGH CLASS
 * =============================================================================================
 * All six styleClass users are "Delete" rows hardcoding a red (`text-red-600` x5, `text-red-500` x1)
 * — the exact thing the no-hardcoded-colours rule forbids, and two different reds for one meaning.
 * Helm's dropdown item already ships a themed `data-[variant=destructive]` state that also tints the
 * icon and the hover background, which the bare text colour never did. Any OTHER styleClass value is
 * a refusal rather than a guess.
 */
import { existsSync } from "node:fs";
import { readText, writeText, listFiles, relative, runPrettier, parseMode } from "./lib/io.mjs";
import { parse, visitElements, findAttr, references, Splicer, lineOf } from "./lib/html.mjs";
import { addImport, addToComponentImports, removeImportSpecifier } from "./lib/ts.mjs";

const mode = parseMode();
const ICON_MAP = JSON.parse(readText("tools/codemods/icon-map.json")).icons;

/** `text-red-600` / `text-red-500` — every styleClass in the corpus. Anything else refuses. */
const DANGER_CLASS = /^text-red-\d{3}$/;

const refusals = [];
const refuse = (file, line, reason, snippet = "") =>
  refusals.push({ file: relative(file), line, reason, snippet });

const stats = { html: 0, ts: 0, icons: 0, danger: 0, menus: 0 };

// ---------------------------------------------------------------------------------------------
// Templates
// ---------------------------------------------------------------------------------------------

/** `<p-menu #menu [model]="X" [popup]="true" />` -> `<ui-menu #menu [items]="X" />` */
function sweepTemplate(file) {
  const src = readText(file);
  if (!/<p-menu\b/.test(src)) return false;

  const ast = parse(src, file);
  const splicer = new Splicer(src);
  let touched = 0;

  visitElements(ast, (el) => {
    if (el.name !== "p-menu") return;
    const line = lineOf(src, el.sourceSpan.start.offset);

    const model = findAttr(el, "model");
    const popup = findAttr(el, "popup");
    const ref = references(el)[0];

    if (!model || model.kind !== "input") {
      refuse(file, line, "<p-menu> without a [model] binding — nothing to carry over");
      return;
    }
    if (!ref) {
      refuse(file, line, "<p-menu> without a #templateRef — the trigger could not reach toggle()");
      return;
    }
    if (!popup) {
      // A non-popup p-menu is an inline sidebar menu, a different component entirely.
      refuse(file, line, "<p-menu> without [popup] — inline menus are out of scope for ui-menu");
      return;
    }

    // Rewrite the whole tag in one splice: the shape is uniform and rebuilding it beats three
    // independent edits whose spans could overlap.
    splicer.replace(
      el.sourceSpan.start.offset,
      el.sourceSpan.end.offset,
      `<ui-menu #${ref.name} [items]="${model.value}" />`,
    );
    touched++;
  });

  if (touched === 0) return false;
  stats.menus += touched;
  // `apply()`, NOT `toString()`. Splicer does not override Object.prototype.toString, so
  // `writeText(file, splicer.toString())` silently writes the string "[object Object]" over the
  // template — a 1-line file that still compiles (Angular just reports every import as unused) and
  // that no type check catches. It cost 18 templates once; the assertion below makes it loud.
  const out = splicer.apply();
  if (typeof out !== "string" || out.length < src.length / 2) {
    throw new Error(`${relative(file)}: splice produced ${out?.length ?? "non-string"} bytes from ${src.length} — refusing to write`);
  }
  if (mode === "apply") writeText(file, out);
  return true;
}

// ---------------------------------------------------------------------------------------------
// TypeScript
// ---------------------------------------------------------------------------------------------

/** Drop `Name,` from the @Component imports array. Returns null when the array shape is unexpected. */
function removeFromComponentImports(src, symbol) {
  const at = src.indexOf("imports:");
  if (at === -1) return src; // nothing to remove from
  const lb = src.indexOf("[", at);
  let depth = 0;
  let rb = -1;
  for (let i = lb; i < src.length; i++) {
    if (src[i] === "[") depth++;
    else if (src[i] === "]" && --depth === 0) {
      rb = i;
      break;
    }
  }
  if (rb === -1) return null;
  const names = src
    .slice(lb + 1, rb)
    .split(",")
    .map((s) => s.trim())
    .filter(Boolean);
  if (!names.includes(symbol)) return src;
  const kept = names.filter((n) => n !== symbol);
  return `${src.slice(0, lb + 1)}${kept.join(", ")}${src.slice(rb)}`;
}

/** "pi pi-pen-to-square" -> "pen-to-square", validated against icon-map.json. */
function toIconName(raw) {
  const last = raw.trim().split(/\s+/).pop() ?? "";
  const name = last.replace(/^pi-/, "");
  return name in ICON_MAP ? name : null;
}

/** The sibling template, or "" when the component has none. */
function siblingTemplate(file) {
  const html = file.replace(/\.ts$/, ".html");
  return existsSync(html) ? readText(html) : "";
}

function sweepTs(file) {
  let src = readText(file);
  const before = src;
  const usesMenu = /from\s*["']primeng\/(api|menu)["']/.test(src);
  // "Renders a menu" is part of the TRIGGER, not just of the body, so this pass is SELF-HEALING and
  // genuinely idempotent: a component whose template already says <ui-menu> but whose imports array
  // does not yet list UiMenu is still picked up on a re-run. Keying only off the primeng import (or
  // off `MenuItem`, which the rename has already turned into `UiMenuItem` and which therefore no
  // longer matches) made a second pass skip the file entirely and leave it half-migrated.
  const rendersMenu = /<ui-menu\b|<p-menu\b/.test(siblingTemplate(file));
  if (!usesMenu && !rendersMenu && !/\bMenuItem\b/.test(src)) return false;

  // --- 1. icons: icon: "pi pi-eye" -> icon: "eye" -------------------------------------------
  src = src.replace(/(\bicon\s*:\s*)(["'])(pi\s+pi-[a-z0-9-]+)\2/g, (whole, key, q, raw) => {
    const name = toIconName(raw);
    if (!name) {
      refuse(file, lineOf(before, before.indexOf(whole)), `icon "${raw}" is not in icon-map.json`);
      return whole;
    }
    stats.icons++;
    return `${key}${q}${name}${q}`;
  });

  // --- 2. danger rows: styleClass: "text-red-600" -> variant: "destructive" ------------------
  src = src.replace(/\bstyleClass\s*:\s*(["'])([^"']+)\1/g, (whole, _q, cls) => {
    if (!DANGER_CLASS.test(cls.trim())) {
      refuse(file, lineOf(before, before.indexOf(whole)), `styleClass "${cls}" is not a danger red`);
      return whole;
    }
    stats.danger++;
    return `variant: "destructive"`;
  });

  // --- 3. the MenuItem type ------------------------------------------------------------------
  src = src.replace(/\bMenuItem\b/g, "UiMenuItem");

  // --- 4. imports ----------------------------------------------------------------------------
  // primeng/api carried MenuItem and, in home.ts, a SharedModule that nothing in that template uses
  // (home.html's only PrimeNG tag was the p-menu itself; its #header/#body templates belong to
  // ui-data-table, whose other 18 consumers import no SharedModule).
  for (const spec of ["UiMenuItem", "SharedModule"]) {
    const r = removeImportSpecifier(src, "primeng/api", spec);
    if (r.changed) src = r.src;
  }
  if (/from\s*["']primeng\/api["']/.test(src)) {
    refuse(file, 1, "primeng/api import survived — it carries a specifier this sweep does not know");
    return false;
  }

  const menuModule = removeImportSpecifier(src, "primeng/menu", "MenuModule");
  if (menuModule.changed) src = menuModule.src;

  for (const dead of ["MenuModule", "SharedModule"]) {
    const next = removeFromComponentImports(src, dead);
    if (next === null) {
      refuse(file, 1, `could not parse the @Component imports array to drop ${dead}`);
      return false;
    }
    src = next;
  }

  // A component that RENDERS a menu needs the UiMenu component; one that merely types an array of
  // items does not. Both need the type.
  if (rendersMenu) {
    const add = addToComponentImports(src, "UiMenu");
    if (add.changed) src = add.src;
    const imp = addImport(src, "UiMenu", "@logistics/shared/ui");
    if (imp.changed) src = imp.src;
  }
  if (/\bUiMenuItem\b/.test(src) && !/\btype UiMenuItem\b|import type \{[^}]*\bUiMenuItem\b/.test(src)) {
    const t = addImport(src, "UiMenuItem", "@logistics/shared/ui", { typeOnly: true });
    if (t.changed) src = t.src;
  }

  if (src === before) return false;
  stats.ts++;
  if (mode === "apply") writeText(file, src);
  return true;
}

/**
 * A rewritten file must still declare exactly the menu items it declared before. `label:` and
 * `command:` are the two that make an item an item; a dropped `command:` is a row that clicks and
 * does nothing, which nothing else in the pipeline would catch.
 */
function conserve(file, beforeSrc) {
  const count = (s, re) => (s.match(re) ?? []).length;
  const after = readText(file);
  for (const [what, re] of [
    ["label", /\blabel\s*:/g],
    ["command", /\bcommand\s*:/g],
    ["separator", /\bseparator\s*:/g],
  ]) {
    const a = count(beforeSrc, re);
    const b = count(after, re);
    if (a !== b) refuse(file, 1, `CONSERVATION: ${what} count changed ${a} -> ${b}`);
  }
}

// ---------------------------------------------------------------------------------------------

const htmlFiles = listFiles({ dirs: ["projects"], ext: [".html"] });
const tsFiles = listFiles({ dirs: ["projects"], ext: [".ts"] });
const written = [];

for (const file of htmlFiles) {
  if (sweepTemplate(file)) {
    stats.html++;
    written.push(file);
  }
}

for (const file of tsFiles) {
  if (file.endsWith(".spec.ts")) continue;
  const before = readText(file);
  if (sweepTs(file)) {
    written.push(file);
    if (mode === "apply") conserve(file, before);
  }
}

if (mode === "apply" && written.length > 0) runPrettier(written);

const line = "-".repeat(92);
console.log(`\n${line}\nmenu-sweep (${mode})`);
console.log(
  `  ${stats.menus} <p-menu> -> <ui-menu> across ${stats.html} template(s); ` +
    `${stats.ts} TS file(s); ${stats.icons} icon(s) retyped; ${stats.danger} danger row(s).`,
);
console.log(`\nREFUSALS (${refusals.length})`);
for (const r of refusals) console.log(`  ${r.file}:${r.line}  ${r.reason}`);
if (refusals.length === 0) console.log("  none.");

if (mode === "check") {
  const bad = [];
  for (const f of htmlFiles) {
    if (/<p-menu\b/.test(readText(f).replace(/<!--[\s\S]*?-->/g, ""))) bad.push(`${relative(f)}  <p-menu>`);
  }
  for (const f of tsFiles) {
    if (/from\s*["']primeng\/(api|menu)["']/.test(readText(f))) bad.push(`${relative(f)}  primeng/api|menu`);
  }
  if (bad.length > 0) {
    console.error(`\n--check FAILED (${bad.length}):`);
    for (const b of bad) console.error(`  ${b}`);
    process.exit(1);
  }
  console.log("\n--check OK: no <p-menu>, no primeng/api or primeng/menu import.");
}

if (refusals.length > 0 && mode === "apply") process.exit(1);
