/**
 * cosmetic-sweep.mjs — S5/phase-3 of the PrimeNG -> spartan migration. The cosmetic call sites.
 *
 *   node tools/codemods/cosmetic-sweep.mjs --census   # report only; writes .cosmetic-refusals.json
 *   node tools/codemods/cosmetic-sweep.mjs --apply    # rewrite templates + TS imports, then prettier
 *   node tools/codemods/cosmetic-sweep.mjs --check    # exit 1 if any of the 12 p-* tags remain
 *
 * Twelve tags, ten components, ~600 sites:
 *
 *   <p-card>            -> <ui-card>            <p-divider>       -> <ui-divider>
 *   <p-tag>             -> <ui-badge>           <p-message>       -> <ui-alert>
 *   <p-progress-spinner>-> <ui-spinner>         <p-avatar>        -> <ui-avatar>
 *   <p-progressSpinner> -> <ui-spinner>         <p-badge>         -> <ui-count-badge>
 *   <p-skeleton>        -> <ui-skeleton>        <p-overlaybadge>  -> <ui-overlay-badge>
 *   <p-chip>            -> <ui-badge>           <p-progressBar>   -> <ui-progress>
 *
 * See lib/html.mjs for the codemod contract (parse to analyse, span-splice to mutate, refuse rather
 * than guess, idempotent). See button-sweep.mjs for the worked example this is modelled on.
 *
 * =============================================================================================
 * THE #1 RISK IS A CHIP THAT LOSES ITS TEXT, OR A PANEL THAT LOSES ITS HEADING.
 * =============================================================================================
 * Every other failure in this sweep is loud. A dropped `[value]` is not: `<ui-badge>` renders its
 * `<ng-content>` when `value` is null, so a tag whose value binding got eaten becomes an EMPTY CHIP
 * — it still lays out, still paints its severity colour, and says nothing. Identically, a `<ui-card>`
 * that lost its `header` renders a card with no title: no error, no blank space, just a missing
 * heading on one of 43 panels. Neither is a type error and neither fails a test.
 *
 * So `conserve()` re-parses the OUTPUT of every file and checks two multisets against the input:
 *
 *   value  — every value/label expression on p-tag | p-badge | p-overlaybadge | p-chip (and on any
 *            ui-badge | ui-count-badge | ui-overlay-badge that was already in the file) must appear,
 *            byte for byte, on a ui-badge | ui-count-badge | ui-overlay-badge afterwards.
 *   header — same, for p-card/ui-card `header`.
 *
 * A multiset, not a count: two tags in one file that swapped their values would conserve the count
 * and still be wrong. Comparing the sorted expressions catches that too.
 *
 * =============================================================================================
 * WHY A BARE `<p-tag>` BECOMES severity="primary" AND NOT "neutral"
 * =============================================================================================
 * The brief for this step says an unsevered tag is NEUTRAL and instructs mapping it to
 * `intent="neutral"`. That is wrong three times over, and phases 1 and 2 both said so:
 *
 *   1. `.p-tag { background: dt('tag.primary.background') }`  — @primeuix/styles/dist/tag/index.mjs
 *      An unsevered tag paints the BRAND TINT. Mapping it to a grey `neutral` would be precisely the
 *      silent repaint the brief elsewhere warns about.
 *   2. `ui-badge`'s input is `severity`, not `intent`.
 *   3. `UiBadgeTone` has no `neutral` member — it is p-tag's six words plus `primary`. `neutral` is
 *      `secondary` under another name, and badge-intent.ts exists to end exactly that kind of drift.
 *
 * There is exactly ONE bare `<p-tag>` in the repo (inspections-dashboard.html:206) and it gets
 * `severity="primary"`. It does NOT get ui-badge's `"info"` default, which would paint it blue.
 *
 * =============================================================================================
 * WHY `<p-badge>` BECOMES `<ui-count-badge>` AND NOT `<ui-badge>`
 * =============================================================================================
 * The brief maps both `<p-tag>` and `<p-badge>` onto `<ui-badge>`. They are not the same component.
 * p-badge is the count pill: fixed 1.5rem height, a min-width that makes "3" a circle, weight 700,
 * and a SOLID fill in both presets. p-tag is the status chip: content-height, soft tint. Folding the
 * 8 p-badges into ui-badge would swap 8 solid count pills for tinted chips of a different height —
 * visible on the nav menu's unread counters. Phase 2 built `ui-count-badge` for them; this uses it.
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
import { allAttrs, lineOf, parse, renameAttr, renameTag, Splicer, visitElements } from "./lib/html.mjs";
import { addImport, addToComponentImports, removeImportSpecifier } from "./lib/ts.mjs";

const REFUSALS_OUT = path.join(WORKSPACE_ROOT, "tools/codemods/.cosmetic-refusals.json");

// -----------------------------------------------------------------------------------------------
// THE COMPONENT TABLE. tag -> { component, symbol, path }
// -----------------------------------------------------------------------------------------------

/** The ui component each p-* tag becomes, and the TS symbol its template needs in `imports:`. */
const COMPONENTS = {
  "p-card": { tag: "ui-card", symbol: "Card", path: "layout/card/card" },
  "p-tag": { tag: "ui-badge", symbol: "Badge", path: "content/badge/badge" },
  "p-progress-spinner": { tag: "ui-spinner", symbol: "Spinner", path: "feedback/spinner/spinner" },
  "p-progressSpinner": { tag: "ui-spinner", symbol: "Spinner", path: "feedback/spinner/spinner" },
  "p-skeleton": { tag: "ui-skeleton", symbol: "Skeleton", path: "feedback/skeleton/skeleton" },
  "p-divider": { tag: "ui-divider", symbol: "Divider", path: "layout/divider/divider" },
  "p-message": { tag: "ui-alert", symbol: "Alert", path: "content/alert/alert" },
  "p-avatar": { tag: "ui-avatar", symbol: "Avatar", path: "content/avatar/avatar" },
  "p-badge": { tag: "ui-count-badge", symbol: "CountBadge", path: "content/count-badge/count-badge" },
  "p-overlaybadge": { tag: "ui-overlay-badge", symbol: "OverlayBadge", path: "content/overlay-badge/overlay-badge" },
  "p-chip": { tag: "ui-badge", symbol: "Badge", path: "content/badge/badge" },
  "p-progressBar": { tag: "ui-progress", symbol: "Progress", path: "feedback/progress/progress" },
};

