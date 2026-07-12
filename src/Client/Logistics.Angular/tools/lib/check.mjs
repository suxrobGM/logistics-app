/**
 * check.mjs — the shared harness behind every gate in tools/checks/.
 *
 * A gate is a SCOPE (dirs + extensions), a DETECTOR (`src` -> `[{ token, offset }]`) and a SELF-TEST
 * table. The rest — walking the tree, offsets to line numbers, the OK/FAIL report, `--self-test`, the
 * CLI guard — is the same for all of them and lives here. The first gate hand-rolled it and the second
 * copied it verbatim; a new gate is now a declaration.
 */

import { pathToFileURL } from "node:url";
import { lineOf, listFiles, readText, relative } from "./io.mjs";

/**
 * @param {object} spec
 * @param {string}   spec.title       banner line
 * @param {string[]} spec.dirs        workspace-relative dirs to scan
 * @param {string[]} spec.ext         extensions to keep, e.g. ['.html', '.ts']
 * @param {RegExp}  [spec.exempt]     paths to skip
 * @param {string}   spec.clean       what a green run proves, e.g. "no hardcoded palette colours"
 * @param {string}   spec.violation   what a red run counts, e.g. "hardcoded colour(s)"
 * @param {string}   spec.hint        printed once, on FAIL
 * @param {(src: string) => { token: string, offset: number }[]} spec.find  the detector
 * @param {[string, number, string][]} spec.cases  self-test: [source, expectedHits, label]
 * @returns {{ run: () => { ok: boolean }, selfTest: () => boolean }}
 */
export function defineCheck({ title, dirs, ext, exempt, clean, violation, hint, find, cases }) {
  function run() {
    console.log(`\n${title}`);

    const files = listFiles({ dirs, ext }).filter((f) => !exempt?.test(f));
    const violations = [];

    for (const file of files) {
      const src = readText(file);
      for (const hit of find(src)) {
        violations.push({ file: relative(file), line: lineOf(src, hit.offset), token: hit.token });
      }
    }

    if (violations.length === 0) {
      console.log(`  OK — ${files.length} files, ${clean}.`);
      return { ok: true };
    }

    console.error(`\n  FAIL — ${violations.length} ${violation}. ${hint}`);
    for (const v of violations) console.error(`    ${v.file}:${v.line}  ${v.token}`);
    return { ok: false };
  }

  /** A check that cannot fail is not a check: prove it fires, and prove it stays quiet. */
  function selfTest() {
    let failed = 0;
    for (const [src, expected, label] of cases) {
      const got = find(src).length;
      const pass = got === expected;
      if (!pass) failed++;
      console.log(`  ${pass ? "pass" : "FAIL"}  ${label} (expected ${expected}, got ${got})`);
    }
    console.log(failed === 0 ? "\n  self-test OK" : `\n  self-test FAILED (${failed})`);
    return failed === 0;
  }

  return { run, selfTest };
}

/**
 * Gate when the module is the entry point; stay quiet when the runner imports it.
 *
 * pathToFileURL, not string concat: on Windows `process.argv[1]` is `C:\...`, which never equals
 * `file://C:/...` — a hand-rolled comparison makes every gate a silent no-op that exits 0.
 */
export function cli(moduleUrl, check) {
  if (moduleUrl !== pathToFileURL(process.argv[1]).href) return;
  const ok = process.argv.includes("--self-test") ? check.selfTest() : check.run().ok;
  process.exit(ok ? 0 : 1);
}
