/**
 * check-all.mjs — the migration gate. Runs every guardrail, prints one summary, exits non-zero if
 * any fail. This is what CI runs and what the orchestrator runs between steps.
 *
 * ---------------------------------------------------------------------------------------------
 * REGISTRATION POINT — later steps add their checks to the CHECKS array below. Nothing else.
 *   S2/S3  check-icons.mjs    — no pi-* icon names, every ui-icon name is in the UiIconName union
 *   S4     check-buttons.mjs  — no <p-button>; every ui-button has an accessible name
 *   S14    check-a11y.mjs     — axe pass over the rendered routes
 * A check module must export `run(): { ok: boolean }` and must not need arguments.
 * ---------------------------------------------------------------------------------------------
 *
 *   node tools/checks/check-all.mjs      (or: bun run check:all)
 */

import { run as budgets } from "./budgets.mjs";
import { run as burndown } from "./burndown.mjs";
import { run as icons } from "./check-icons.mjs";
import { run as spartanTokens } from "./spartan-tokens.mjs";

const CHECKS = [
  { name: "burndown", run: burndown },
  { name: "budgets", run: budgets },
  { name: "spartan-tokens", run: spartanTokens },
  { name: "icons", run: icons },
  // <- register new checks here (see REGISTRATION POINT above)
];

const results = [];

for (const check of CHECKS) {
  try {
    const { ok } = check.run();
    results.push({ name: check.name, ok });
  } catch (error) {
    console.error(`\n  ${check.name} THREW: ${error.message}`);
    results.push({ name: check.name, ok: false });
  }
}

const failed = results.filter((r) => !r.ok);

console.log(`\n${"=".repeat(60)}`);
for (const r of results) console.log(`  ${r.ok ? "PASS" : "FAIL"}  ${r.name}`);
console.log("=".repeat(60));

if (failed.length > 0) {
  console.error(
    `\n${failed.length} of ${results.length} checks FAILED: ${failed.map((f) => f.name).join(", ")}\n`,
  );
  process.exit(1);
}

console.log(`\nAll ${results.length} checks passed.\n`);
