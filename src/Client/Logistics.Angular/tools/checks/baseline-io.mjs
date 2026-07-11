/**
 * baseline-io.mjs — single reader/writer for tools/checks/baseline.json.
 *
 * burndown.mjs and budgets.mjs both persist state here, each under its own key, so that one check's
 * `--update` can never clobber the other's baseline.
 */

import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

export const BASELINE_FILE = path.join(
  fileURLToPath(new URL(".", import.meta.url)),
  "baseline.json",
);

export function readBaseline() {
  try {
    return JSON.parse(fs.readFileSync(BASELINE_FILE, "utf8"));
  } catch {
    return {};
  }
}

export function writeBaseline(data) {
  const ordered = {
    $comment:
      "Ratchet baseline for the PrimeNG -> spartan/ui migration. burndown: counts may only DECREASE. " +
      "budgets: angular.json maximumError values may not change at all. Update deliberately, per step, " +
      "via `node tools/checks/burndown.mjs --update` / `node tools/checks/budgets.mjs --update` — never by hand.",
    ...data,
  };
  fs.writeFileSync(BASELINE_FILE, `${JSON.stringify(ordered, null, 2)}\n`, "utf8");
}