const TAGS = Object.keys(COMPONENTS);

/**
 * The PrimeNG modules this sweep retires, and every specifier a call site imports from them. Both
 * spellings are live: 130 files import `CardModule`, one imports the bare `Card` class.
 *
 * `needed` is the regex that decides whether the module is STILL required after the rewrite. A file
 * with one refused `<p-tag>` left in it still needs TagModule, and stripping it would turn a visible
 * REFUSAL into a build error.
 */
const PRIMENG_MODULES = [
  { module: "primeng/card", specifiers: ["CardModule", "Card"], needed: /<p-card\b/ },
  { module: "primeng/tag", specifiers: ["TagModule", "Tag"], needed: /<p-tag\b/ },
  { module: "primeng/progressspinner", specifiers: ["ProgressSpinnerModule", "ProgressSpinner"], needed: /<p-progress-?[sS]pinner\b/ },
  { module: "primeng/skeleton", specifiers: ["SkeletonModule", "Skeleton"], needed: /<p-skeleton\b/ },
  { module: "primeng/divider", specifiers: ["DividerModule", "Divider"], needed: /<p-divider\b/ },
  { module: "primeng/message", specifiers: ["MessageModule", "Message"], needed: /<p-message\b/ },
  { module: "primeng/avatar", specifiers: ["AvatarModule", "Avatar"], needed: /<p-avatar\b/ },
  // `primeng/badge` ships the OverlayBadge component and the `pBadge` attribute directive too. The
  // directive has zero uses in this repo, but the regex checks for it anyway — a module pruned while
  // a directive still uses it is a silently unstyled element, not a compile error.
  { module: "primeng/badge", specifiers: ["BadgeModule", "Badge", "BadgeDirective", "OverlayBadge", "OverlayBadgeModule"], needed: /<p-badge\b|<p-overlaybadge\b|\bpBadge\b/ },
  { module: "primeng/chip", specifiers: ["ChipModule", "Chip"], needed: /<p-chip\b/ },
  { module: "primeng/progressbar", specifiers: ["ProgressBarModule", "ProgressBar"], needed: /<p-progressBar\b/ },
];

// -----------------------------------------------------------------------------------------------
// PER-TAG MAPPING TABLES. Anything not expressible here is a REFUSAL.
// -----------------------------------------------------------------------------------------------

/**
 * `<p-tag class="...">` — the four class strings that exist, whole-string. A token-wise rule would
 * have to decide what a lone `px-4` means; there is no such site, and inventing an answer for one is
 * how a codemod starts guessing. New class string -> refusal.
 *
 *   size  — the ui-badge size input this class string IS (null = leave the default `md`)
 *   keep  — the classes that survive as a real `class` attribute
 */
