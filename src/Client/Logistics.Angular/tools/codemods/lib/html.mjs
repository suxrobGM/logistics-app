/**
 * html.mjs — Angular template analysis + surgical mutation.
 *
 * ===========================================================================================
 * THE CORE RULE: **PARSE for analysis, SPAN-SPLICE for mutation. NEVER re-print an AST.**
 * ===========================================================================================
 * `@angular/compiler` can parse a template into an AST, but it has no faithful printer. Rendering
 * the AST back to text reformats the entire file — attribute order, quoting, whitespace, control
 * flow blocks — and the resulting 400-line diff drowns the one line that actually changed. We
 * review these diffs to catch bad codemods, so a codemod that destroys diff reviewability has
 * disabled one of our detectors. Therefore: use the AST to *find* things and to read their
 * absolute source offsets, then edit the ORIGINAL STRING at those offsets via `Splicer`.
 *
 * Corollary: never use String#replace to mutate a template. A page with 20 identical
 * `<p-button icon="pi pi-plus" />` will have the naive replace hit the wrong one. Every mutation
 * in this file is expressed as a span.
 *
 * ===========================================================================================
 * THE CODEMOD CONTRACT — every codemod script in tools/codemods/ MUST implement it.
 * ===========================================================================================
 * Three modes, exactly one per invocation (see `parseMode` in io.mjs):
 *
 *   --census   Report what it WOULD do, and every REFUSAL. Change nothing. Exit 0.
 *              This is the mode you run FIRST, and whose refusal count gets signed off
 *              before anyone is allowed to run --apply.
 *
 *   --apply    Make the changes. Print a summary. Exit 0.
 *              Exit 1 if an unresolved refusal remains AND the codemod declares refusals fatal.
 *              MUST call runPrettier() on every .html it touched (lint-staged skips .html).
 *
 *   --check    Exit 1 if anything remains that --apply should have handled.
 *              This is the idempotency + completeness gate; it is what CI runs.
 *
 * Two invariants on top of the modes:
 *
 *   IDEMPOTENT — running --apply twice must make zero changes the second time. If --apply is not
 *                idempotent it cannot be re-run after a merge, and --check becomes meaningless.
 *
 *   REFUSE, NEVER GUESS — when a site does not match a shape the codemod fully understands, it
 *                must record a REFUSAL (file:line + reason) and leave the code untouched, so a
 *                human converts it. A codemod that silently "does its best" on an ambiguous site
 *                produces damage that is invisible in review and only shows up at runtime.
 *
 *   A codemod that reports ZERO refusals across a 500-site sweep is lying. Real templates contain
 *   conditional bindings, spread inputs, and hand-rolled wrappers. Expect refusals; sign off the
 *   expected count during --census; investigate any run whose refusal count comes in UNDER it.
 */

import { parseTemplate } from "@angular/compiler";
import { relative } from "./io.mjs";

// ---------------------------------------------------------------------------------------------
// Parsing
// ---------------------------------------------------------------------------------------------

/**
 * Parse an Angular template. Throws (naming the file) on any parse error — a codemod must never
 * operate on a template it could not fully understand.
 *
 * preserveWhitespaces + empty leadingTriviaChars keep every source offset faithful to the file on
 * disk. Without them the parser trims text nodes and every span we compute is silently shifted.
 */
export function parse(source, filename = "template.html") {
  const result = parseTemplate(source, filename, {
    preserveWhitespaces: true,
    leadingTriviaChars: [],
  });

  if (result.errors?.length) {
    const details = result.errors
      .map((e) => `  ${e.span?.start?.line ?? "?"}: ${e.msg}`)
      .join("\n");
    throw new Error(`Template parse failed: ${relative(filename)}\n${details}`);
  }
  return result.nodes;
}

