/**
 * button-sweep.mjs — S4 of the PrimeNG -> spartan migration: `<p-button>` -> `<ui-button>`.
 *
 *   node tools/codemods/button-sweep.mjs --census   # report only; writes .button-refusals.json
 *   node tools/codemods/button-sweep.mjs --apply    # rewrite templates + TS imports, then prettier
 *   node tools/codemods/button-sweep.mjs --check    # exit 1 if any <p-button / (onClick) remains
 *
 * See lib/html.mjs for the codemod contract (parse to analyse, span-splice to mutate, refuse rather
 * than guess, idempotent).
 *
 * =============================================================================================
 * THE #1 RISK IS A DEAD SAVE BUTTON, AND IT IS NOT THE ONE PEOPLE EXPECT.
 * =============================================================================================
 * `<p-button>` has NO `type` default — its template renders `[attr.type]="type || buttonProps?.type"`
 * so an untyped p-button inside a <form> IS a submit button, by HTML's own default. `ui-button`
 * defaults to `type="button"`. The tempting worry ("buttons will start submitting") is therefore
 * backwards: nothing gains submit behaviour, and 14 accidental submitters LOSE it (which is the fix).
 *
 * The real risk runs the other way. If this codemod drops one of the 37 explicit `type="submit"`
 * attributes, that Save button still renders, still highlights, still clicks — and silently does
 * nothing. No type error, no test failure, no console warning. So `conserveSubmit()` counts
 * `type="submit"` in the source and in the output of EVERY file and hard-fails on a mismatch. It is
 * cheap and it is the only mechanical proof that none were lost.
 *
 * =============================================================================================
 * WHY `styleClass` BECOMES `buttonClass` AND NOT `class`
 * =============================================================================================
 * PrimeNG puts styleClass on the INNER <button> (`[class]="cn(cx('root'), styleClass, ...)"`), not
 * on the <p-button> host. Mapping it to the host `class` would look right in review and silently
 * break layout at all 44 sites — the classes are things like `whitespace-nowrap` and `justify-start`
 * that only mean something on the button box. `ui-button.buttonClass` forwards to the inner button,
 * so it lands exactly where PrimeNG put it.
 *
 * The `w-full` case is the exception, and it is a real trap. On a p-button, `styleClass="w-full"`
 * sizes the inner <button> against the nearest BLOCK ancestor, because the <p-button> host is an
 * unstyled inline element and a percentage width skips it. `ui-button`'s host is `inline-flex`, which
 * IS a containing block — so a naive `buttonClass="w-full"` would resolve 100% against a shrink-to-fit
 * box and produce a button that is exactly as wide as its own text. That is why `ui-button` has
 * `block`, which sets `w-full` on the host AND the inner button. `w-full` in a styleClass therefore
 * becomes `block`, not `buttonClass`.
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

const ICON_MAP = JSON.parse(readText(path.join(WORKSPACE_ROOT, "tools/codemods/icon-map.json"))).icons;
const REFUSALS_OUT = path.join(WORKSPACE_ROOT, "tools/codemods/.button-refusals.json");
const BUTTON_COMPONENT = "projects/shared/src/lib/ui/action/button/button";

// -----------------------------------------------------------------------------------------------
// THE MAPPING TABLE. Anything not expressible here is a REFUSAL.
// -----------------------------------------------------------------------------------------------

/** severity -> intent. A missing severity means `primary`, which is ui-button's default. */
const SEVERITY_INTENT = new Map([
  ["primary", "primary"],
  ["secondary", "secondary"],
  ["success", "success"],
  ["info", "info"],
  ["warn", "warn"],
  ["danger", "danger"],
  ["help", "help"],
  ["contrast", "contrast"],
]);

/** PrimeNG's `size`. Its default (no attribute) is our `md`. */
const SIZE = new Map([
  ["small", "sm"],
  ["large", "lg"],
]);

/**
 * Legacy `p-button-*` severity classes hand-written into a `styleClass`. They are a class-string
 * back-door around the `severity` input and there are exactly two of them; both become a real
 * `intent`. Any OTHER `p-button-*` class in a styleClass is a refusal — it is styling a PrimeNG
 * internal that will not exist.
 */
const STYLECLASS_SEVERITY = new Map([["p-button-danger", "danger"]]);