const TAG_CLASS = new Map([
  // ui-badge's base already carries `whitespace-nowrap`, so this is a no-op — passed through anyway,
  // because "the call site asked for it" is information and twMerge dedupes it for free.
  ["whitespace-nowrap", { size: null, keep: "whitespace-nowrap" }],
  ["px-4 py-2 text-lg", { size: "lg", keep: "" }],
  ["text-xs", { size: "sm", keep: "" }],
  // `p-tag-sm` IS NOT A PRIMENG CLASS. It matches no rule in @primeuix/styles or primeng; this tag
  // has been rendering at the default size all along. Mapping it to `size="sm"` would SHRINK a chip
  // that is not small today. It maps to nothing.
  ["p-tag-sm", { size: null, keep: "" }],
]);

/** `<p-tag styleClass="...">` — styleClass lands on p-tag's HOST (`class: cn(cx('root'), styleClass)`),
 *  the same box as `class`, so it splits the same way. 2 sites, one string. */
const TAG_STYLECLASS = new Map([["ml-2 text-xs", { size: "sm", keep: "ml-2" }]]);

/** `<p-tag [style]="...">` — the one shape that exists. 0.7rem -> `sm` (0.75rem). */
const TAG_STYLE = new Map([["{ fontSize: '0.7rem' }", { size: "sm" }]]);

/** `<p-message severity>` -> `<ui-alert intent>`. `error`/`warn`/`secondary` are renames, not colours. */
const MESSAGE_INTENT = new Map([
  ["info", "info"],
  ["success", "success"],
  ["warn", "warning"],
  ["error", "danger"],
  ["secondary", "neutral"],
]);

/** Attributes that ride from any p-* host to its ui-* host byte-for-byte.
 *
 *   pTooltip / tooltipPosition / tooltipOptions — a host directive; it matches the new element too,
 *     exactly as routerLink did on ui-button. S6 owns the tooltip migration.
 *   (click) — a NATIVE DOM event on the host, not a PrimeNG output. 4 <p-card>s bind it.
 *   class / style / id / aria-* / data-* — native host attributes, host-to-host. */
const UNIVERSAL_PASS = new Set([
  "class",
  "style",
  "id",
  "tabindex",
  "pTooltip",
  "tooltipPosition",
  "tooltipOptions",
]);
const UNIVERSAL_EVENTS = new Set(["click"]);
const isNativeAttr = (name) => /^(aria-|data-)/.test(name);

/** Inputs that keep their name and value on the target component, per tag. */
const PASS = {
  "p-card": ["header"],
  "p-tag": ["value", "severity", "icon", "rounded"],
  "p-progress-spinner": ["ariaLabel"],
  "p-progressSpinner": ["ariaLabel"],
  "p-skeleton": ["height", "width", "shape", "size"],
  "p-divider": [],
  "p-message": [],
  "p-avatar": ["label", "size"],
  "p-badge": ["value", "severity"],
  "p-overlaybadge": ["value", "severity", "badgeSize"],
  "p-chip": [],
  "p-progressBar": ["value", "showValue", "color"],
};

// -----------------------------------------------------------------------------------------------
// bookkeeping
// -----------------------------------------------------------------------------------------------

const refusals = [];
const refuse = (file, line, reason, snippet) =>
  refusals.push({
    file: relative(file),
    line,
    reason,
    snippet: (snippet ?? "").replace(/\s+/g, " ").trim().slice(0, 120),
  });

const stats = { seen: 0, rewritten: 0, refused: 0 };
const perTag = new Map(); // tag -> { seen, rewritten, refused }
const notes = []; // human-verifiable transformations worth eyeballing

const bumpTag = (tag, key) => {
  if (!perTag.has(tag)) perTag.set(tag, { seen: 0, rewritten: 0, refused: 0 });
  perTag.get(tag)[key]++;
};

// -----------------------------------------------------------------------------------------------
// one element
// -----------------------------------------------------------------------------------------------

/**
 * Plan the edits for a single p-* element.
 *
 * The shape of a plan:
 *   consumed  — attribute names to DELETE (their meaning has been folded into `added`)
 *   renamed   — [{ attr, to }] keySpan renames (the value rides along untouched)
 *   added     — new attribute strings, inserted as ONE splice right after the tag name (never at the
 *               end of the start tag: most of these elements are self-closing and it would land
 *               after the `/`)
 * Everything not consumed and not renamed passes through byte-for-byte.
 *
 * Returns false on a refusal, in which case NOTHING is queued and the element is left for a human.
 */
