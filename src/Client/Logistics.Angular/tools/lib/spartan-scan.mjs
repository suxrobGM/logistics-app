/**
 * The ONE bare-`spartan-*`-class detector. Used by both `checks/spartan-tokens.mjs` (guards the tree,
 * in CI) and `normalize-helm.mjs` (guards generation). It used to be implemented twice, with different
 * boundary rules, so the two could disagree about the same file.
 *
 * WHY IT MATTERS: the Spartan CLI leaves a few tokens un-inlined (`spartan-invalid`,
 * `spartan-menu-target`, `spartan-logical-sides`). Those class names are UNDEFINED here — we
 * deliberately do not ship `@spartan-ng/brain/hlm-tailwind-preset.css` — so a component that emits one
 * renders UNSTYLED, silently.
 *
 * Three things look like hits but are NOT, and must never be flagged:
 *   1. `@spartan-ng/brain/...`                       — the real package import specifier
 *   2. `data-[matches-spartan-invalid=true]:...`     — a Tailwind arbitrary-variant on brain's invalid
 *                                                      state attribute. Load-bearing: removing it kills
 *                                                      every error style on every form control.
 *   3. `--spartan-*`                                 — CSS custom properties
 * A checker that fires on those gets disabled within a day, which is worse than no checker.
 */

/**
 * Extract every string / template literal, skipping comments (so a `spartan-foo` in a code comment is
 * not a violation). Returns [{ text, offset }] with absolute offsets.
 */
function stringLiterals(src) {
  const out = [];
  let i = 0;

  while (i < src.length) {
    const c = src[i];
    const next = src[i + 1];

    if (c === "/" && next === "/") {
      while (i < src.length && src[i] !== "\n") i++;
      continue;
    }
    if (c === "/" && next === "*") {
      i += 2;
      while (i < src.length && !(src[i] === "*" && src[i + 1] === "/")) i++;
      i += 2;
      continue;
    }
    if (c === "'" || c === '"' || c === "`") {
      const quote = c;
      const start = ++i;
      while (i < src.length) {
        if (src[i] === "\\") {
          i += 2;
          continue;
        }
        if (src[i] === quote) break;
        i++;
      }
      out.push({ text: src.slice(start, i), offset: start });
      i++;
      continue;
    }
    i++;
  }
  return out;
}

/** The three legitimate contexts (1/2/3 above), as ONE alternation — so scrubbing is a single pass. */
const LEGITIMATE = /@spartan-ng[\w/-]*|data-\[[^\]]*\]|--spartan-[a-z-]*/g;

/** A bare class: `spartan-x` not glued to a preceding word char, '-' or '@'. */
const BARE_TOKEN = /(?<![\w@-])spartan-[a-z-]+/g;

/** Blank out (preserving length, so offsets stay valid) every legitimate context, then flag the rest. */
export function findBareTokens(src) {
  const hits = [];

  for (const { text, offset } of stringLiterals(src)) {
    // Most literals are import specifiers and class strings that cannot hit. Skipping them here keeps
    // the scrub off the hot path.
    if (!text.includes("spartan")) continue;

    const scrubbed = text.replace(LEGITIMATE, (m) => " ".repeat(m.length));
    for (const m of scrubbed.matchAll(BARE_TOKEN)) {
      hits.push({ token: m[0], offset: offset + m.index });
    }
  }
  return hits;
}

/** A check that cannot fail is not a check: fires on a real bare class, silent on every legit shape. */
export const SELF_TEST_CASES = [
  ["const x = 'spartan-button flex';", 1, "bare class"],
  ["import { X } from '@spartan-ng/brain/button';", 0, "package specifier"],
  ["const c = 'data-[matches-spartan-invalid=true]:border-destructive';", 0, "arbitrary variant"],
  ["const c = 'dark:data-[matches-spartan-invalid=true]:ring-destructive/40';", 0, "dark: variant"],
  ["const c = 'var(--spartan-accent)';", 0, "custom property"],
  ["// spartan-legacy is fine in a comment", 0, "comment"],
  ["const c = 'flex spartan-input rounded';", 1, "bare class mid-string"],
];
