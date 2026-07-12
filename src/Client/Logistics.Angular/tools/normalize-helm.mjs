#!/usr/bin/env node
/**
 * normalize-helm.mjs — post-processor for `ng g @spartan-ng/cli:ui <primitive>`.
 *
 * The Spartan CLI stays the source of truth for Helm component code; this only fixes the monorepo
 * incompatibilities in its output so `ng build shared` (ng-packagr) succeeds and the layout matches
 * the hand-authored primitives.
 *
 * Passes:
 *   1. flattenSrc()               `<name>/src/**` -> `<name>/**` (the generator nests; we are flat).
 *
 *      ANTI-CLOBBER GUARD — the important part. The CLI recursively CO-GENERATES dependencies, so
 *      generating `pagination` also re-emits a STOCK `select`, which would overwrite our vendored one
 *      and silently drop its load-bearing `*hlmSelectPortal` directive (without which the overlay never
 *      opens). Therefore: any primitive dir ALREADY TRACKED IN GIT is "vendored" — if the generator
 *      emits a `src/` for it we DISCARD the regenerated copy and keep ours. `--force <name>` overrides,
 *      for a deliberate upstream re-pull. Without the flag we refuse, loudly.
 *
 *   2. rewriteSources()           ONE read/write per file, applying three independent rewrites, then
 *                                 gating on bare `spartan-*` classes:
 *                                   relativizeImports()  self-alias -> relative (ng-packagr rejects the alias)
 *                                   fixTypeOnlyImports() TS1484 under `verbatimModuleSyntax`
 *                                   stripGeneratorBugs() drop the CLI's bogus `[forceInvalid]` binding
 *                                 GATE — see lib/spartan-scan.mjs.
 *   3. stripInertCva()            the date-pickers ship dead CVA plumbing; this repo is Signal Forms.
 *   4. stripTsconfigPaths()       drop the paths the generator adds.
 *
 * Run it right after generating a primitive:
 *   bunx ng g @spartan-ng/cli:ui <primitive> --no-interactive
 *   bun run ui:normalize
 *   bunx prettier --write "projects/shared/src/lib/ui/primitives/**" tsconfig.json
 *
 * Idempotent: re-running on an already-normalized tree is a no-op and exits 0.
 */
import { execFileSync } from "node:child_process";
import { existsSync, mkdirSync, readdirSync, renameSync, rmSync, statSync } from "node:fs";
import { dirname, join, relative } from "node:path";
import {
  lineOf,
  listFiles,
  readText,
  relative as toWorkspacePath,
  WORKSPACE_ROOT,
  writeText,
} from "./lib/io.mjs";
import { findBareTokens } from "./lib/spartan-scan.mjs";

const ALIAS = "@logistics/shared/ui/primitives";
const PRIMS_DIR = "projects/shared/src/lib/ui/primitives";
const PRIMS_ABS = join(WORKSPACE_ROOT, PRIMS_DIR);
const TSCONFIG = "tsconfig.json";

/**
 * A safety floor only — the real guard is `trackedPrimitives()` ("is this dir tracked in git?"), which
 * protects every vendored primitive automatically. Listed here because their hand-modifications are the
 * ones that hurt most if silently regenerated: `utils` is hand-canonicalised, and `input` / `textarea`
 * had brain's `BrnInput` / `BrnFieldControlDescribedBy` host-directives stripped (they inject the
 * ambient `NgControl` and drive `aria-invalid` from ungated state — the pristine-invalid bug).
 */
const PRESERVE = new Set(["utils", "input", "textarea"]);

/** The 2 vendored date-picker components that ship an inert ControlValueAccessor. See stripInertCva(). */
const CVA_FILES = [
  "date-picker/lib/hlm-date-picker.ts",
  "date-picker/lib/hlm-date-range-picker.ts",
];

/** `--force select,button` or `--force=select,button` (repeatable). */
function parseForce(argv) {
  const force = new Set();
  for (let i = 0; i < argv.length; i++) {
    const arg = argv[i];
    let value = null;
    if (arg === "--force") value = argv[++i];
    else if (arg.startsWith("--force=")) value = arg.slice("--force=".length);
    if (!value) continue;
    for (const name of value.split(",")) {
      const trimmed = name.trim();
      if (trimmed) force.add(trimmed);
    }
  }
  return force;
}

const FORCE = parseForce(process.argv.slice(2));

/**
 * Primitive dirs that are already committed — i.e. vendored, hand-modified, ours.
 * `git ls-files` is the source of truth: a brand-new primitive (not yet committed) is NOT in here,
 * so its first generation lands normally; every subsequent co-generation of it is discarded.
 */