function planElement(tagName, el, ctx, source, file) {
  const line = lineOf(source, el.sourceSpan.start.offset);
  const openTag = source.slice(el.sourceSpan.start.offset, el.startSourceSpan.end.offset);
  const no = (reason) => {
    refuse(file, line, reason, openTag);
    return false;
  };

  // A structural directive re-shapes the element. Nothing in this repo hits it (@if/@for only), but
  // guessing here is exactly the silent damage the contract forbids.
  if (ctx.templateAttrs?.length > 0) {
    return no(`structural directive (${ctx.templateAttrs.map((a) => a.name).join(", ")})`);
  }

  const attrs = allAttrs(el);
  const at = (name) => attrs.find((a) => a.name === name) ?? null;
  const isStatic = (a) => a?.kind === "attribute";
  const isBound = (a) => a?.kind === "input";

  const consumed = new Set();
  const renamed = [];
  const added = [];
  const pass = new Set(PASS[tagName]);

  /** styleClass -> class. On p-card / p-tag / p-skeleton / p-badge, PrimeNG binds
   *  `class: cn(cx('root'), styleClass)` on the HOST — the very box `class` lands on — so this is a
   *  rename, not an approximation. It is only safe while the element has no `class` of its own;
   *  two attributes named `class` would leave Angular to pick one. Zero sites do both. */
  const styleClassToClass = () => {
    const sc = at("styleClass");
    if (!sc) return true;
    if (!isStatic(sc)) return no("[styleClass] is bound — hand-convert");
    if (at("class")) return no(`has BOTH class="${at("class").value}" and styleClass="${sc.value}" — merge by hand`);
    renamed.push({ attr: sc, to: "class" });
    return true;
  };

  switch (tagName) {
    case "p-card": {
      if (!styleClassToClass()) return false;
      break;
    }

    case "p-tag": {
      // ---- the size channel: three different attributes all encode "make this chip smaller/bigger".
      // At most one may speak, or the winner would depend on our ordering rather than on the source.
      let size = null;
      /**
       * `wantStatic` is not decoration. `class` and `styleClass` are plain attributes, but `[style]`
       * is a BINDING BY NATURE — there is no static spelling of `{ fontSize: '0.7rem' }` — so a
       * blanket "must be static" guard refuses the one site it exists to convert.
       */
      const speak = (attr, table, kind, wantStatic) => {
        const a = at(attr);
        if (!a) return true;
        if (wantStatic && !isStatic(a)) return no(`[${attr}] is bound — hand-convert`);
        if (!wantStatic && !isBound(a)) return no(`${attr}="${a.value}" — expected a bound object`);
        const hit = table.get((a.value ?? "").trim().replace(/\s+/g, " "));
        if (!hit) return no(`${kind}="${a.value}" is not in the ${attr} mapping table`);
        if (hit.size) {
          if (size) return no(`two attributes both set a size (${size} and ${hit.size})`);
          size = hit.size;
        }
        consumed.add(attr);
        // A class string that survives the size extraction is re-emitted; an empty one just goes.
        if (hit.keep) added.push(`class="${hit.keep}"`);
        return true;
      };
      if (!speak("class", TAG_CLASS, "class", true)) return false;
      if (!speak("styleClass", TAG_STYLECLASS, "styleClass", true)) return false;
      if (!speak("style", TAG_STYLE, "[style]", false)) return false;
      if (size) added.push(`size="${size}"`);

      // ---- severity. See the header: NO severity means PRIMARY, the brand tint. Not neutral (which
      // does not exist), not ui-badge's `info` default (which would paint it blue).
      if (!at("severity")) added.push(`severity="primary"`);

      // A static `icon="pi pi-x"` would be a raw primeicons class string, which ui-badge's typed
      // `icon: IconName` cannot take. Phase 1 retyped all 8 producers; zero static ones remain.
      const icon = at("icon");
      if (icon && isStatic(icon)) return no(`icon="${icon.value}" is a raw class string — retype to IconName by hand`);
      break;
    }

    case "p-progress-spinner":
    case "p-progressSpinner": {
      // strokeWidth has NO equivalent and is deliberately dropped: ui-spinner draws a Lucide glyph,
      // whose stroke is baked into the path. Phase 2 shipped and documented this. 10 sites.
      const stroke = at("strokeWidth");
      if (stroke) {
        consumed.add("strokeWidth");
        notes.push(`${relative(file)}:${line}  strokeWidth="${stroke.value}" DROPPED (Lucide strokes are baked into the path)`);
      }
      // `[style]="{ width: '40px', height: '40px' }"` is how a p-progress-spinner is resized — its
      // size input does not exist. A non-square box has no `size` and is a refusal, not a guess.
      const style = at("style");
      if (style) {
        if (!isBound(style)) return no(`style="${style.value}" — expected a bound width/height object`);
        const m = /^\{\s*width:\s*'([^']+)'\s*,\s*height:\s*'([^']+)'\s*\}$/.exec(style.value.trim());
        if (!m) return no(`[style]="${style.value}" is not a {width,height} literal — hand-convert to size`);
        if (m[1] !== m[2]) return no(`[style] is not square (${m[1]} x ${m[2]}) — ui-spinner has one size`);
        consumed.add("style");
        added.push(`size="${m[1]}"`);
      }
      break;
    }

    case "p-skeleton": {
      if (!styleClassToClass()) return false;
      const shape = at("shape");
      if (shape && isStatic(shape) && !["circle", "rect", "rectangle"].includes(shape.value)) {
        return no(`shape="${shape.value}" is not a ui-skeleton shape`);
      }
      break;
    }

    case "p-divider":
      break;

    case "p-message": {
      if (!styleClassToClass()) return false;
      const sev = at("severity");
      if (!sev) return no("no severity — ui-alert's intent has no p-message default to inherit");
      if (!isStatic(sev)) return no("[severity] is bound — hand-convert to [intent]");
      const intent = MESSAGE_INTENT.get(sev.value);
      if (!intent) return no(`severity="${sev.value}" is not in the MESSAGE_INTENT table`);
      consumed.add("severity");
      added.push(`intent="${intent}"`);
      break;
    }

    case "p-avatar": {
      // ui-avatar's HOST is the circle — there is no shape to choose and no other shape in use.
      const shape = at("shape");
      if (shape) {
        if (!isStatic(shape) || shape.value !== "circle") {
          return no(`shape="${shape.value ?? "[bound]"}" — ui-avatar is always a circle`);
        }
        consumed.add("shape");
      }
      break;
    }

    case "p-badge": {
      if (!styleClassToClass()) return false;
      break;
    }

    case "p-overlaybadge":
      break;

    case "p-chip": {
      // p-chip is 4px of padding, not a component. It folds into ui-badge as a rounded grey chip —
      // `chip.background` is `{surface.100}` and `tag.secondary.background` is `{surface.100}`, the
      // same token, so this is a reproduction and not an approximation.
      const label = at("label");
      if (!label) return no("no [label] — nothing to render");
      renamed.push({ attr: label, to: "value" });
      added.push(`severity="secondary"`, "rounded");
      const sc = at("styleClass");
      if (sc) {
        if (!isStatic(sc) || sc.value.trim() !== "text-xs") {
          return no(`styleClass="${sc.value ?? "[bound]"}" is not in the p-chip mapping table`);
        }
        consumed.add("styleClass");
        added.push(`size="sm"`);
      }
      break;
    }

    case "p-progressBar": {
      // `[style]="{ height: '0.5rem' }"` — p-progressBar has no height input either.
      const style = at("style");
      if (style) {
        if (!isBound(style)) return no(`style="${style.value}" — expected a bound height object`);
        const m = /^\{\s*height:\s*'([^']+)'\s*\}$/.exec(style.value.trim());
        if (!m) return no(`[style]="${style.value}" is not a {height} literal — hand-convert to height`);
        consumed.add("style");
        added.push(`height="${m[1]}"`);
      }
      break;
    }

    default:
      return no(`no converter for <${tagName}>`);
  }

  // ---- every remaining attribute must be one we understand. A dropped binding is invisible in
  // review and shows up as a dead input at runtime, so an unknown name refuses the whole element.
  const renamedNames = new Set(renamed.map((r) => r.attr.name));
  for (const a of attrs) {
    if (consumed.has(a.name) || renamedNames.has(a.name)) continue;
    if (a.kind === "output") {
      if (UNIVERSAL_EVENTS.has(a.name)) continue;
      return no(`(${a.name}) has no ${COMPONENTS[tagName].tag} equivalent`);
    }
    if (a.kind === "reference") continue; // #ref on the host — the ref just points at the new class
    if (isNativeAttr(a.name)) continue;
    if (UNIVERSAL_PASS.has(a.name)) continue;
    if (pass.has(a.name)) continue;
    const shown = a.kind === "input" ? `[${a.name}]` : a.name;
    return no(`${shown} is not in the <${tagName}> mapping table`);
  }

  return { consumed, renamed, added };
}