/**
 * Attributes that ride from <p-button> to <ui-button> byte-for-byte.
 *
 *   routerLink / queryParams — RouterLink's selector matches the HOST, so it is already bound there
 *     and keeps working untouched. ui-button deliberately declares no input of that name: it would
 *     bind a SECOND time and navigate twice per click at all 90 sites.
 *   pTooltip / tooltipPosition — S6 owns the tooltip migration.
 *   pRowToggler — S12 owns the table.
 *   class / style / id / aria-* / data-* — native host attributes, host-to-host.
 */
const PASSTHROUGH = new Set([
  "label",
  "type",
  "disabled",
  "loading",
  "rounded",
  "ariaLabel",
  "routerLink",
  "queryParams",
  "pTooltip",
  "tooltipPosition",
  "pRowToggler",
  "class",
  "style",
  "id",
  "tabindex",
]);

const isNativeAttr = (name) => /^(aria-|data-)/.test(name);

/**
 * The accessible name for an icon-only button that has no tooltip to borrow one from.
 *
 * Every string here was written by a human who read the call site. The codemod will NOT synthesise a
 * name from a handler identifier — "openEditDialog" -> "Open edit dialog" reads plausible and is
 * exactly how a button ends up announcing something it does not do. An icon-only button that is not
 * in this table and has no tooltip is a REFUSAL.
 *
 * Keyed by `<file>#<icon>#<handler>` rather than file:line, so the key survives the reflow that this
 * very codemod causes (a line-number key would silently stop matching after the first run and the
 * codemod would stop being idempotent).
 */
const ARIA_OVERRIDES = new Map([
  ["projects/customer-portal/src/app/shared/layout/navbar/navbar.html#pi pi-user#menu.toggle($event)", "Open profile menu"],
  ["projects/tms-portal/src/app/pages/ai-dispatch/sessions-list/sessions-list.html#pi pi-eye#", "View session"],
  ["projects/tms-portal/src/app/pages/employees/components/driver-licenses-tab/driver-licenses-tab.html#pi pi-pencil#openEditDialog(row.license)", "Edit license"],
  ["projects/tms-portal/src/app/pages/employees/components/driver-licenses-tab/driver-licenses-tab.html#pi pi-trash#onDelete(row.license)", "Delete license"],
  ["projects/tms-portal/src/app/pages/loads/components/load-exceptions-content/load-exceptions-content.html#pi pi-refresh#refresh()", "Refresh exceptions"],
  ["projects/tms-portal/src/app/pages/messages/components/conversation-header/conversation-header.html#pi pi-arrow-left#", "Back to messages"],
  ["projects/tms-portal/src/app/pages/messages/conversation/conversation.html#pi pi-send#sendMessage()", "Send message"],
  ["projects/tms-portal/src/app/pages/safety/driver-behavior-list/driver-behavior-list.html#pi pi-eye#$event.stopPropagation(); openReviewDialog(event)", "Review event"],
]);

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

const stats = {
  seen: 0,
  rewritten: 0,
  refused: 0,
  iconResolved: 0,
  iconEnd: 0,
  onClick: 0,
  ariaFromTooltip: 0,
  ariaFromTable: 0,
  ariaFromNativeAttr: 0,
  styleClassToButtonClass: 0,
  styleClassToBlock: 0,
  styleClassToIntent: 0,
};
const intentCount = new Map();
const appearanceCount = new Map();
/** Untyped p-buttons inside a <form> — today they submit by accident; ui-button's default fixes them. */
const accidentalSubmits = [];
/** Every `class="..."` we passed through to the host, so a human can eyeball the inline -> inline-flex shift. */
const hostClasses = [];

const bump = (map, key) => map.set(key, (map.get(key) ?? 0) + 1);

/**
 * The source text of an attribute's value, for ANY binding kind.
 *
 * `allAttrs` reads `node.value?.source`, which is right for a BoundAttribute but is always null for a
 * BoundEvent — the compiler parks an event's expression on `node.handler`, not `node.value`. So
 * `(click)="onDelete(row)"` reports a null value, and an ARIA_OVERRIDES key built from it silently
 * becomes `file#icon#` and matches nothing. Read the bytes instead of trusting the field.
 */