function trackedPrimitives() {
  const names = new Set();
  let out = "";
  try {
    out = execFileSync("git", ["ls-files", "--", PRIMS_DIR], { encoding: "utf8" });
  } catch {
    console.warn(
      "normalize-helm: WARNING — `git ls-files` failed; falling back to the PRESERVE floor only. " +
        "Vendored primitives beyond [" +
        [...PRESERVE].join(", ") +
        "] are NOT protected in this run.",
    );
    return names;
  }
  // Paths are cwd-relative, so key off the `ui/primitives/` marker instead of PRIMS_DIR.
  const marker = "ui/primitives/";
  for (const line of out.split("\n")) {
    const at = line.indexOf(marker);
    if (at === -1) continue;
    const name = line.slice(at + marker.length).split("/")[0];
    if (name) names.add(name);
  }
  return names;
}

/** Move a file or directory tree from `from` to `to`, overwriting existing files. */
function moveInto(from, to) {
  if (statSync(from).isDirectory()) {
    if (!existsSync(to)) mkdirSync(to, { recursive: true });
    for (const entry of readdirSync(from)) moveInto(join(from, entry), join(to, entry));
    rmSync(from, { recursive: true, force: true });
  } else {
    if (existsSync(to)) rmSync(to);
    renameSync(from, to);
  }
}

/**
 * Flatten `<name>/src/**` up into `<name>/**`, but DISCARD the regenerated `src/` of any primitive
 * we already vendor (see the anti-clobber note in the header). `--force <name>` overrides.
 */
function flattenSrc(vendored) {
  let flattened = 0;
  const preserved = [];
  const forced = [];
  for (const name of readdirSync(PRIMS_DIR)) {
    const src = join(PRIMS_DIR, name, "src");
    if (!existsSync(src) || !statSync(src).isDirectory()) continue;

    const isVendored = vendored.has(name) || PRESERVE.has(name);
    if (isVendored && !FORCE.has(name)) {
      // Hand-customised / already committed — drop the stock regenerated copy, keep ours.
      rmSync(src, { recursive: true, force: true });
      preserved.push(name);
      continue;
    }
    if (isVendored && FORCE.has(name)) forced.push(name);

    for (const entry of readdirSync(src)) {
      moveInto(join(src, entry), join(PRIMS_DIR, name, entry));
    }
    rmSync(src, { recursive: true, force: true });
    flattened++;
  }

  for (const name of preserved) {
    console.log(
      `normalize-helm: PRESERVED vendored '${name}' — discarded the co-generated stock copy. ` +
        `Pass --force ${name} to overwrite it deliberately.`,
    );
  }
  for (const name of forced) {
    console.log(
      `normalize-helm: --force ${name} — OVERWROTE the vendored '${name}' with the regenerated copy. ` +
        `Re-apply any local modifications before committing.`,
    );
  }
  return { flattened, preserved: preserved.length, forced: forced.length };
}

const ALIAS_IMPORT_RE = new RegExp(`(['"\`])${ALIAS}/([a-z0-9-]+)\\1`, "g");

/** Rewrite `@logistics/shared/ui/primitives/<name>` imports to a relative path (flat layout). */
function relativizeImports(src, file) {
  return src.replace(ALIAS_IMPORT_RE, (_m, q, name) => {
    let rel = relative(dirname(file), join(PRIMS_ABS, name)).replaceAll("\\", "/");
    if (!rel.startsWith(".")) rel = "./" + rel;
    return `${q}${rel}${q}`;
  });
}

/**
 * The generator sometimes imports type-only symbols as values, which `verbatimModuleSyntax`
 * rejects (TS1484). Promote the known offenders to `import type` / inline `type`.
 *
 * NOTE: this pass runs PRE-PRETTIER, on raw generator output. The patterns are therefore
 * quote-agnostic and whitespace-tolerant rather than assuming Prettier's double-quoted,
 * single-space formatting. Same for `stripGeneratorBugs()`.
 */
const COERCION_TYPES = /^(?:BooleanInput|NumberInput|StringInput)$/;