// Node properties that hold expressions or attribute lists, not child nodes. The walker must not
// descend into them (attributes are surfaced through the element API instead).
const NON_CHILD_PROPS = new Set([
  "attributes",
  "inputs",
  "outputs",
  "references",
  "variables",
  "templateAttrs",
  "expression",
  "trackBy",
  "item",
  "contextVariables",
  "value",
  "handler",
  "i18n",
  "directives",
]);

/**
 * Walk every element in a template, calling fn(element, context) on each.
 *
 * Two gotchas this handles, both verified against @angular/compiler at the pinned version — both
 * of which silently corrupt a naive walker:
 *
 * 1. STRUCTURAL DIRECTIVE DOUBLE-VISIT.
 *    `<div *ngIf="a" class="x">` parses to a Template("div") wrapper whose child is an
 *    Element("div") with THE SAME sourceSpan and a COPY of the same attributes. A walker that
 *    fires on both visits the element twice, and the Splicer then sees two identical edits for one
 *    span and throws (or, worse, a rename gets applied twice).
 *    Rule: fire on every Element; fire on a Template only when it is a real `<ng-template>`.
 *    Verified against `<div *ngIf>`, `<ng-container *ngIf>`, `<ng-template *ngIf>`, `<p-button *ngIf>`.
 *    The wrapper's structural attrs are still reachable: see `ctx.templateAttrs`.
 *
 * 2. BLOCK CHILDREN LIVE UNDER DIFFERENT PROPERTY NAMES.
 *    @if -> `branches`, @for -> `children` + `empty`, @defer -> `placeholder`/`loading`/`error`,
 *    and @switch -> **`groups`**, NOT `cases`. Hardcoding a list of child properties silently skips
 *    every element inside a @switch. So we recurse generically into any property holding node-like
 *    values, minus NON_CHILD_PROPS. This repo is @if/@for-only, so blocks are the common case.
 */
export function visitElements(nodes, fn) {
  const walk = (list, wrapper) => {
    for (const node of list) {
      if (!node || typeof node !== "object") continue;
      const kind = node.constructor?.name;

      const isElement = kind === "Element";
      const isRealTemplate = kind === "Template" && node.tagName === "ng-template";
      // A Template that is NOT <ng-template> is a structural-directive wrapper (gotcha 1).
      const isWrapper = kind === "Template" && !isRealTemplate;

      if (isElement || isRealTemplate) {
        fn(node, { templateAttrs: wrapper?.templateAttrs ?? [], wrapper: wrapper ?? null });
      }

      // Recurse generically (gotcha 2). Pass the wrapper down one level so the wrapped element can
      // still see its own *ngIf/*ngFor.
      for (const [key, value] of Object.entries(node)) {
        if (NON_CHILD_PROPS.has(key) || !value) continue;
        const childWrapper = isWrapper ? node : null;
        if (Array.isArray(value)) {
          if (value.some((v) => v?.sourceSpan)) walk(value, childWrapper);
        } else if (value.sourceSpan && typeof value === "object") {
          walk([value], childWrapper);
        }
      }
    }
  };

  walk(nodes, null);
}

// ---------------------------------------------------------------------------------------------
// Attribute access — every accessor returns spans with .start.offset / .end.offset
// ---------------------------------------------------------------------------------------------

/**
 * Normalize one AST attribute into { kind, name, value, keySpan, valueSpan, sourceSpan }.
 *   sourceSpan — the whole attribute, e.g. `[disabled]="x()"` (what removeAttr deletes)
 *   keySpan    — just the name as written; for `[disabled]` that is `disabled`, WITHOUT brackets
 *   valueSpan  — inside the quotes; null for a valueless attribute like `rounded`
 */
function toAttr(node, kind) {
  return {
    kind,
    name: node.name,
    value: kind === "attribute" ? node.value : (node.value?.source ?? null),
    keySpan: node.keySpan ?? null,
    valueSpan: node.valueSpan ?? null,
    sourceSpan: node.sourceSpan,
    node,
  };
}

/** Static attributes: `label="Save"`, `rounded`. */
export const staticAttrs = (el) => (el.attributes ?? []).map((a) => toAttr(a, "attribute"));