function exprOf(source, attr) {
  if (attr.value != null) return attr.value;
  const span = attr.node?.handlerSpan ?? attr.valueSpan;
  return span ? source.slice(span.start.offset, span.end.offset) : "";
}

/** `pi pi-check` / `pi-check` -> `check`, but ONLY if the result is a canonical icon-map key. */
function canonicalIcon(raw) {
  const last = (raw ?? "").trim().split(/\s+/).pop() ?? "";
  const bare = last.startsWith("pi-") ? last.slice(3) : last;
  return bare in ICON_MAP ? bare : null;
}

// -----------------------------------------------------------------------------------------------
// one <p-button>
// -----------------------------------------------------------------------------------------------

/**
 * Plan (and queue) the edits for a single <p-button>. Returns true if it was rewritten, false if it
 * was refused — in which case NOTHING is queued for it and the element is left for a human.
 */
function convertButton(el, ctx, source, file, splicer, inForm) {
  const line = lineOf(source, el.sourceSpan.start.offset);
  const openTag = source.slice(el.sourceSpan.start.offset, el.startSourceSpan.end.offset);
  const no = (reason) => {
    refuse(file, line, reason, openTag);
    return false;
  };

  // ---- projected content. ui-button has no <ng-content>: `label`/`icon` are the only way in, and
  // accepting both would be two ways to say the same thing. 2 sites; hand-converted.
  const kids = (el.children ?? []).filter(
    (c) => !(c.constructor?.name === "Text" && !c.value?.trim()),
  );
  if (kids.length > 0) return no("projected content — ui-button has no <ng-content>; hand-convert");

  // A structural directive re-shapes the element; nothing in this repo hits it (@if/@for only), but
  // guessing here would be exactly the silent damage the contract forbids.
  if (ctx.templateAttrs?.length > 0) {
    return no(`structural directive (${ctx.templateAttrs.map((a) => a.name).join(", ")})`);
  }

  const attrs = allAttrs(el);
  const at = (name) => attrs.find((a) => a.name === name) ?? null;
  const isStatic = (a) => a?.kind === "attribute";
  const isBound = (a) => a?.kind === "input";

  // ---- appearance: severity + outlined/text/link/variant -> (intent, appearance)
  const severity = at("severity");
  if (isBound(severity)) return no(`[severity] is bound ("${severity.value}") — no dynamic intent; hand-convert`);

  let intent = "primary";
  if (isStatic(severity)) {
    const mapped = SEVERITY_INTENT.get(severity.value);
    if (!mapped) return no(`severity="${severity.value}" is not in the mapping table`);
    intent = mapped;
  }

  let appearance = "solid";
  for (const [flag, name] of [
    ["outlined", "outlined"],
    ["text", "text"],
    ["link", "link"],
  ]) {
    const a = at(flag);
    if (!a) continue;
    // `[outlined]="true"` and the valueless `outlined` both mean the same thing. `[outlined]="expr"`
    // does not: the appearance would have to change at runtime, which is an `[appearance]` binding a
    // human must write. 3 sites.
    if (isBound(a) && a.value?.trim() !== "true") {
      return no(`[${flag}]="${a.value}" is a dynamic appearance — bind [appearance] by hand`);
    }
    if (isStatic(a) && a.value !== "" && a.value !== "true") {
      return no(`${flag}="${a.value}" — expected a valueless flag or "true"`);
    }
    if (appearance !== "solid") return no(`two appearance flags (${appearance} + ${name})`);
    appearance = name;
  }

  // PrimeNG's alternative spelling. It coexists with [outlined]/[text] in this codebase.
  const variant = at("variant");
  if (variant) {
    if (!isStatic(variant) || !["text", "outlined"].includes(variant.value)) {
      return no(`variant="${variant.value ?? "[bound]"}" is not in the mapping table`);
    }
    if (appearance !== "solid" && appearance !== variant.value) {
      return no(`variant="${variant.value}" conflicts with the ${appearance} flag`);
    }
    appearance = variant.value;
  }

  // ---- icon
  const icon = at("icon");
  if (isBound(icon)) return no(`[icon]="${icon.value}" is a bound class string — hand-convert to IconName`);
  let iconName = null;
  if (icon) {
    iconName = canonicalIcon(icon.value);
    if (!iconName) return no(`icon="${icon.value}" does not resolve to a key in icon-map.json`);
  }

  const iconPos = at("iconPos");
  if (iconPos && (!isStatic(iconPos) || iconPos.value !== "right")) {
    return no(`iconPos="${iconPos.value ?? "[bound]"}" — only "right" is mapped (to iconEnd)`);
  }
  const iconIsTrailing = Boolean(iconPos);

  // ---- size
  const size = at("size");
  if (isBound(size)) return no(`[size]="${size.value}" is bound — hand-convert`);
  let sizeValue = null;
  if (size) {
    sizeValue = SIZE.get(size.value);
    if (!sizeValue) return no(`size="${size.value}" is not in the mapping table`);
  }

  // ---- styleClass -> buttonClass (inner <button>) / block / intent
  const styleClass = at("styleClass");
  if (isBound(styleClass)) return no("[styleClass] is bound — hand-convert to [buttonClass]");
  let buttonClass = null;
  let block = false;
  if (styleClass) {
    const tokens = (styleClass.value ?? "").split(/\s+/).filter(Boolean);
    const kept = [];
    for (const token of tokens) {
      if (token.startsWith("p-button")) {
        const mapped = STYLECLASS_SEVERITY.get(token);
        if (!mapped) return no(`styleClass contains "${token}" — a PrimeNG internal class with no ui-button equivalent`);
        if (isStatic(severity) && intent !== mapped) {
          return no(`styleClass "${token}" contradicts severity="${severity.value}"`);
        }
        intent = mapped;
        stats.styleClassToIntent++;
        continue;
      }
      // See the header: w-full on the INNER button alone would resolve against ui-button's
      // shrink-to-fit inline-flex host and produce a text-width button. `block` sizes both.
      if (token === "w-full") {
        block = true;
        stats.styleClassToBlock++;
        continue;
      }
      kept.push(token);
    }
    if (kept.length > 0) {
      buttonClass = kept.join(" ");
      stats.styleClassToButtonClass++;
    }
  }

  // ---- accessible name. An icon with no label renders an inert <svg> and nothing else: without an
  // ariaLabel the button announces as "button". A pTooltip is NOT an accessible name — it is not
  // wired to aria-labelledby, and it never reaches a screen reader that is not hovering a mouse.
  const label = at("label");
  const iconOnly = Boolean(icon) && !label;
  const ariaInput = at("ariaLabel");
  const nativeAriaAttr = attrs.find((a) => a.kind === "attribute" && a.name === "aria-label");
  // We rewrite `aria-label` INTO `ariaLabel`; if both are already present the rewrite would emit the
  // attribute twice and Angular would take whichever it saw last. Two names for one thing is a human
  // decision, not a codemod's.
  if (ariaInput && nativeAriaAttr) return no("has both aria-label and ariaLabel — pick one by hand");
  const existingAria = ariaInput ?? nativeAriaAttr;
  let ariaLabel = null; // { bound: boolean, value: string }

  if (iconOnly && !existingAria) {
    const tooltip = at("pTooltip");
    const clickAttr = attrs.find((a) => a.kind === "output" && (a.name === "click" || a.name === "onClick"));
    const handler = clickAttr ? exprOf(source, clickAttr) : "";
    const key = `${relative(file)}#${icon.value}#${handler.trim()}`;
    if (isStatic(tooltip) && tooltip.value?.trim()) {
      ariaLabel = { bound: false, value: tooltip.value };
      stats.ariaFromTooltip++;
    } else if (isBound(tooltip) && tooltip.value?.trim()) {
      // Same string the tooltip shows, evaluated the same way. Not an invention.
      ariaLabel = { bound: true, value: tooltip.value };
      stats.ariaFromTooltip++;
    } else if (ARIA_OVERRIDES.has(key)) {
      ariaLabel = { bound: false, value: ARIA_OVERRIDES.get(key) };
      stats.ariaFromTable++;
    } else {
      return no(`icon-only with no accessible name (no ariaLabel, no pTooltip, no ARIA_OVERRIDES entry for "${key}")`);
    }
  }

  // ---- every remaining attribute must be one we understand. A dropped binding is invisible in
  // review and shows up as a dead handler at runtime, so an unknown name refuses the whole element.
  const CONSUMED = new Set(["severity", "outlined", "text", "link", "variant", "icon", "iconPos", "size", "styleClass"]);
  for (const a of attrs) {
    if (CONSUMED.has(a.name)) continue;
    if (a.kind === "output") {
      if (a.name === "click" || a.name === "onClick") continue;
      return no(`(${a.name}) has no ui-button equivalent`);
    }
    if (a.name === "aria-label" || isNativeAttr(a.name)) continue;
    if (PASSTHROUGH.has(a.name)) continue;
    return no(`${a.kind === "input" ? `[${a.name}]` : a.name} is not in the mapping table`);
  }

  // =============================================================================================
  // Nothing below refuses. Queue the edits.
  // =============================================================================================
  renameTag(splicer, el, "ui-button");

  // (onClick) -> (click). PrimeNG's onClick emits `$event` straight from the inner button's native
  // click (`(click)="onClick.emit($event)"`), so the payload is a plain MouseEvent either way and
  // the rename is identity, not an approximation. (click) then bubbles from the inner <button> to
  // the ui-button host, where the call site's listener already is.
  const onClick = attrs.find((a) => a.kind === "output" && a.name === "onClick");
  if (onClick) {
    renameAttr(splicer, onClick, "click");
    stats.onClick++;
  }

  if (icon) {
    // Rewrite the value in place, and rename the key to `iconEnd` when iconPos said "right".
    splicer.replace(icon.valueSpan.start.offset, icon.valueSpan.end.offset, iconName, "icon name");
    stats.iconResolved++;
    // `iconPos` itself is deleted by the CONSUMED loop below — do not also delete it here, or two
    // edits claim the same span and the Splicer (correctly) refuses the whole file.
    if (iconIsTrailing) {
      renameAttr(splicer, icon, "iconEnd");
      stats.iconEnd++;
    }
  }

  // aria-label (a native attribute on the HOST, where it names nothing — the focusable element is
  // the inner <button>) -> ariaLabel (the input, which PrimeNG and ui-button both put on the inner
  // button as [attr.aria-label]). These sites are silently unlabelled today.
  if (nativeAriaAttr) {
    renameAttr(splicer, nativeAriaAttr, "ariaLabel");
    stats.ariaFromNativeAttr++;
  }

  // Drop the attributes we folded into (intent, appearance, size, buttonClass, block).
  for (const name of CONSUMED) {
    const a = at(name);
    if (!a) continue;
    if (a === icon) continue; // rewritten above, not removed
    let start = a.sourceSpan.start.offset;
    while (start > 0 && /[ \t\n]/.test(source[start - 1])) start--;
    splicer.remove(start, a.sourceSpan.end.offset, `remove ${name}`);
  }

  // ...and re-emit them in ui-button's vocabulary, as ONE insert right after the tag name (never at
  // the end of the start tag — 493 of these are self-closing and it would land after the `/`).
  const added = [];
  if (intent !== "primary") added.push(`intent="${intent}"`);
  if (appearance !== "solid") added.push(`appearance="${appearance}"`);
  if (sizeValue) added.push(`size="${sizeValue}"`);
  if (block) added.push("block");
  if (buttonClass) added.push(`buttonClass="${buttonClass}"`);
  if (icon) {
    /* the icon attribute itself stays in place */
  }
  if (ariaLabel) {
    added.push(ariaLabel.bound ? `[ariaLabel]="${ariaLabel.value}"` : `ariaLabel="${ariaLabel.value}"`);
  }
  if (added.length > 0) {
    splicer.insert(el.startSourceSpan.start.offset + 1 + "p-button".length, ` ${added.join(" ")}`, "ui-button props");
  }

  // ---- report-only bookkeeping
  bump(intentCount, intent);
  bump(appearanceCount, appearance);
  const cls = attrs.find((a) => a.kind === "attribute" && a.name === "class");
  if (cls) hostClasses.push(`${relative(file)}:${line}  class="${cls.value}"`);

  const type = at("type");
  if (inForm && !type) {
    accidentalSubmits.push(
      `${relative(file)}:${line}  ${label ? `label="${label.value}"` : `icon="${icon?.value}"`}` +
        `  -> was an implicit submit, now type="button"`,
    );
  }
  return true;
}