function fixTypeOnlyImports(src) {
  return (
    src
      // `import { BooleanInput[, NumberInput] } from "@angular/cdk/coercion";` → `import type { ... }`
      // Only when EVERY specifier is a coercion *type* (cdk/coercion also exports real functions).
      .replace(
        /import\s+\{([^}]*)\}\s+from\s+(["'])@angular\/cdk\/coercion\2;/g,
        (whole, specs, q) => {
          const names = specs
            .split(",")
            .map((s) => s.trim())
            .filter(Boolean);
          if (!names.length || !names.every((n) => COERCION_TYPES.test(n))) return whole;
          return `import type {${specs}} from ${q}@angular/cdk/coercion${q};`;
        },
      )
      .replace(
        /import\s+\{(\s*ClassValue\s*)\}\s+from\s+(["'])clsx\2;/g,
        (_m, specs, q) => `import type {${specs}} from ${q}clsx${q};`,
      )
      .replace(
        /import\s+\{\s*ButtonVariants\s*,\s*HlmButtonImports\s*\}/g,
        "import { type ButtonVariants, HlmButtonImports }",
      )
  );
}

/**
 * The `@spartan-ng/cli@1.1.0` date-picker inputs bind `[forceInvalid]="forceInvalid()"` on the
 * native `<input>`, but `forceInvalid` is a property of the enclosing `BrnDateInput` component, not
 * of the input element (a CLI/brain version mismatch → NG8002). The component already receives
 * `forceInvalid` as an input, so strip the redundant inner binding.
 *
 * Runs pre-prettier — quote-agnostic (Angular templates accept `'` or `"` around a binding value).
 */
function stripGeneratorBugs(src) {
  return src.replace(/^[ \t]*\[forceInvalid\]=(["'])forceInvalid\(\)\1[ \t]*\r?\n/gm, "");
}

/**
 * The three rewrites above, plus the gate, in ONE read/write per file. They used to be four passes,
 * each re-walking PRIMS_DIR and re-reading all ~124 files. They are independent, order-free rewrites
 * over the same source, so they compose.
 *
 * The gate reads the in-memory text rather than calling checks/spartan-tokens.mjs, which would re-read
 * the tree from disk. The DETECTOR is the thing that must not drift, and that is shared.
 */
function rewriteSources() {
  const changed = { imports: 0, typeImports: 0, bugs: 0 };
  const offenders = [];

  for (const file of listFiles({ dirs: [PRIMS_DIR], ext: [".ts"] })) {
    const src = readText(file);

    const relativized = relativizeImports(src, file);
    if (relativized !== src) changed.imports++;

    const typed = fixTypeOnlyImports(relativized);
    if (typed !== relativized) changed.typeImports++;

    const next = stripGeneratorBugs(typed);
    if (next !== typed) changed.bugs++;

    if (next !== src) writeText(file, next);

    for (const hit of findBareTokens(next)) {
      offenders.push({
        file: toWorkspacePath(file),
        line: lineOf(next, hit.offset),
        token: hit.token,
      });
    }
  }

  return { changed, offenders };
}

// ---------------------------------------------------------------------------------------------
// Pass 5 — strip the inert ControlValueAccessor from the vendored date pickers.
// ---------------------------------------------------------------------------------------------

/** Index of `src`'s matching `}` for the `{` at `open`. Bodies here contain no braces in strings. */
function matchBrace(src, open) {
  let depth = 0;
  for (let i = open; i < src.length; i++) {
    if (src[i] === "{") depth++;
    else if (src[i] === "}" && --depth === 0) return i;
  }
  return -1;
}

/** True if `this.<field>(...)` / `this.<field>?.(...)` is READ (called) anywhere — assignment doesn't count. */
function fieldIsRead(src, field) {
  return new RegExp(`this\\.${field}\\s*\\??\\.?\\s*\\(`).test(src);
}

/** True if `<name>(...)` is invoked as a method anywhere (`this.name(`, `picker.name(`). */
function methodIsCalled(src, name) {
  return new RegExp(`\\.${name}\\s*\\(`).test(src);
}

/** Remove a class method `name` (with its body) if present. Returns the new source. */
function removeMethod(src, name) {
  const re = new RegExp(
    `\\n[ \\t]*(?:public |protected |private )?${name}\\s*\\([^)]*\\)\\s*(?::[^{]+)?\\{`,
  );
  const m = re.exec(src);
  if (!m) return src;
  const open = m.index + m[0].length - 1;
  const close = matchBrace(src, open);
  if (close === -1) return src;
  return src.slice(0, m.index) + src.slice(close + 1);
}

/** Remove a single-line class field declaration `_name...;` if present. */
function removeField(src, name) {
  return src.replace(
    new RegExp(`\\n[ \\t]*(?:public |protected |private )?${name}\\??:[^\\n]*;`),
    "",
  );
}

/** Drop named import specifiers that are no longer referenced; drop the import if it empties. */
function pruneUnusedImports(src) {
  const IMPORT_RE = /import\s+(type\s+)?\{([\s\S]*?)\}\s*from\s*(["'])(.*?)\3;[ \t]*\r?\n?/g;
  // Usage check runs against the source with all import statements blanked out, so an identifier
  // that only ever appears inside an import block is provably unused.
  const body = src.replace(IMPORT_RE, "");
  return src.replace(IMPORT_RE, (whole, typeKw, specs, q, mod) => {
    const kept = specs
      .split(",")
      .map((s) => s.trim())
      .filter(Boolean)
      .filter((spec) => {
        // `type Foo`, `Foo as Bar`, `Foo` → the local binding is the last identifier.
        const local = spec
          .replace(/^type\s+/, "")
          .split(/\s+as\s+/)
          .pop()
          .trim();
        return new RegExp(`\\b${local}\\b`).test(body);
      });
    if (!kept.length) return "";
    if (
      kept.length ===
      specs
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean).length
    )
      return whole;
    return `import ${typeKw ?? ""}{ ${kept.join(", ")} } from ${q}${mod}${q};\n`;
  });
}

/**
 * Strip the INERT ControlValueAccessor plumbing from the 2 vendored date-picker components.
 *
 * WHY IT IS INERT (verified, not assumed):
 *   The ONLY consumer of `hlm-date-picker*` in this workspace is
 *   `ui-date-field` (projects/shared/src/lib/ui/form/date-field/). It drives the picker with plain
 *   `[date]` / `(dateChange)` bindings and additionally applies `uiDetachedControl`, which severs
 *   the ambient `NgControl`. No template anywhere binds `ngModel` / `formControlName` / `[formControl]`
 *   onto a picker. Angular therefore never resolves the `NG_VALUE_ACCESSOR` provider, and
 *   `writeValue` / `registerOnChange` / `registerOnTouched` / `setDisabledState` are never called.
 *   (Consumers bind `formControlName` on `ui-date-field` itself — that is the wrapper's own
 *   FormValueControl, a different mechanism.)
 *
 * WHY WE STRIP IT: the project is FormValueControl-only, zero CVA; the Phase 7 exit gate is a
 * literal `git grep ControlValueAccessor` → 0.
 *
 * WHAT WE DO **NOT** STRIP: `_onChange` / `_onTouched` are still READ from live code paths
 * (`updateDate()`, `reset()`, `_onStateChange()`, `touched()` — the latter two are part of brain's
 * `BrnDatePickerBase` contract). So the fields and the `registerOnX` methods that assign them stay;
 * they are simply never invoked. Only provably-dead members are removed. Correctness > aggression.
 *
 * Idempotent: every step is a no-op once applied.
 */
function stripInertCva() {
  const report = [];
  for (const rel of CVA_FILES) {
    const file = join(PRIMS_DIR, rel);
    if (!existsSync(file)) continue;
    const original = readText(file);
    let src = original;
    const removed = [];

    // 1. The `export const HLM_*_VALUE_ACCESSOR = { provide: NG_VALUE_ACCESSOR, ... };` block.
    const accessorConsts = [];
    const constRe = /(?:^|\n)(?:export\s+)?const\s+([A-Za-z0-9_]+)\s*=\s*\{/g;
    for (let m; (m = constRe.exec(src)); ) {
      const open = src.indexOf("{", m.index + m[0].length - 1);
      const close = matchBrace(src, open);
      if (close === -1) continue;
      if (!src.slice(open, close).includes("NG_VALUE_ACCESSOR")) continue;
      accessorConsts.push(m[1]);
      const start = m.index + (src[m.index] === "\n" ? 1 : 0);
      let end = close + 1;
      while (end < src.length && (src[end] === ";" || src[end] === "\n")) end++;
      src = src.slice(0, start) + src.slice(end);
      constRe.lastIndex = 0;
    }
    if (accessorConsts.length) removed.push(`const ${accessorConsts.join(", ")}`);

    // 2. Its entry in `providers: [...]` (and any inline `{ provide: NG_VALUE_ACCESSOR, ... }`).
    const before2 = src;
    for (const name of accessorConsts) {
      src = src.replace(new RegExp(`[ \\t]*${name}\\s*,?[ \\t]*\\r?\\n?`, "g"), "");
    }
    src = src.replace(/[ \t]*\{\s*provide:\s*NG_VALUE_ACCESSOR[\s\S]*?\}\s*,?[ \t]*\r?\n?/g, "");
    // Drop `providers: []` entirely if it is now empty.
    src = src.replace(/[ \t]*providers:\s*\[\s*\],?[ \t]*\r?\n?/g, "");
    if (src !== before2) removed.push("NG_VALUE_ACCESSOR provider entry");

    // 3. `ControlValueAccessor` from the `implements` clause.
    const before3 = src;
    src = src
      .replace(/(\bimplements\s+[^{]*?)\s*,\s*ControlValueAccessor\b/g, "$1")
      .replace(/\bimplements\s+ControlValueAccessor\s*,\s*/g, "implements ")
      .replace(/\s*\bimplements\s+ControlValueAccessor\b\s*(?=\{)/g, " ");
    if (src !== before3) removed.push("implements ControlValueAccessor");

    // 4. Provably-dead CVA methods (never called from this file, and NOT part of BrnDatePickerBase,
    //    which declares only popover/disabledState/formattedDate/hasDate/value?/updateDate?/touched?).
    for (const method of ["writeValue", "setDisabledState"]) {
      if (methodIsCalled(src, method)) continue;
      const before = src;
      src = removeMethod(src, method);
      if (src !== before) removed.push(`${method}()`);
    }

    // 5. `registerOnX` + `_onX` ONLY if the field is never read. In these 4 files `_onChange` /
    //    `_onTouched` ARE read from value-setters, so this correctly leaves them alone.
    for (const [field, register] of [
      ["_onChange", "registerOnChange"],
      ["_onTouched", "registerOnTouched"],
    ]) {
      if (fieldIsRead(src, field)) continue;
      const before = src;
      src = removeField(removeMethod(src, register), field);
      if (src !== before) removed.push(`${register}() + ${field}`);
    }

    // 6. The now-orphaned marker comment, the dead imports, and the blank lines they left behind.
    src = src.replace(/[ \t]*\/\*\*[ \t]*CONTROL VALUE ACCESSOR[ \t]*\*\/[ \t]*\r?\n/g, "");
    const beforeImports = src;
    src = pruneUnusedImports(src);
    if (src !== beforeImports) removed.push("dead imports");
    src = src.replace(/\n{3,}/g, "\n\n");

    if (src !== original) {
      writeText(file, src);
      report.push({ file: rel, removed });
    }
  }

  for (const { file, removed } of report) {
    console.log(`normalize-helm: stripped inert CVA from ${file} — removed ${removed.join("; ")}.`);
  }
  return report.length;
}

/**
 * GATE: a bare `spartan-*` CSS class token renders the component unstyled (we do not ship
 * `hlm-tailwind-preset.css`). Detector lives in lib/spartan-scan.mjs, shared with the CI check so the
 * two cannot drift apart.
 */
function assertNoBareSpartanClasses(offenders) {
  if (!offenders.length) return;

  console.error(
    `\nnormalize-helm: FAILED — ${offenders.length} bare \`spartan-*\` class token(s).\n` +
      `These class names are UNDEFINED in this repo, so the component renders UNSTYLED.\n` +
      `Replace each with the equivalent inlined Tailwind utilities:\n`,
  );
  for (const o of offenders) console.error(`  ${o.file}:${o.line}  ${o.token}`);
  console.error("");
  process.exit(1);
}

/** Drop every `@logistics/shared/ui/primitives/*` entry the generator added to tsconfig paths. */
function stripTsconfigPaths() {
  const raw = readText(TSCONFIG);
  const json = JSON.parse(raw);
  const paths = json?.compilerOptions?.paths;
  if (!paths) return 0;
  let removed = 0;
  for (const key of Object.keys(paths)) {
    if (key.startsWith(ALIAS + "/") || key === ALIAS) {
      delete paths[key];
      removed++;
    }
  }
  if (removed) writeText(TSCONFIG, JSON.stringify(json, null, 2) + "\n");
  return removed;
}

const vendored = trackedPrimitives();
const { flattened, preserved, forced } = flattenSrc(vendored);
const { changed, offenders } = rewriteSources();
const cvaStripped = stripInertCva();
const pathsRemoved = stripTsconfigPaths();
console.log(
  `normalize-helm: flattened ${flattened} primitive(s), preserved ${preserved} vendored, ` +
    `force-overwrote ${forced}, relativized ${changed.imports} file(s), ` +
    `fixed ${changed.typeImports} type-only import(s), stripped ${changed.bugs} generator bug(s), ` +
    `stripped CVA from ${cvaStripped} file(s), removed ${pathsRemoved} tsconfig path alias(es).`,
);
assertNoBareSpartanClasses(offenders);