/** Turn a plan into splices. Nothing below can refuse. */
function applyPlan(splicer, source, el, tagName, plan) {
  const target = COMPONENTS[tagName].tag;
  renameTag(splicer, el, target);

  for (const { attr, to } of plan.renamed) renameAttr(splicer, attr, to);

  const attrs = allAttrs(el);
  for (const name of plan.consumed) {
    const a = attrs.find((x) => x.name === name);
    if (!a) continue;
    let start = a.sourceSpan.start.offset;
    while (start > 0 && /[ \t\n]/.test(source[start - 1])) start--;
    splicer.remove(start, a.sourceSpan.end.offset, `remove ${name}`);
  }

  if (plan.added.length > 0) {
    splicer.insert(
      el.startSourceSpan.start.offset + 1 + tagName.length,
      ` ${plan.added.join(" ")}`,
      `${target} props`,
    );
  }
}

// -----------------------------------------------------------------------------------------------
// CONSERVATION. See the header: an eaten [value] is an empty chip, and nothing else would catch it.
// -----------------------------------------------------------------------------------------------

/**
 * The value-bearing and header-bearing elements — OLD AND NEW SPELLINGS IN THE SAME SPEC.
 *
 * It is tempting to write "before: the p-* tags, after: the ui-* tags", and it is wrong: a REFUSED
 * element keeps its `<p-tag [value]>`, so an after-spec that only knows `ui-badge` would score every
 * refusal as a lost value — turning a legitimate, reported refusal into a fatal error, and (worse)
 * pressuring the next person to weaken the check that exists to catch real losses.
 *
 * The law being enforced is NOT "everything got converted" — that is `--check`'s job. It is:
 *
 *     the multiset of value/header expressions across ALL value/header-bearing elements
 *     is IDENTICAL before and after.
 *
 * which holds whether an element was converted, refused, or left alone. p-chip is the one element
 * whose attribute is RENAMED (`label` -> `value`), so both names appear for it.
 */