// -----------------------------------------------------------------------------------------------
// one template
// -----------------------------------------------------------------------------------------------

const SUBMIT_RE = /\btype="submit"/g;
const countSubmits = (src) => (src.match(SUBMIT_RE) ?? []).length;

/**
 * The conservation check. See the header: a dropped `type="submit"` is a Save button that renders,
 * clicks and does nothing, and NOTHING else in the toolchain would catch it.
 */
function conserveSubmit(file, before, after) {
  const b = countSubmits(before);
  const a = countSubmits(after);
  if (b !== a) {
    console.error(
      `\nFATAL: ${relative(file)} — type="submit" count changed ${b} -> ${a}.\n` +
        `A dropped submit is a dead save button. Refusing to write this file.`,
    );
    process.exit(1);
  }
  return b;
}

function sweepTemplate(file, source) {
  let nodes;
  try {
    nodes = parse(source, file);
  } catch (e) {
    refuse(file, 0, `template parse failed: ${e.message}`, "");
    return { src: source, changed: false, submits: 0 };
  }

  const splicer = new Splicer(source, relative(file));

  // <form> ancestry, so we can report which untyped buttons were accidental submitters. visitElements
  // does not hand us a parent chain, so we track it with our own walk over the same node shapes.
  const inFormOffsets = new Set();
  const markForms = (list, inForm) => {
    for (const n of list) {
      if (!n || typeof n !== "object") continue;
      const tag = n.name ?? n.tagName;
      if (inForm && n.sourceSpan) inFormOffsets.add(n.sourceSpan.start.offset);
      for (const [k, v] of Object.entries(n)) {
        if (["attributes", "inputs", "outputs", "references", "variables", "templateAttrs", "expression", "trackBy", "item", "contextVariables", "value", "handler", "i18n", "directives"].includes(k) || !v) continue;
        const next = inForm || tag === "form";
        if (Array.isArray(v)) {
          if (v.some((x) => x?.sourceSpan)) markForms(v, next);
        } else if (typeof v === "object" && v.sourceSpan) {
          markForms([v], next);
        }
      }
    }
  };
  markForms(nodes, false);

  visitElements(nodes, (el, ctx) => {
    if ((el.name ?? el.tagName) !== "p-button") return;
    stats.seen++;
    const inForm = inFormOffsets.has(el.sourceSpan.start.offset);
    if (convertButton(el, ctx, source, file, splicer, inForm)) stats.rewritten++;
    else stats.refused++;
  });

  if (splicer.size === 0) return { src: source, changed: false, submits: countSubmits(source) };
  const out = splicer.apply();
  const submits = conserveSubmit(file, source, out);
  return { src: out, changed: true, submits };
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
function buttonImportSpecifier(tsFile) {
  const rel = relative(tsFile);
  if (!rel.startsWith("projects/shared/")) return "@logistics/shared/ui";
  const from = path.dirname(path.join(WORKSPACE_ROOT, rel));
  const to = path.join(WORKSPACE_ROOT, BUTTON_COMPONENT);
  let spec = path.relative(from, to).split(path.sep).join("/");
  if (!spec.startsWith(".")) spec = `./${spec}`;
  return spec;
}

/**
 * Wire the TS for a template that now renders <ui-button>:
 *   + UiButton in @Component.imports (and its import statement)
 *   - Button / ButtonModule from 'primeng/button', but ONLY once nothing in the template needs them.
 *
 * `stillNeedsPrimeButton` is the load-bearing half. A file with one refused <p-button> left in it
 * still needs the PrimeNG import, and stripping it would turn a REFUSAL (visible, listed, waiting
 * for a human) into a build error — or worse, into a silently unrendered element.
 */
function wireTs(templateFile, finalHtml) {
  const owner = ownerComponent(templateFile);
  if (!owner) {
    refuse(templateFile, 0, "cannot resolve the owning component — UiButton import not added", "");
    return null;
  }
  let src = owner.src;

  const added = addToComponentImports(src, "UiButton");
  if (!added.changed && added.reason !== "already in imports") {
    refuse(owner.file, 0, `could not add UiButton to @Component imports: ${added.reason}`, "");
    return null;
  }
  src = addImport(added.src, "UiButton", buttonImportSpecifier(owner.file)).src;

  const stillNeedsPrimeButton = /<p-button|\bpButton\b/.test(finalHtml);
  if (!stillNeedsPrimeButton) {
    src = removeImportSpecifier(src, "primeng/button", "ButtonModule").src;
    src = removeImportSpecifier(src, "primeng/button", "Button").src;
    src = src.replace(/(\bimports:\s*\[)([^\]]*)\]/, (m, head, body) => {
      const kept = body
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean)
        .filter((n) => n !== "ButtonModule" && n !== "Button");
      return `${head}${kept.join(", ")}]`;
    });
  }
  return { file: owner.file, src };
}

