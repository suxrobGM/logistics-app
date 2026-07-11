/**
 * burndown.mjs — the migration ratchet.
 *
 * Counts every remaining trace of PrimeNG across projects/ and compares against baseline.json.
 * FAILS (exit 1) if ANY count went UP.
 *
 * Why a ratchet and not a target: we are removing PrimeNG across ~15 steps and many parallel
 * agents. The failure mode that actually kills a migration like this is not a step that removes too
 * little — it is a step that quietly ADDS a new `<p-button>` while removing three others, so the
 * net number improves and nobody notices the surface growing back. Monotonic-decrease is the only
 * property that composes across parallel work.
 *
 *   node tools/checks/burndown.mjs            # gate: fail if anything increased
 *   node tools/checks/burndown.mjs --update   # re-baseline (run after each step lands)
 */

import { pathToFileURL } from "node:url";
import { listFiles, readText, relative } from "../codemods/lib/io.mjs";
import { readBaseline, writeBaseline } from "./baseline-io.mjs";

const TS = [".ts"];
const HTML = [".html"];
const CSS = [".css", ".scss"];

/**
 * Each metric: which files it scans, its pattern, and whether we count matching LINES or raw
 * OCCURRENCES. `pTags` counts occurrences because one line can hold several tags.
 */
export const METRICS = {
  primengImports: { ext: [...TS], re: /from ['"](primeng|@primeuix)/g, mode: "lines" },
  piIcons: { ext: [...TS, ...HTML], re: /\bpi[- ][a-z]/g, mode: "occurrences" },
  pTags: { ext: HTML, re: /<p-[a-zA-Z]/g, mode: "occurrences" },
  pDirectives: {
    ext: [...TS, ...HTML],
    re: /\bp(Button|Tooltip|Template|SortableColumn|SelectableRow|RowToggler|InputText)\b/g,
    mode: "occurrences",
  },
  pCssSelectors: { ext: CSS, re: /\.p-[a-z]/g, mode: "occurrences" },
  cva: { ext: TS, re: /ControlValueAccessor|NG_VALUE_ACCESSOR/g, mode: "occurrences" },
  legacyForms: {
    ext: [...TS, ...HTML],
    re: /ReactiveFormsModule|FormsModule|\[\(?ngModel/g,
    mode: "occurrences",
  },
};

/** Count every metric. Returns { counts, worstFiles } — worstFiles powers the "where is it" hint. */
export function measure() {
  const counts = {};
  const perFile = {};

  for (const [name, spec] of Object.entries(METRICS)) {
    counts[name] = 0;
    perFile[name] = new Map();

    for (const file of listFiles({ dirs: ["projects"], ext: spec.ext })) {
      const src = readText(file);
      let n = 0;

      if (spec.mode === "lines") {
        for (const line of src.split("\n")) {
          spec.re.lastIndex = 0;
          if (spec.re.test(line)) n++;
        }
      } else {
        n = (src.match(spec.re) ?? []).length;
      }

      if (n > 0) {
        counts[name] += n;
        perFile[name].set(relative(file), n);
      }
    }
  }
  return { counts, perFile };
}

function pad(s, w) {
  return String(s).padEnd(w);
}
function padStart(s, w) {
  return String(s).padStart(w);
}

export function printTable(counts, baseline) {
  const names = Object.keys(METRICS);
  const w = Math.max(...names.map((n) => n.length)) + 2;

  console.log(
    `  ${pad("metric", w)}${padStart("current", 9)}${padStart("baseline", 10)}${padStart("delta", 8)}`,
  );
  console.log(`  ${"-".repeat(w + 27)}`);

  for (const name of names) {
    const cur = counts[name];
    const base = baseline?.[name];
    const delta = base === undefined ? null : cur - base;
    const arrow =
      delta === null ? "new" : delta > 0 ? `+${delta} FAIL` : delta < 0 ? `${delta}` : "0";
    console.log(
      `  ${pad(name, w)}${padStart(cur, 9)}${padStart(base ?? "-", 10)}${padStart(arrow, 8)}`,
    );
  }

  const total = names.reduce((a, n) => a + counts[n], 0);
  const baseTotal = baseline ? names.reduce((a, n) => a + (baseline[n] ?? 0), 0) : null;
  console.log(`  ${"-".repeat(w + 27)}`);
  console.log(`  ${pad("TOTAL", w)}${padStart(total, 9)}${padStart(baseTotal ?? "-", 10)}`);
}

export function run({ update = false } = {}) {
  const { counts, perFile } = measure();
  const baseline = readBaseline();
  const previous = baseline.burndown ?? null;

  console.log("\nPrimeNG burndown");
  printTable(counts, previous);

  if (update || !previous) {
    writeBaseline({ ...baseline, burndown: counts });
    console.log(previous ? "\n  baseline updated." : "\n  baseline established (first run).");
    return { ok: true, counts };
  }

  const regressions = Object.keys(METRICS).filter((n) => counts[n] > previous[n]);
  if (regressions.length === 0) {
    console.log("\n  OK — nothing increased.");
    return { ok: true, counts };
  }

  console.error("\n  FAIL — PrimeNG surface INCREASED. The migration only moves one direction.");
  for (const name of regressions) {
    console.error(
      `\n  ${name}: ${previous[name]} -> ${counts[name]} (+${counts[name] - previous[name]})`,
    );
    const top = [...perFile[name].entries()].sort((a, b) => b[1] - a[1]).slice(0, 5);
    for (const [file, n] of top) console.error(`      ${n.toString().padStart(4)}  ${file}`);
  }
  console.error(
    "\n  If this increase is genuinely intended, re-baseline with: node tools/checks/burndown.mjs --update",
  );
  return { ok: false, counts };
}

// pathToFileURL — see the note in spartan-tokens.mjs. String-concat comparison silently no-ops on Windows.
if (import.meta.url === pathToFileURL(process.argv[1]).href) {
  const result = run({ update: process.argv.includes("--update") });
  process.exit(result.ok ? 0 : 1);
}