/** Property bindings: `[disabled]="x()"`. NOTE: also includes the input half of a two-way binding. */
export const inputs = (el) => (el.inputs ?? []).map((a) => toAttr(a, "input"));

/** Event bindings: `(click)="f()"`. NOTE: also includes the `xChange` half of a two-way binding. */
export const outputs = (el) => (el.outputs ?? []).map((a) => toAttr(a, "output"));

/** Template references: `#ref`. */
export const references = (el) => (el.references ?? []).map((a) => toAttr(a, "reference"));

/**
 * Two-way bindings: `[(value)]="v"`.
 * The parser expands one `[(value)]` into an input `value` AND an output `valueChange` that share
 * an identical sourceSpan. Pairing them here means a codemod renaming `value` does not emit two
 * overlapping edits for the same text (the Splicer would throw). Returns one entry per banana box.
 */
export function twoWay(el) {
  const outs = outputs(el);
  const pairs = [];
  for (const input of inputs(el)) {
    const match = outs.find(
      (o) =>
        o.name === `${input.name}Change` &&
        o.sourceSpan.start.offset === input.sourceSpan.start.offset &&
        o.sourceSpan.end.offset === input.sourceSpan.end.offset,
    );
    if (match) pairs.push({ ...input, kind: "twoWay", output: match });
  }
  return pairs;
}

/** Every attribute on an element, with two-way pairs collapsed into a single entry. */
export function allAttrs(el) {
  const banana = twoWay(el);
  const isBananaSpan = (a) =>
    banana.some(
      (b) =>
        b.sourceSpan.start.offset === a.sourceSpan.start.offset &&
        b.sourceSpan.end.offset === a.sourceSpan.end.offset,
    );
  return [
    ...staticAttrs(el),
    ...inputs(el).filter((a) => !isBananaSpan(a)),
    ...outputs(el).filter((a) => !isBananaSpan(a)),
    ...banana,
    ...references(el),
  ].sort((a, b) => a.sourceSpan.start.offset - b.sourceSpan.start.offset);
}

/** Find one attribute by name across every binding kind. */
export const findAttr = (el, name) => allAttrs(el).find((a) => a.name === name) ?? null;

/** 1-based line number of an offset, for refusal messages. */
export const lineOf = (source, offset) => source.slice(0, offset).split("\n").length;

// ---------------------------------------------------------------------------------------------
// Mutation
// ---------------------------------------------------------------------------------------------

/**
 * Collects {start, end, replacement} edits and applies them BACK-TO-FRONT, so that an earlier edit
 * cannot invalidate the offsets of a later one. Asserts that no two edits overlap — an overlap
 * means two rules both claimed the same text and the result would depend on ordering, which is
 * exactly the class of silent corruption we refuse to ship.
 */
export class Splicer {
  #edits = [];

  constructor(source, file = "<memory>") {
    this.source = source;
    this.file = file;
  }

  /** Queue a replacement of [start, end) with `replacement`. */
  replace(start, end, replacement, label = "") {
    if (!Number.isInteger(start) || !Number.isInteger(end) || start > end) {
      throw new Error(`${this.file}: invalid span [${start}, ${end}) for "${label}"`);
    }
    this.#edits.push({ start, end, replacement, label });
    return this;
  }

  remove(start, end, label = "") {
    return this.replace(start, end, "", label);
  }

  insert(at, text, label = "") {
    return this.replace(at, at, text, label);
  }

  get size() {
    return this.#edits.length;
  }