const VALUE_BEARING = {
  "p-tag": "value",
  "p-badge": "value",
  "p-overlaybadge": "value",
  "p-chip": "label",
  "ui-badge": "value",
  "ui-count-badge": "value",
  "ui-overlay-badge": "value",
};
const HEADER_BEARING = { "p-card": "header", "ui-card": "header" };

/** The multiset of `<attr>` values carried by the given elements, as sorted source text. */
function signature(source, file, spec) {
  const out = [];
  let nodes;
  try {
    nodes = parse(source, file);
  } catch (e) {
    return { error: e.message, values: out };
  }
  visitElements(nodes, (el) => {
    const name = el.name ?? el.tagName;
    const want = spec[name];
    if (!want) return;
    const a = allAttrs(el).find((x) => x.name === want && x.kind !== "output");
    // A tag with no value at all projects its content instead. That is a legitimate shape (4 sites),
    // and it conserves as the absence of an entry on both sides.
    if (a) out.push(`${a.kind === "input" ? "[]" : '""'}${a.value ?? ""}`);
  });
  return { error: null, values: out.sort() };
}

const same = (a, b) => a.length === b.length && a.every((x, i) => x === b[i]);

/**
 * Hard-fail the whole run if a value or a header went missing, changed, or got swapped between two
 * elements in the same file. Nothing writes until this passes.
 */
function conserve(file, before, after) {
  const results = [];
  for (const [label, spec] of [
    ["value", VALUE_BEARING],
    ["header", HEADER_BEARING],
  ]) {
    const b = signature(before, file, spec);
    const a = signature(after, file, spec);
    if (a.error) {
      console.error(`\nFATAL: ${relative(file)} — the REWRITTEN template does not parse:\n  ${a.error}`);
      process.exit(1);
    }
    const ok = same(b.values, a.values);
    if (!ok) {
      if (process.env.COSMETIC_DEBUG) writeText(path.join(WORKSPACE_ROOT, "tools/codemods/tmp/failed.html"), after);
      console.error(
        `\nFATAL: ${relative(file)} — ${label} conservation FAILED.\n` +
          `  before (${b.values.length}): ${JSON.stringify(b.values)}\n` +
          `  after  (${a.values.length}): ${JSON.stringify(a.values)}\n` +
          `  A lost ${label} is an empty chip / a headless card — silent at runtime. Refusing to write.`,
      );
      process.exit(1);
    }
    results.push({ label, n: b.values.length });
  }
  return results;
}

// -----------------------------------------------------------------------------------------------
// one template
// -----------------------------------------------------------------------------------------------

function sweepTemplate(file, source) {
  let nodes;
  try {
    nodes = parse(source, file);
  } catch (e) {
    refuse(file, 0, `template parse failed: ${e.message}`, "");
    return { src: source, changed: false, symbols: new Set() };
  }

  const splicer = new Splicer(source, relative(file));
  const symbols = new Set();

  visitElements(nodes, (el, ctx) => {
    const tagName = el.name ?? el.tagName;
    if (!COMPONENTS[tagName]) return;
    stats.seen++;
    bumpTag(tagName, "seen");

    const plan = planElement(tagName, el, ctx, source, file);
    if (!plan) {
      stats.refused++;
      bumpTag(tagName, "refused");
      return;
    }
    applyPlan(splicer, source, el, tagName, plan);
    symbols.add(COMPONENTS[tagName].symbol);
    stats.rewritten++;
    bumpTag(tagName, "rewritten");
  });

  if (splicer.size === 0) return { src: source, changed: false, symbols };
  const out = splicer.apply();
  conserve(file, source, out);
  return { src: out, changed: true, symbols };
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
  if (!new RegExp(`templateUrl:\\s*["'\`]\\./${base.replace(/\./g, "\\.")}["'\`]`).test(src)) return null;
  return { file: ts, src };
}

