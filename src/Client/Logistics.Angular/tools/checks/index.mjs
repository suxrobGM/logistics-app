#!/usr/bin/env node
/**
 * Every gate in tools/checks/, in one process. What `bun run check` and CI call.
 *
 * DISCOVERED, not enumerated: any file here exporting `check` runs. The list used to live in three
 * places (a `check:*` script, the `check` chain, a CI step) — two lists too many, and a gate that
 * lands in one but not the workflow runs green on every machine except the one that matters.
 *
 * Runs all gates before exiting rather than bailing on the first red.
 *
 *   node tools/checks/index.mjs              # all gates
 *   node tools/checks/index.mjs --self-test  # every gate's self-test
 */
import { readdirSync } from "node:fs";
import path from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const dir = path.dirname(fileURLToPath(import.meta.url));
const gates = readdirSync(dir)
  .filter((f) => f.endsWith(".mjs") && f !== "index.mjs")
  .sort();

const selfTest = process.argv.includes("--self-test");
let ok = true;

for (const gate of gates) {
  const { check } = await import(pathToFileURL(path.join(dir, gate)).href);
  if (!check) {
    console.warn(`\n  SKIP — ${gate} exports no \`check\`.`);
    continue;
  }
  const passed = selfTest ? check.selfTest() : check.run().ok;
  ok = passed && ok;
}

console.log("");
process.exit(ok ? 0 : 1);