  /** Apply every queued edit and return the new source. Throws on overlap. */
  apply() {
    const sorted = [...this.#edits].sort((a, b) => a.start - b.start || a.end - b.end);

    for (let i = 1; i < sorted.length; i++) {
      const prev = sorted[i - 1];
      const cur = sorted[i];
      // Touching spans are fine (prev.end === cur.start). Genuine overlap is not.
      // Two pure inserts at the same point are also fine.
      const overlaps = cur.start < prev.end;
      if (overlaps) {
        throw new Error(
          `${this.file}: overlapping edits — ` +
            `[${prev.start},${prev.end}) "${prev.label}" vs [${cur.start},${cur.end}) "${cur.label}". ` +
            `Two rules claimed the same text; refusing to guess which wins.`,
        );
      }
    }

    let out = this.source;
    for (let i = sorted.length - 1; i >= 0; i--) {
      const { start, end, replacement } = sorted[i];
      out = out.slice(0, start) + replacement + out.slice(end);
    }
    return out;
  }
}

/**
 * Rename an element's tag, both the open tag and the close tag.
 *
 * The AST gives no span for the tag NAME, only for the whole start/end tag, so we derive it:
 *   startSourceSpan `<p-button ...>` -> name begins 1 char in (after `<`)
 *   endSourceSpan   `</p-button>`    -> name begins 2 chars in (after `</`)
 * A self-closing or void element has endSourceSpan === startSourceSpan; we detect that by span
 * equality and skip the close-tag edit, otherwise we would corrupt `<p-button/>`.
 */
export function renameTag(splicer, el, newName) {
  const oldName = el.name ?? el.tagName;
  const start = el.startSourceSpan.start.offset + 1;
  splicer.replace(start, start + oldName.length, newName, `tag <${oldName}>`);

  const end = el.endSourceSpan;
  const selfClosing =
    !end ||
    (end.start.offset === el.startSourceSpan.start.offset &&
      end.end.offset === el.startSourceSpan.end.offset);

  if (!selfClosing) {
    const closeStart = end.start.offset + 2;
    splicer.replace(closeStart, closeStart + oldName.length, newName, `tag </${oldName}>`);
  }
  return splicer;
}

/**
 * Rename an attribute, preserving its binding syntax: `[x]` stays bracketed, `(x)` stays
 * parenthesized, `[(x)]` stays a banana box. We only ever touch the keySpan (the bare name), so the
 * brackets, the `=`, the quotes and the expression are all left byte-for-byte intact.
 */
export function renameAttr(splicer, attr, newName) {
  const key = attr.keySpan;
  if (!key) throw new Error(`Cannot rename attribute "${attr.name}": no keySpan`);
  splicer.replace(key.start.offset, key.end.offset, newName, `attr ${attr.name}`);
  return splicer;
}

/**
 * Remove an attribute entirely, including the whitespace that preceded it — otherwise every removal
 * leaves a double space and prettier has to clean up after us on files it may not touch.
 */
export function removeAttr(splicer, source, attr) {
  let start = attr.sourceSpan.start.offset;
  const end = attr.sourceSpan.end.offset;
  while (start > 0 && /[ \t]/.test(source[start - 1])) start--;
  splicer.remove(start, end, `remove ${attr.name}`);
  return splicer;
}

/**
 * Set an attribute: rewrite the value in place if it already exists, otherwise insert a new
 * attribute immediately after the tag name (never at the end of the start tag — a self-closing
 * `<p-button />` would put it after the `/`).
 */
export function setAttr(splicer, el, name, value) {
  const existing = findAttr(el, name);
  if (existing?.valueSpan) {
    splicer.replace(
      existing.valueSpan.start.offset,
      existing.valueSpan.end.offset,
      value,
      `set ${name}`,
    );
    return splicer;
  }
  if (existing) {
    // Valueless attribute (e.g. `rounded`) becoming a valued one.
    splicer.replace(
      existing.sourceSpan.start.offset,
      existing.sourceSpan.end.offset,
      `${name}="${value}"`,
      `set ${name}`,
    );
    return splicer;
  }
  const tagName = el.name ?? el.tagName;
  const at = el.startSourceSpan.start.offset + 1 + tagName.length;
  splicer.insert(at, ` ${name}="${value}"`, `add ${name}`);
  return splicer;
}
