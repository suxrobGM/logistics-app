/**
 * phase7.mjs — THE EXIT GATE. PrimeNG is gone; this is what keeps it gone.
 *
 * Run: node tools/gates/phase7.mjs        (or: bun run gate:phase7 / tools/gates/phase7.sh)
 * Exit 0 = no PrimeNG. Exit 1 = it came back.
 *
 * =============================================================================================
 * WHY THIS IS NOT JUST `git grep primeng`
 * =============================================================================================
 * The obvious gate — a dozen raw `git grep` lines — CANNOT return zero on this repo, and the
 * reason is not that PrimeNG survives. Three separate problems, each of which silently breaks a
 * naive word-grep:
 *
 *   1. PROSE. ~75 files carry comments that NAME PrimeNG in order to explain what was removed and
 *      why ("`.p-card` was p-card's own HOST, so `.dashboard-panel .p-card` beat it on specificity
 *      — that is why this rule is NOT ported"). A word-grep counts those as surface and pressures
 *      the next person to delete the EXPLANATION rather than a component. That is exactly backwards:
 *      a metric that punishes documenting PrimeNG cannot be used to remove PrimeNG. So we strip
 *      comments before matching, and then PROVE the stripped-out hits were only ever comments
 *      (see `prose` in the output — it is printed, not hidden).
 *
 *      This does not weaken the gate: a `<p-button>` hidden inside an HTML comment is not rendered,
 *      so it is not surface.
 *
 *   2. BINARIES. The brief's `git grep -nE "\bpi[- ][a-z]"` matches
 *      `projects/website/public/demo-video.mp4` — raw video bytes happen to contain the byte
 *      sequence. A gate that reports a REGRESSION because of an .mp4 gets switched off within a
 *      week. We scan an extension ALLOWLIST (.ts/.html/.css/.scss), so binaries cannot match.
 *
 *   3. THE TOOLING ITSELF. `tools/codemods/*` and `tools/checks/burndown.mjs` must name PrimeNG in
 *      order to remove and measure it. `git grep -i primeng` will always hit them. Satisfying that
 *      literally means deleting the tooling that enforces the gate. So the source scan is scoped to
 *      `projects/` (the shipped app) plus the manifests. tools/ is deliberately OUT of scope, and
 *      this comment is the loud version of saying so.
 *
 * =============================================================================================
 * WHY THIS DOES NOT REUSE burndown.mjs (Lesson 7)
 * =============================================================================================
 * burndown.mjs measures the same 7 metrics and currently reports 0. It would have been one import.
 * It is deliberately NOT reused, because the most expensive bug of this migration was a checker
 * that shared a scanner with the thing it checked and therefore confirmed only its own blind spots:
 * `listFiles()` used `git ls-files` (tracked files only), so every file a step CREATED was invisible
 * to every gate until it was committed. check-icons passed green over a brand-new paginator that
 * asked for three unregistered glyphs, and EVERY SORT ARROW IN ALL FOUR PORTALS rendered blank.
 *
 * So this gate is an INDEPENDENT implementation of the same question:
 *   - its own file walk — a real fs walk from disk, NOT `git ls-files`. Untracked, un-added,
 *     never-committed files are seen BY CONSTRUCTION. There is no `git add -N` ritual to forget.
 *   - its own comment stripper — a character state machine, not burndown's regex.
 *
 * Two independent scanners agreeing on 0 is evidence. One scanner agreeing with itself is not.
 * Run BOTH (`check:all` runs burndown; CI runs this). If they ever disagree, one of them has a bug
 * and you have learned something.
 */

import fs from "node:fs";
import path from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

/** Angular workspace root — this file lives at tools/gates/. */
const ROOT = path.resolve(fileURLToPath(new URL(".", import.meta.url)), "../..");
const REPO_ROOT = path.resolve(ROOT, "../../..");

const SCAN_EXT = [".ts", ".html", ".css", ".scss"];
const SKIP_DIRS = new Set(["node_modules", "dist", ".angular", "coverage", ".git", "public"]);

/**
 * Walk `projects/` from DISK. Not `git ls-files` — see the header. An untracked file is still a
 * file that ships, and this is the gate that has to see it.
 *
 * `public/` is skipped: it holds binary assets (the .mp4 above) and never contains source.
 */
function walk(dir, out = []) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      if (!SKIP_DIRS.has(entry.name)) walk(full, out);
    } else if (SCAN_EXT.includes(path.extname(entry.name).toLowerCase())) {
      out.push(full);
    }
  }
  return out;
}

