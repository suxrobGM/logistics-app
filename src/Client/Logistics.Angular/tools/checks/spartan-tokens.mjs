#!/usr/bin/env node
/**
 * CI gate: no bare `spartan-*` CSS classes in the vendored Helm primitives — they would render the
 * component unstyled, silently. `normalize-helm.mjs` strips them at generation time; this guards the
 * tree, so a hand-edit or a re-vendor that skipped the normalizer cannot smuggle one back in.
 *
 * The detector lives in ../lib/spartan-scan.mjs (shared with normalize-helm); the scan/report/CLI
 * shell lives in ../lib/check.mjs (shared with every other gate).
 *
 *   node tools/checks/spartan-tokens.mjs             # gate
 *   node tools/checks/spartan-tokens.mjs --self-test
 */
import { cli, defineCheck } from "../lib/check.mjs";
import { findBareTokens, SELF_TEST_CASES } from "../lib/spartan-scan.mjs";

export const check = defineCheck({
  title: "Spartan bare-class tokens",
  dirs: ["projects/shared/src/lib/ui/primitives"],
  ext: [".ts"],
  clean: "no bare spartan-* classes",
  violation: "bare spartan-* class(es)",
  hint: "Strip them (see normalize-helm.mjs):",
  find: findBareTokens,
  cases: SELF_TEST_CASES,
});

cli(import.meta.url, check);