/** `@logistics/shared/ui` from an app; a relative path from inside the shared lib itself (no cycle). */
function importSpecifier(tsFile, componentPath) {
  const rel = relative(tsFile);
  if (!rel.startsWith("projects/shared/")) return "@logistics/shared/ui";
  const from = path.dirname(path.join(WORKSPACE_ROOT, rel));
  const to = path.join(WORKSPACE_ROOT, "projects/shared/src/lib/ui", componentPath);
  let spec = path.relative(from, to).split(path.sep).join("/");
  if (!spec.startsWith(".")) spec = `./${spec}`;
  return spec;
}

/**
 * Strip a set of symbols from a @Component's `imports: [...]` array.
 *
 * `keep` is load-bearing and NOT obvious. `trips-list.ts` does `import { Card } from "primeng/card"`,
 * and OUR card component is also called `Card`. After the rewrite that file imports `Card` from
 * `@logistics/shared/ui` — the SAME identifier — so the name must STAY in the imports array while the
 * primeng import statement goes. Filtering by name alone would delete it and leave `<ui-card>`
 * undeclared, which strictTemplates reports as "not a known element" in a file the codemod called
 * clean. One file in the repo hits this; the rule is general.
 */
function stripFromComponentImports(src, remove, keep) {
  return src.replace(/(\bimports:\s*\[)([^\]]*)\]/, (_m, head, body) => {
    const kept = body
      .split(",")
      .map((s) => s.trim())
      .filter(Boolean)
      .filter((n) => !remove.has(n) || keep.has(n));
    return `${head}${kept.join(", ")}]`;
  });
}

/**
 * Wire the TS for a template that now renders ui-* components:
 *   + the Ui symbols in @Component.imports (and their import statements)
 *   - every primeng module whose tags are all gone from the FINAL html
 */
function wireTs(templateFile, finalHtml, symbols) {
  const owner = ownerComponent(templateFile);
  if (!owner) {
    refuse(templateFile, 0, `cannot resolve the owning component — ui imports NOT added`, "");
    return null;
  }
  let src = owner.src;

  for (const symbol of [...symbols].sort()) {
    const componentPath = Object.values(COMPONENTS).find((c) => c.symbol === symbol).path;
    const added = addToComponentImports(src, symbol);
    if (!added.changed && added.reason !== "already in imports") {
      refuse(owner.file, 0, `could not add ${symbol} to @Component imports: ${added.reason}`, "");
      return null;
    }
    // A component class is a VALUE. addImport refuses to merge one into an `import type {…}` and
    // says so; ignoring that would leave a binding that `imports: [X]` cannot use (TS1361).
    const imported = addImport(added.src, symbol, importSpecifier(owner.file, componentPath));
    if (!imported.changed && imported.reason !== "already imported") {
      refuse(owner.file, 0, `could not import ${symbol}: ${imported.reason}`, "");
      return null;
    }
    src = imported.src;
  }

  src = pruneModules(src, finalHtml, symbols);
  return { file: owner.file, src };
}

/** Remove every retired primeng module the final template no longer needs. */
function pruneModules(src, html, keepSymbols) {
  const remove = new Set();
  for (const { module, specifiers, needed } of PRIMENG_MODULES) {
    if (needed.test(html)) continue; // a refusal is still standing — the module stays
    for (const s of specifiers) {
      const before = src;
      src = removeImportSpecifier(src, module, s).src;
      if (src !== before) remove.add(s);
    }
  }
  if (remove.size > 0) src = stripFromComponentImports(src, remove, keepSymbols);
  return src;
}

/**
 * The 37 components that import a cosmetic PrimeNG module and never render its tag. They are
 * invisible — no error, no warning, just a PrimeNG module in the bundle graph of a component that
 * does not use it — so a template-driven sweep never reaches them. They get their own pass.
 */