/**
 * Blank out comments, preserving newlines and byte offsets so line numbers survive.
 *
 * STRINGS ARE PRESERVED, NOT BLANKED. A string literal is code: `class="p-button"` inside a .ts
 * string is real surface and must still be caught. Only comments are prose.
 */
function stripCommentsMarkup(src, ext) {
  const blank = (m) => m.replace(/[^\n]/g, " ");
  if (ext === ".html") return src.replace(/<!--[\s\S]*?-->/g, blank);
  return src.replace(/\/\*[\s\S]*?\*\//g, blank); // .css / .scss
}

/**
 * .ts — a character state machine, deliberately NOT burndown's single-regex approach (see header:
 * two independent scanners agreeing is the whole point).
 *
 * Enters a string state on the OPENING quote and leaves on the next unescaped matching quote; the
 * opener cannot also close it, because we only test the closing condition on chars reached AFTER
 * the state was entered.
 */
function stripCommentsTs(src) {
  const out = [];
  let i = 0;
  let state = "code";
  while (i < src.length) {
    const c = src[i];
    const next = src[i + 1];

    if (state === "code") {
      if (c === "/" && next === "/") {
        state = "line";
        out.push("  ");
        i += 2;
      } else if (c === "/" && next === "*") {
        state = "block";
        out.push("  ");
        i += 2;
      } else {
        if (c === "'") state = "sq";
        else if (c === '"') state = "dq";
        else if (c === "`") state = "tpl";
        out.push(c);
        i++;
      }
      continue;
    }

    if (state === "line") {
      if (c === "\n") {
        state = "code";
        out.push("\n");
      } else out.push(" ");
      i++;
      continue;
    }

    if (state === "block") {
      if (c === "*" && next === "/") {
        state = "code";
        out.push("  ");
        i += 2;
      } else {
        out.push(c === "\n" ? "\n" : " ");
        i++;
      }
      continue;
    }

    // sq | dq | tpl
    if (c === "\\") {
      out.push(c, next ?? "");
      i += 2;
      continue;
    }
    out.push(c);
    if (
      (state === "sq" && c === "'") ||
      (state === "dq" && c === '"') ||
      (state === "tpl" && c === "`")
    ) {
      state = "code";
    }
    i++;
  }
  return out.join("");
}

function strip(src, ext) {
  return ext === ".ts" ? stripCommentsTs(src) : stripCommentsMarkup(src, ext);
}

const read = (f) => fs.readFileSync(f, "utf8").replace(/\r\n/g, "\n");
const rel = (f) => path.relative(REPO_ROOT, f).split(path.sep).join("/");

/**
 * The gate lines. `re` must find ZERO matches in comment-stripped source.
 *
 * Every one of these is a line from the S14 brief. Where a line is adapted, the `note` says how and
 * why — nothing is weakened silently.
 */
const SOURCE_CHECKS = [
  {
    id: "primeng-imports",
    desc: 'import … from "primeng" | "@primeuix"',
    ext: [".ts"],
    re: /from\s*['"](primeng|@primeuix)/g,
  },
  {
    id: "primeicons",
    desc: "primeicons (package, CSS, or glyph class)",
    ext: [".ts", ".html", ".css", ".scss"],
    re: /primeicons/g,
    note: "scoped to projects/ — tools/codemods legitimately names primeicons in order to have removed it",
  },
  {
    id: "pi-icon-classes",
    desc: "pi / pi-* icon class tokens",
    ext: [".ts", ".html"],
    re: /\bpi[- ][a-z]/g,
    note: "extension allowlist, so demo-video.mp4 (which matches these bytes) cannot false-positive",
  },
  {
    id: "p-tags",
    desc: "<p-*> component markup",
    ext: [".html", ".ts"],
    re: /<p-[a-zA-Z]/g,
    note: "also scans .ts — burndown only scans .html, so a <p-table> in an INLINE template (template: `…`) would be invisible to it. The repo convention is templateUrl-only, but a gate must not depend on a convention it does not itself enforce.",
  },
  {
    id: "p-directives",
    desc: "pButton / pTooltip / pTemplate / pSortableColumn / pSelectableRow / pRowToggler / pInputText",
    ext: [".ts", ".html"],
    re: /\bp(Button|Tooltip|Template|SortableColumn|SelectableRow|RowToggler|InputText)\b/g,
  },
  {
    id: "p-css-selectors",
    desc: ".p-* CSS selectors",
    ext: [".css", ".scss"],
    re: /\.p-[a-z]/g,
  },
  {
    id: "ng-deep-primeng",
    desc: "::ng-deep reaching into a .p-* class",
    ext: [".css", ".scss"],
    re: /::ng-deep[^\n{]*\.p-[a-z]/g,
    note: "NOT `::ng-deep -> 0`: angular-gridster2 legitimately requires it (home.css). Only ::ng-deep AT a PrimeNG class is banned.",
  },
  {
    id: "primeng-api",
    desc: "MessageService / ConfirmationService / MenuItem",
    ext: [".ts", ".html"],
    re: /MessageService|ConfirmationService|\bMenuItem\b/g,
    note: "\\bMenuItem\\b does not match the repo's own UiMenuItem (no word boundary inside the identifier)",
  },
  {
    id: "value-accessor",
    desc: "ControlValueAccessor / NG_VALUE_ACCESSOR",
    ext: [".ts"],
    re: /ControlValueAccessor|NG_VALUE_ACCESSOR/g,
  },
  {
    id: "legacy-forms",
    desc: "ReactiveFormsModule / FormsModule / ngModel / formControlName / [formGroup] / NgForm / FormBuilder",
    ext: [".ts", ".html"],
    re: /ReactiveFormsModule|FormsModule|ngModel|formControlName|formGroupName|formArrayName|\[formGroup\]|\[formArray\]|ngForm|\(ngSubmit\)|\bNgForm\b|\bFormBuilder\b|new Form(Group|Control|Array)\(/g,
    note:
      "The brief's line was `ReactiveFormsModule|FormsModule|\\[\\(?ngModel`. That MISSES the entire " +
      "template surface: a BARE `<input ngModel>` (no brackets), `formControlName`, `[formGroup]`, " +
      "`#f=\"ngForm\"`, `(ngSubmit)`. It only held because every legacy forms directive in Angular 22 " +
      "is `standalone: false` ([ngModel]:not(...), [formControlName], [formGroup] — verified in " +
      "@angular/forms/fesm2022), so a template CANNOT use them without a `FormsModule` / " +
      "`ReactiveFormsModule` token appearing in the .ts — which the old regex did catch. That is a gate " +
      "leaning on a property of a DEPENDENCY it does not control: bump Angular, Angular makes NgModel " +
      "standalone, `imports: [NgModel]` compiles, and the gate goes silently blind. Widened so the HTML " +
      "side stands on its own. All existing hits are prose (see the prose column) — this cost nothing.",
  },
  {
    id: "tailwindcss-primeui",
    desc: "tailwindcss-primeui plugin",
    ext: [".ts", ".html", ".css", ".scss"],
    re: /tailwindcss-primeui/g,
  },
];

const BANNED_PKGS = ["primeng", "primeicons", "@primeuix/themes", "tailwindcss-primeui"];

/** Manifest assertions. These are the source of truth for what CI will actually install. */
function checkManifests() {
  const results = [];

  // THE load-bearing one. NOT the repo-root package.json: that is a bun workspace root and has
  // NEVER listed primeng, so a root-targeted assertion passes VACUOUSLY and always has. The
  // dependency lived here, in the Angular workspace package.json, so this is where we assert.
  const pkgPath = path.join(ROOT, "package.json");
  const pkg = JSON.parse(fs.readFileSync(pkgPath, "utf8"));
  const declared = { ...(pkg.dependencies ?? {}), ...(pkg.devDependencies ?? {}) };
  const hits = Object.keys(declared).filter((d) =>
    BANNED_PKGS.some((b) => d === b || d.startsWith("@primeuix/")),
  );
  results.push({
    id: "package.json (Angular workspace)",
    detail: rel(pkgPath),
    count: hits.length,
    hits: hits.map((h) => `${h}@${declared[h]}`),
  });

  // bun.lock is the transitive proof: package.json can be clean while the lockfile still pins
  // primeng for something else. It is also what `bun install --frozen-lockfile` in CI obeys.
  const lockPath = path.join(REPO_ROOT, "bun.lock");
  const lockLines = fs
    .readFileSync(lockPath, "utf8")
    .split("\n")
    .filter((l) => /"(primeng|primeicons|@primeuix\/|tailwindcss-primeui)/.test(l));
  results.push({
    id: "bun.lock",
    detail: "no primeng/@primeuix/primeicons entries (transitive proof)",
    count: lockLines.length,
    hits: lockLines.map((l) => l.trim().slice(0, 100)),
  });

  // angular.json: a dead `styles`/`assets` entry pointing at primeicons.css would break the build
  // only at build time, and only for the app that still references it.
  const ngPath = path.join(ROOT, "angular.json");
  const ngHits = fs
    .readFileSync(ngPath, "utf8")
    .split("\n")
    .map((l, i) => [i + 1, l])
    .filter(([, l]) => /primeng|primeicons|primeuix/i.test(l));
  results.push({
    id: "angular.json",
    detail: "no primeng/primeicons in styles, assets, or allowedCommonJsDependencies",
    count: ngHits.length,
    hits: ngHits.map(([n, l]) => `${n}: ${l.trim()}`),
  });

  return results;
}

export function run() {
  const files = walk(path.join(ROOT, "projects"));
  const byExt = new Map();
  for (const f of files) {
    const ext = path.extname(f).toLowerCase();
    if (!byExt.has(ext)) byExt.set(ext, []);
    byExt.get(ext).push(f);
  }

  console.log("\nPhase 7 exit gate — PrimeNG must not come back");
  console.log(
    `  scanning ${files.length} source files under projects/ (fs walk, not git ls-files)`,
  );
  console.log(
    `\n  ${"gate".padEnd(22)}${"code".padStart(6)}${"prose".padStart(7)}   result  (prose = named in a comment; exempt, see header)`,
  );
  console.log(`  ${"-".repeat(86)}`);

  let failed = 0;
  const failures = [];

  for (const check of SOURCE_CHECKS) {
    let code = 0;
    let raw = 0;
    const codeHits = [];

    for (const ext of check.ext) {
      for (const file of byExt.get(ext) ?? []) {
        const src = read(file);

        check.re.lastIndex = 0;
        raw += (src.match(check.re) ?? []).length;

        const stripped = strip(src, ext);
        check.re.lastIndex = 0;
        const found = stripped.match(check.re) ?? [];
        if (found.length > 0) {
          code += found.length;
          stripped.split("\n").forEach((line, n) => {
            check.re.lastIndex = 0;
            if (check.re.test(line)) codeHits.push(`${rel(file)}:${n + 1}: ${line.trim()}`);
          });
        }
      }
    }

    const prose = raw - code;
    const ok = code === 0;
    if (!ok) {
      failed++;
      failures.push({ check, codeHits });
    }
    console.log(
      `  ${check.id.padEnd(22)}${String(code).padStart(6)}${String(prose).padStart(7)}   ${ok ? "PASS" : "FAIL"}`,
    );
  }

  console.log(`\n  ${"manifest".padEnd(30)}${"hits".padStart(6)}   result`);
  console.log(`  ${"-".repeat(86)}`);
  for (const m of checkManifests()) {
    const ok = m.count === 0;
    if (!ok) {
      failed++;
      failures.push({ check: { id: m.id, desc: m.detail }, codeHits: m.hits });
    }
    console.log(`  ${m.id.padEnd(30)}${String(m.count).padStart(6)}   ${ok ? "PASS" : "FAIL"}`);
    console.log(`    ${m.detail}`);
  }

  if (failed > 0) {
    console.error(`\n  FAIL — PrimeNG is back in ${failed} place(s).\n`);
    for (const { check, codeHits } of failures) {
      console.error(`  ${check.id} — ${check.desc}`);
      for (const h of codeHits.slice(0, 15)) console.error(`      ${h}`);
      if (codeHits.length > 15) console.error(`      … and ${codeHits.length - 15} more`);
      console.error("");
    }
    console.error(
      "  These are matches in CODE, not in comments. Comments naming PrimeNG are exempt by design.\n",
    );
    return { ok: false };
  }

  console.log("\n  OK — PrimeNG is gone, and it is code-gone, not word-gone.\n");
  return { ok: true };
}

/**
 * SELF-TEST — the gate proves it can still SEE before it reports green.
 *
 * This exists because of the most expensive lesson of the migration: a checker that cannot see the
 * thing it checks confirms only its own blind spots. This gate's power comes from a comment
 * stripper, and a comment stripper is exactly the kind of component that can fail OPEN — over-strip
 * by one character and a live `<p-table>` becomes invisible, the gate goes green, and it stays green
 * forever.
 *
 * So: POSITIVE CONTROLS. Every case below is real PrimeNG surface that MUST still be caught after
 * stripping, plus the prose cases that must NOT be. If the stripper ever starts swallowing code,
 * CI fails here — on the gate itself — instead of silently passing the repo.
 *
 *   node tools/gates/phase7.mjs --self-test
 */
const SELF_TEST = [
  // [ext, source, gate id, expected matches AFTER stripping]
  [".ts", `import { ButtonModule } from "primeng/button";`, "primeng-imports", 1],
  [".ts", `// import { ButtonModule } from "primeng/button";`, "primeng-imports", 0],
  [".ts", `/**\n * import x from "primeng/api";\n */`, "primeng-imports", 0],
  // A string containing "//" must not fake a line comment and blank the rest of the line.
  [".ts", `const u = "https://x.dev"; import { X } from "primeng/api";`, "primeng-imports", 1],
  // Surface hidden in a string literal is STILL surface — strings are code, not prose.
  [".ts", `const cls = "pi pi-check";`, "pi-icon-classes", 1],
  [".ts", `const t = \`<p-button></p-button>\`;`, "p-directives", 0], // pButton token absent
  [".html", `<p-table [value]="x"></p-table>`, "p-tags", 1],
  [".html", `<!-- <p-table> used to live here -->`, "p-tags", 0],
  [".html", `<button pButton></button>`, "p-directives", 1],
  [".html", `<!-- pButton is gone -->`, "p-directives", 0],
  [".css", `.p-card { color: red; }`, "p-css-selectors", 1],
  [".css", `/* .p-card was p-card's own host */`, "p-css-selectors", 0],
  [".css", `::ng-deep .p-drawer-content { padding: 0; }`, "ng-deep-primeng", 1],
  [".css", `::ng-deep gridster { overflow: visible; }`, "ng-deep-primeng", 0],
  [".ts", `export class X implements ControlValueAccessor {}`, "value-accessor", 1],
  [".ts", `/** never a ControlValueAccessor */`, "value-accessor", 0],
  [".ts", `imports: [FormsModule]`, "legacy-forms", 1],
  // Every template spelling must be caught, INDEPENDENTLY of the .ts import — see the check's note.
  [".html", `<input [(ngModel)]="x" />`, "legacy-forms", 1],
  [".html", `<input [ngModel]="x" />`, "legacy-forms", 1],
  [".html", `<input ngModel name="x" />`, "legacy-forms", 1], // BARE attribute — the old regex missed this
  [".html", `<input formControlName="y" />`, "legacy-forms", 1],
  [".html", `<form [formGroup]="fg" (ngSubmit)="s()"></form>`, "legacy-forms", 2],
  [".html", `<form #f="ngForm"></form>`, "legacy-forms", 1],
  [".ts", `const fg = new FormGroup({ a: new FormControl("") });`, "legacy-forms", 2],
  [".html", `<!-- ngModel is banned; use [formField] -->`, "legacy-forms", 0],
  // Signal Forms' own API must NOT trip the widened regex — a gate that cries wolf gets switched off.
  [".html", `<form [formRoot]="form"><input [formField]="form.a" /></form>`, "legacy-forms", 0],
  [".ts", `import { form, FormField, FormRoot, submit } from "@angular/forms/signals";`, "legacy-forms", 0],
];

function selfTest() {
  let bad = 0;
  for (const [ext, src, id, expected] of SELF_TEST) {
    const check = SOURCE_CHECKS.find((c) => c.id === id);
    check.re.lastIndex = 0;
    const got = (strip(src, ext).match(check.re) ?? []).length;
    if (got !== expected) {
      bad++;
      console.error(
        `  SELF-TEST FAIL  ${id} on ${ext}: expected ${expected}, got ${got}\n      ${src.replace(/\n/g, "\\n")}`,
      );
    }
  }
  if (bad > 0) {
    console.error(
      `\n  ${bad} self-test(s) FAILED. The gate's comment stripper is broken — it may be blind to real\n` +
        `  PrimeNG surface. Do NOT trust a green run until this passes.\n`,
    );
    return false;
  }
  console.log(
    `  self-test: ${SELF_TEST.length}/${SELF_TEST.length} — the stripper still sees code.`,
  );
  return true;
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) {
  if (process.argv.includes("--self-test")) {
    process.exit(selfTest() ? 0 : 1);
  }
  // The self-test always runs first. A gate that has not proved it can see is not a gate.
  if (!selfTest()) process.exit(1);
  process.exit(run().ok ? 0 : 1);
}
