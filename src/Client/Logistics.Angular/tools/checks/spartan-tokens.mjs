/**
 * spartan-tokens.mjs — no bare `spartan-*` CSS classes in the vendored Helm primitives.
 *
 * The Spartan CLI generates components whose host classes sometimes carry a bare marker class like
 * `spartan-button`. Those classes are dead in our build: nothing defines them, they collide with
 * nothing, and they quietly imply a styling contract we do not honour. `normalize-helm.mjs` strips
 * them at GENERATION time; this check guards CI, so a hand-edit or a re-vendor that skips the
 * normalizer cannot smuggle them back in. Deliberate duplication: one guards generation, one guards
 * the tree.
 *
 * Two things that look like hits but are NOT, and must never be flagged:
 *   1. `@spartan-ng/brain/...`            — the real package import specifier. Lives inside a string
 *                                            literal, so a naive string-literal scan flags all ~200.
 *   2. `data-[matches-spartan-invalid=true]:border-destructive`
 *                                          — a Tailwind arbitrary-variant selecting Brain's invalid
 *                                            state attribute. Load-bearing; removing it silently
 *                                            kills every error style on every form control.
 *   3. `--spartan-*`                       — CSS custom properties.
 * A checker that fires on those gets disabled within a day, which is worse than no checker.
 *
 *   node tools/checks/spartan-tokens.mjs           # gate
 *   node tools/checks/spartan-tokens.mjs --self-test
 */

import { pathToFileURL } from "node:url";
import { listFiles, readText, relative } from "../codemods/lib/io.mjs";

const PRIMITIVES = "projects/shared/src/lib/ui/primitives";

/**
 * Extract the contents of every string / template literal, skipping comments (so a `spartan-foo`
 * mentioned in a code comment is not a violation). Returns [{ text, offset }] with absolute offsets.
 */
export function stringLiterals(src) {
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

/**
 * Blank out (preserving length, so offsets stay valid) every legitimate `spartan-` context, then
 * look for what remains.
 */
export function findBareTokens(src) {
  const blank = (s, re) => s.replace(re, (m) => " ".repeat(m.length));
  const hits = [];

  for (const { text, offset } of stringLiterals(src)) {
    let scrubbed = text;
    scrubbed = blank(scrubbed, /@spartan-ng[\w/-]*/g); // package specifiers
    scrubbed = blank(scrubbed, /data-\[[^\]]*\]/g); // arbitrary-variant attribute selectors
    scrubbed = blank(scrubbed, /--spartan-[a-z-]*/g); // CSS custom properties

    // A bare class: `spartan-x` not glued to a preceding word char, '-' or '@'.
    for (const m of scrubbed.matchAll(/(?<![\w@-])spartan-[a-z-]+/g)) {
      hits.push({ token: m[0], offset: offset + m.index });
    }
  }
  return hits;
}

const lineOf = (src, offset) => src.slice(0, offset).split("\n").length;

export function run() {
  console.log("\nSpartan bare-class tokens");

  const files = listFiles({ dirs: [PRIMITIVES], ext: [".ts"] });
  const violations = [];

  for (const file of files) {
    const src = readText(file);
    for (const hit of findBareTokens(src)) {
      violations.push({ file: relative(file), line: lineOf(src, hit.offset), token: hit.token });
    }
  }

  if (violations.length === 0) {
    console.log(`  OK — ${files.length} primitive files, no bare spartan-* classes.`);
    return { ok: true };
  }

  console.error(
    `\n  FAIL — ${violations.length} bare spartan-* class(es). Strip them (see normalize-helm.mjs):`,
  );
  for (const v of violations) console.error(`    ${v.file}:${v.line}  ${v.token}`);
  return { ok: false };
}

/**
 * Self-test — a check that cannot fail is not a check.
 * Proves the detector fires on a real bare class AND stays silent on all three legitimate shapes.
 */
function selfTest() {
  const cases = [
    ["const x = 'spartan-button flex';", 1, "bare class"],
    ["import { X } from '@spartan-ng/brain/button';", 0, "package specifier"],
    ["const c = 'data-[matches-spartan-invalid=true]:border-destructive';", 0, "arbitrary variant"],
    [
      "const c = 'dark:data-[matches-spartan-invalid=true]:ring-destructive/40';",
      0,
      "dark: variant",
    ],
    ["const c = 'var(--spartan-accent)';", 0, "custom property"],
    ["// spartan-legacy is fine in a comment", 0, "comment"],
    ["const c = 'flex spartan-input rounded';", 1, "bare class mid-string"],
  ];

  let failed = 0;
  for (const [src, expected, label] of cases) {
    const got = findBareTokens(src).length;
    const pass = got === expected;
    if (!pass) failed++;
    console.log(`  ${pass ? "pass" : "FAIL"}  ${label} (expected ${expected}, got ${got})`);
  }
  console.log(failed === 0 ? "\n  self-test OK" : `\n  self-test FAILED (${failed})`);
  return failed === 0;
}

// NOTE: pathToFileURL, not string concat. On Windows process.argv[1] is `C:\...`, which never
// equals `file://C:/...` — a hand-rolled comparison makes the script a silent no-op that exits 0.
if (import.meta.url === pathToFileURL(process.argv[1]).href) {
  const ok = process.argv.includes("--self-test") ? selfTest() : run().ok;
  process.exit(ok ? 0 : 1);
}
