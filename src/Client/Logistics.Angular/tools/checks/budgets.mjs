/**
 * budgets.mjs — anti-tampering guard on the angular.json bundle budgets.
 *
 * THE RISK THIS EXISTS FOR: an agent whose step blows the bundle budget has two ways to go green —
 * fix the bundle, or edit one number in angular.json. The second is faster, produces a smaller diff,
 * looks like config housekeeping, and permanently destroys the signal that would have caught the
 * regression. It is a top-8 risk of this migration in its own right. So the budgets are not merely
 * enforced by the build; they are themselves under version-controlled ratchet.
 *
 * Rules:
 *   maximumError   — may not change AT ALL (raising hides a regression; lowering is out of scope for
 *                    a migration step and is almost always someone "tidying" a number they should not
 *                    be touching). Change it deliberately via --update, in its own reviewed commit.
 *   maximumWarning — may be LOWERED freely (stricter is always safe). RAISING requires --update.
 *   removal        — deleting a budget entry, or a project's whole budgets block, always fails.
 *                    This is the loophole an error-budget guard would otherwise leave wide open.
 *
 *   node tools/checks/budgets.mjs            # gate
 *   node tools/checks/budgets.mjs --update   # re-baseline (deliberate, reviewed)
 */

import fs from "node:fs";
import path from "node:path";
import { pathToFileURL } from "node:url";
import { WORKSPACE_ROOT } from "../codemods/lib/io.mjs";
import { readBaseline, writeBaseline } from "./baseline-io.mjs";

const ANGULAR_JSON = path.join(WORKSPACE_ROOT, "angular.json");

/** "5.5mb" | "500kB" | "1.5MB" | "4kb" -> bytes. Returns null if unparseable. */
export function toBytes(size) {
  const m = /^\s*([\d.]+)\s*(b|kb|mb|gb)\s*$/i.exec(String(size));
  if (!m) return null;
  const units = { b: 1, kb: 1024, mb: 1024 ** 2, gb: 1024 ** 3 };
  return Number(m[1]) * units[m[2].toLowerCase()];
}

/** Read every project's production budgets from angular.json, keyed by project then budget type. */
export function readBudgets() {
  const json = JSON.parse(fs.readFileSync(ANGULAR_JSON, "utf8"));
  const out = {};

  for (const [name, project] of Object.entries(json.projects ?? {})) {
    const budgets = project.architect?.build?.configurations?.production?.budgets;
    if (!budgets) continue;
    out[name] = {};
    for (const b of budgets) {
      out[name][b.type] = {
        ...(b.maximumWarning !== undefined && { maximumWarning: b.maximumWarning }),
        ...(b.maximumError !== undefined && { maximumError: b.maximumError }),
      };
    }
  }
  return out;
}

export function run({ update = false } = {}) {
  const current = readBudgets();
  const baseline = readBaseline();
  const previous = baseline.budgets ?? null;

  console.log("\nBundle budgets (anti-tampering)");

  if (update || !previous) {
    writeBaseline({ ...baseline, budgets: current });
    for (const [proj, types] of Object.entries(current)) {
      for (const [type, lim] of Object.entries(types)) {
        console.log(
          `  ${proj} / ${type}: warn ${lim.maximumWarning ?? "-"}, error ${lim.maximumError ?? "-"}`,
        );
      }
    }
    console.log(previous ? "\n  baseline updated." : "\n  baseline established (first run).");
    return { ok: true };
  }

  const failures = [];

  for (const [proj, types] of Object.entries(previous)) {
    if (!current[proj]) {
      failures.push(`${proj}: budgets block REMOVED from angular.json`);
      continue;
    }
    for (const [type, baseLim] of Object.entries(types)) {
      const curLim = current[proj][type];
      if (!curLim) {
        failures.push(`${proj} / ${type}: budget entry REMOVED`);
        continue;
      }

      // maximumError: any change at all is a failure.
      if (baseLim.maximumError !== curLim.maximumError) {
        failures.push(
          `${proj} / ${type}: maximumError CHANGED ${baseLim.maximumError} -> ${curLim.maximumError} ` +
            `(error budgets are not a knob a migration step may turn)`,
        );
      }

      // maximumWarning: lowering is fine, raising needs an explicit re-baseline.
      if (baseLim.maximumWarning !== curLim.maximumWarning) {
        const before = toBytes(baseLim.maximumWarning);
        const after = toBytes(curLim.maximumWarning);
        if (before === null || after === null) {
          failures.push(
            `${proj} / ${type}: maximumWarning unparseable (${baseLim.maximumWarning} -> ${curLim.maximumWarning})`,
          );
        } else if (after > before) {
          failures.push(
            `${proj} / ${type}: maximumWarning RAISED ${baseLim.maximumWarning} -> ${curLim.maximumWarning} ` +
              `without --update`,
          );
        } else {
          console.log(
            `  ${proj} / ${type}: maximumWarning tightened ${baseLim.maximumWarning} -> ${curLim.maximumWarning} (allowed)`,
          );
        }
      }
    }
  }

  // A brand-new project must be baselined rather than silently unguarded.
  for (const proj of Object.keys(current)) {
    if (!previous[proj])
      failures.push(`${proj}: new project with budgets, not in baseline — run --update`);
  }

  if (failures.length === 0) {
    console.log(`  OK — ${Object.keys(previous).length} projects, all error budgets intact.`);
    return { ok: true };
  }

  console.error("\n  FAIL — bundle budgets were tampered with:");
  for (const f of failures) console.error(`    - ${f}`);
  console.error(
    "\n  If this change is intended and reviewed: node tools/checks/budgets.mjs --update",
  );
  return { ok: false };
}

// pathToFileURL — see the note in spartan-tokens.mjs. String-concat comparison silently no-ops on Windows.
if (import.meta.url === pathToFileURL(process.argv[1]).href) {
  const result = run({ update: process.argv.includes("--update") });
  process.exit(result.ok ? 0 : 1);
}