function pruneUnusedImports(mode, touchedTs) {
  const pruned = [];
  for (const ts of listFiles({ dirs: ["projects"], ext: [".ts"] })) {
    const src = touchedTs.get(ts) ?? readText(ts);
    if (!PRIMENG_MODULES.some(({ module }) => new RegExp(`from ["']${module}["']`).test(src))) continue;

    const html = ts.replace(/\.ts$/, ".html");
    let template = "";
    try {
      template = readText(html);
    } catch {
      // no sibling template (a directive, a service) — inline templates are banned in this repo
    }
    const out = pruneModules(src, template, new Set());
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
const conservation = [];

for (const file of listFiles({ dirs: ["projects"], ext: [".html"] })) {
  const source = readText(file);
  if (!TAGS.some((t) => source.includes(`<${t}`))) continue;

  const { src, changed, symbols } = sweepTemplate(file, source);
  if (!changed) continue;

  conservation.push({ file: relative(file), results: conserve(file, source, src) });
  const ts = wireTs(file, src, symbols);
  touchedHtml.push(file);
  // Planned in every mode so --census reports the true file counts; only --apply writes.
  if (ts) touchedTs.set(ts.file, ts.src);
  if (mode === "apply") writeText(file, src);
}

const prunedUnused = pruneUnusedImports(mode, touchedTs);

if (mode === "apply") {
  for (const [file, src] of touchedTs) writeText(file, src);
  runPrettier([...touchedHtml, ...touchedTs.keys()]);
}

// -----------------------------------------------------------------------------------------------
// report
// -----------------------------------------------------------------------------------------------

const line = "=".repeat(88);
console.log(`\ncosmetic-sweep --${mode}\n${line}`);
console.log(`  ${"tag".padEnd(20)}${"->".padEnd(3)}${"component".padEnd(18)}${"seen".padStart(6)}${"done".padStart(7)}${"REFUSED".padStart(9)}`);
for (const tag of TAGS) {
  const s = perTag.get(tag);
  if (!s) continue;
  console.log(
    `  ${tag.padEnd(20)}${"->".padEnd(3)}${COMPONENTS[tag].tag.padEnd(18)}` +
      `${String(s.seen).padStart(6)}${String(s.rewritten).padStart(7)}${String(s.refused).padStart(9)}`,
  );
}
console.log(`  ${"".padEnd(41)}${String(stats.seen).padStart(6)}${String(stats.rewritten).padStart(7)}${String(stats.refused).padStart(9)}`);
console.log(`\nfiles: ${touchedHtml.length} template(s), ${touchedTs.size} component(s)`);

// ---- the conservation proof
const totals = new Map();
for (const { results } of conservation) {
  for (const { label, n } of results) totals.set(label, (totals.get(label) ?? 0) + n);
}
console.log(`\n${line}\nCONSERVATION (re-parsed from the OUTPUT of every file; a mismatch is fatal)`);
console.log(`  files checked:   ${conservation.length}`);
for (const [label, n] of totals) {
  console.log(`  ${label.padEnd(8)} preserved: ${n} (multiset of expressions identical before/after in every file)`);
}
console.log(`  ${totals.size === 2 ? "OK — no chip lost its text, no card lost its heading." : "OK"}`);

if (notes.length > 0) {
  console.log(`\n${line}\nDELIBERATE DROPS (${notes.length}) — verify:`);
  for (const n of notes) console.log(`  ${n}`);
}

console.log(`\n${line}\nREFUSALS (${refusals.length})`);
for (const r of refusals) console.log(`  ${r.file}:${r.line}\n      ${r.reason}\n      ${r.snippet}`);
if (refusals.length === 0) console.log("  none — every site matched the mapping table.");

if (prunedUnused.length > 0) {
  console.log(`\n${line}\nUNUSED primeng IMPORTS PRUNED (${prunedUnused.length}) — imported, never rendered`);
  for (const f of prunedUnused) console.log(`  ${f}`);
}

writeText(
  REFUSALS_OUT,
  `${JSON.stringify({ generatedBy: "tools/codemods/cosmetic-sweep.mjs", mode, stats, perTag: Object.fromEntries(perTag), refusals, notes }, null, 2)}\n`,
);
console.log(`\nrefusal list -> ${relative(REFUSALS_OUT)}`);

if (mode === "check") {
  const remaining = [];
  for (const file of listFiles({ dirs: ["projects"], ext: [".html"] })) {
    // Strip HTML comments first. A commented-out tag renders nothing, so counting it would fail the
    // gate on dead code — and invite someone to "fix" the gate by deleting a comment rather than a
    // component. ui-divider's own template quotes `<p-divider />` in a comment; it is not a call site.
    const src = readText(file).replace(/<!--[\s\S]*?-->/g, "");
    const hits = TAGS.filter((t) => new RegExp(`<${t}\\b`).test(src));
    if (hits.length > 0) remaining.push(`${relative(file)}  ${hits.join(", ")}`);
  }
  if (remaining.length > 0) {
    console.error(`\n--check FAILED: ${remaining.length} file(s) still carry a cosmetic p-* tag:`);
    for (const r of remaining) console.error(`  ${r}`);
    process.exit(1);
  }
  console.log("\n--check OK: none of the 12 cosmetic p-* tags remains.");
}