/**
 * The ~15 components that import ButtonModule and never render a button. They are invisible: no
 * error, no warning, just a PrimeNG module in the bundle graph of a component that does not use it.
 * A template-driven sweep never reaches them, so they get their own pass.
 */
function pruneUnusedButtonImports(mode, touchedTs) {
  const pruned = [];
  for (const ts of listFiles({ dirs: ["projects"], ext: [".ts"] })) {
    const src = touchedTs.get(ts) ?? readText(ts);
    if (!/from ["']primeng\/button["']/.test(src)) continue;

    const html = ts.replace(/\.ts$/, ".html");
    let template = "";
    try {
      template = readText(html);
    } catch {
      // no sibling template (a directive, a service) — an inline template is not allowed in this repo
    }
    if (/<p-button|\bpButton\b/.test(template)) continue;

    let out = removeImportSpecifier(src, "primeng/button", "ButtonModule").src;
    out = removeImportSpecifier(out, "primeng/button", "Button").src;
    out = out.replace(/(\bimports:\s*\[)([^\]]*)\]/, (m, head, body) => {
      const kept = body
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean)
        .filter((n) => n !== "ButtonModule" && n !== "Button");
      return `${head}${kept.join(", ")}]`;
    });
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
const submitPerFile = [];

for (const file of listFiles({ dirs: ["projects"], ext: [".html"] })) {
  const source = readText(file);
  if (!source.includes("<p-button")) continue;

  const before = countSubmits(source);
  const { src, changed } = sweepTemplate(file, source);
  const after = countSubmits(src);
  submitPerFile.push({ file: relative(file), before, after, ok: before === after });

  if (!changed) continue;

  const ts = wireTs(file, src);
  touchedHtml.push(file);
  if (mode === "apply") {
    writeText(file, src);
    if (ts) touchedTs.set(ts.file, ts.src);
  }
}

const prunedUnused = pruneUnusedButtonImports(mode, touchedTs);

if (mode === "apply") {
  for (const [file, src] of touchedTs) writeText(file, src);
  runPrettier([...touchedHtml, ...touchedTs.keys()]);
}

// -----------------------------------------------------------------------------------------------
// report
// -----------------------------------------------------------------------------------------------

const line = "=".repeat(86);
console.log(`\nbutton-sweep --${mode}\n${line}`);
console.log(`<p-button> seen        ${String(stats.seen).padStart(4)}`);
console.log(`  rewritten            ${String(stats.rewritten).padStart(4)}`);
console.log(`  REFUSED              ${String(stats.refused).padStart(4)}`);
console.log(`\nfiles: ${touchedHtml.length} template(s), ${touchedTs.size || "?"} component(s)`);

console.log(`\nmappings applied`);
console.log(`  icon="pi pi-x" -> icon="x"        ${String(stats.iconResolved).padStart(4)}`);
console.log(`  iconPos="right" -> iconEnd        ${String(stats.iconEnd).padStart(4)}`);
console.log(`  (onClick) -> (click)              ${String(stats.onClick).padStart(4)}`);
console.log(`  styleClass -> buttonClass         ${String(stats.styleClassToButtonClass).padStart(4)}`);
console.log(`  styleClass w-full -> block        ${String(stats.styleClassToBlock).padStart(4)}`);
console.log(`  styleClass p-button-* -> intent   ${String(stats.styleClassToIntent).padStart(4)}`);
console.log(`  aria-label -> ariaLabel (host->btn)${String(stats.ariaFromNativeAttr).padStart(3)}`);
console.log(`  ariaLabel from pTooltip           ${String(stats.ariaFromTooltip).padStart(4)}`);
console.log(`  ariaLabel from ARIA_OVERRIDES     ${String(stats.ariaFromTable).padStart(4)}`);

console.log(`\nintent                              appearance`);
const intents = [...intentCount.entries()].sort((a, b) => b[1] - a[1]);
const appearances = [...appearanceCount.entries()].sort((a, b) => b[1] - a[1]);
for (let i = 0; i < Math.max(intents.length, appearances.length); i++) {
  const l = intents[i] ? `  ${intents[i][0].padEnd(12)}${String(intents[i][1]).padStart(4)}` : "  ".padEnd(18);
  const r = appearances[i] ? `  ${appearances[i][0].padEnd(12)}${String(appearances[i][1]).padStart(4)}` : "";
  console.log(`${l.padEnd(36)}${r}`);
}

// ---- the conservation proof
const totalBefore = submitPerFile.reduce((n, f) => n + f.before, 0);
const totalAfter = submitPerFile.reduce((n, f) => n + f.after, 0);
const broken = submitPerFile.filter((f) => !f.ok);
console.log(`\n${line}\ntype="submit" CONSERVATION (per file)`);
console.log(`  files carrying a submit: ${submitPerFile.filter((f) => f.before > 0).length}`);
console.log(`  total before: ${totalBefore}   total after: ${totalAfter}   ${broken.length === 0 ? "OK — every submit conserved" : "FAIL"}`);
for (const f of broken) console.log(`  FAIL ${f.file}: ${f.before} -> ${f.after}`);

console.log(`\n${line}\nNOW-FIXED ACCIDENTAL SUBMIT BUTTONS (${accidentalSubmits.length})`);
console.log(`  untyped <p-button> inside a <form> submits the form today (p-button has no type default).`);
console.log(`  ui-button defaults to type="button", so each of these STOPS submitting. Intended — verify:`);
for (const s of accidentalSubmits) console.log(`    ${s}`);

console.log(`\n${line}\nREFUSALS (${refusals.length}) — left as <p-button> for a human`);
for (const r of refusals) console.log(`  ${r.file}:${r.line}\n      ${r.reason}\n      ${r.snippet}`);

if (prunedUnused.length > 0) {
  console.log(`\n${line}\nUNUSED primeng/button IMPORTS PRUNED (${prunedUnused.length})`);
  for (const f of prunedUnused) console.log(`  ${f}`);
}

if (hostClasses.length > 0) {
  console.log(`\n${line}\nclass="..." PASSED THROUGH TO THE HOST (${hostClasses.length})`);
  console.log(`  <p-button>'s host is an unstyled inline element; <ui-button>'s is inline-flex. Width and`);
  console.log(`  height now APPLY to the host where they were previously ignored. Eyeball these:`);
  for (const c of hostClasses) console.log(`    ${c}`);
}

writeText(
  REFUSALS_OUT,
  `${JSON.stringify({ generatedBy: "tools/codemods/button-sweep.mjs", mode, stats, refusals, accidentalSubmits }, null, 2)}\n`,
);
console.log(`\nrefusal list -> ${relative(REFUSALS_OUT)}`);

if (mode === "check") {
  const remaining = [];
  for (const file of listFiles({ dirs: ["projects"], ext: [".html"] })) {
    // Strip HTML comments first. A commented-out tag renders nothing, so counting it would fail the
    // gate on dead code — and, worse, it invites someone to "fix" the gate by deleting a comment
    // rather than a component. (The same blindness bit the S4 component step, whose pCssSelectors
    // count went up because its own CSS comment mentioned a class name. Our tooling reads comments;
    // measure the template, not the prose about it.)
    const src = readText(file).replace(/<!--[\s\S]*?-->/g, "");
    const n = (src.match(/<p-button/g) ?? []).length;
    const c = (src.match(/\(onClick\)/g) ?? []).length;
    if (n + c > 0) remaining.push(`${relative(file)}  ${n} <p-button, ${c} (onClick)`);
  }
  if (remaining.length > 0) {
    console.error(`\n--check FAILED: ${remaining.length} file(s) still carry <p-button> or (onClick):`);
    for (const r of remaining) console.error(`  ${r}`);
    process.exit(1);
  }
  console.log("\n--check OK: no <p-button> and no (onClick) remains.");
}
