#!/usr/bin/env node
/**
 * normalize-helm.mjs — post-processor for `ng g @spartan-ng/cli:ui <primitive>`.
 * ============================================================================
 * This is NOT a reimplementation of the Spartan CLI (that was the old
 * `vendor-spartan-helm.mjs`, now deleted). The CLI remains the single source of
 * truth for the Helm component code. This script fixes the monorepo
 * incompatibilities in the generator's output so `ng build shared` (ng-packagr)
 * succeeds and the layout stays consistent with the hand-authored primitives.
 *
 * Passes, in order:
 *
 *   1. `flattenSrc()` — flatten `<name>/src/**` → `<name>/**`. The generator emits a
 *      `src/`-nested layout; our primitives are flat (`<name>/index.ts`,
 *      `<name>/lib/*.ts`). We keep one layout.
 *
 *      *** ANTI-CLOBBER GUARD (the important part) ***
 *      The CLI's `getDependentPrimitives` recursively CO-GENERATES dependencies:
 *        pagination → [utils, button, select]      dialog → [utils, button]
 *        tabs       → [utils, button]              sheet  → [utils, button]
 *        toggle-group → [utils, toggle]
 *        sidebar    → [utils, button, input, separator, sheet, skeleton, tooltip]
 *      So generating `pagination` re-emits a STOCK `select` — which would overwrite our
 *      vendored one and silently drop the load-bearing `*hlmSelectPortal` structural
 *      directive (without it the overlay never opens/closes — this bug already shipped
 *      once in this migration).
 *      Therefore: any primitive dir ALREADY TRACKED IN GIT is "vendored". If the
 *      generator emits a `src/` for it, we DISCARD the regenerated copy and keep ours.
 *      Escape hatch: `--force <name>[,<name>]` deliberately lets the regenerated copy
 *      win (for an intentional upstream re-pull). Without the flag we refuse, loudly.
 *
 *   2. `relativizeImports()` — rewrite `@logistics/shared/ui/primitives/<name>` self-alias
 *      imports to plain relative paths. ng-packagr rejects tsconfig path aliases that
 *      resolve outside the entry point; we consume the library from source.
 *
 *   3. `fixTypeOnlyImports()` — promote known type-only symbols to `import type` (TS1484
 *      under `verbatimModuleSyntax`).
 *
 *   4. `stripGeneratorBugs()` — drop the CLI's bogus `[forceInvalid]` inner binding.
 *
 *   5. `stripInertCva()` — strip the dead ControlValueAccessor plumbing from the 4
 *      date-picker components. See the function's docblock for the inertness proof.
 *
 *   6. `stripTsconfigPaths()` — drop the `@logistics/shared/ui/primitives/*` tsconfig
 *      paths the generator adds.
 *
 *   7. `assertNoBareSpartanClasses()` — GATE. The generator inlines the Tailwind
 *      utilities it harvests from `style-vega.css`, but leaves three tokens un-inlined:
 *      `spartan-invalid`, `spartan-menu-target`, `spartan-logical-sides`. Those class
 *      names are UNDEFINED here (we deliberately do not import
 *      `@spartan-ng/brain/hlm-tailwind-preset.css`), so a component emitting one renders
 *      UNSTYLED — silently. Any bare `spartan-*` CSS class token → print file:line and
 *      exit(1).
 *
 * Run it right after generating a primitive:
 *   bunx ng g @spartan-ng/cli:ui <primitive> --no-interactive
 *   bun run ui:normalize          # (== node tools/normalize-helm.mjs)
 *   bunx prettier --write "projects/shared/src/lib/ui/primitives/**" tsconfig.json
 *
 * Idempotent: re-running it on an already-normalized tree is a no-op and exits 0.
 */
import {
  readFileSync,
  writeFileSync,
  readdirSync,
  statSync,
  rmSync,
  renameSync,
  existsSync,
  mkdirSync,
} from "node:fs";
import { execFileSync } from "node:child_process";
import { join, relative, dirname } from "node:path";

const ALIAS = "@logistics/shared/ui/primitives";
const PRIMS_DIR = "projects/shared/src/lib/ui/primitives";
const TSCONFIG = "tsconfig.json";

/**
 * WHY primitives get preserved (documentation; the runtime guard is `trackedPrimitives()`).
 *
 * `utils` is hand-canonicalised (its `classes()` / `provideSpartanHlm()` are load-bearing).
 * `input` / `textarea` were deliberately stripped of brain's `BrnInput` /
 * `BrnFieldControlDescribedBy` host-directives, which inject the ambient `NgControl` and drive
 * `aria-invalid` from the raw, ungated control state — the pristine-invalid bug.
 * `select` carries the `*hlmSelectPortal` structural directive; `date-picker` has its CVA stripped.
 *
 * This list is now only a SAFETY FLOOR. The real guard is "is this dir tracked in git?", so every
 * vendored primitive is protected automatically as Phase 6 lands ~18 more.
 */
const PRESERVE = new Set(["utils", "input", "textarea"]);

/** The 2 vendored date-picker components that ship an inert ControlValueAccessor. See stripInertCva(). */
const CVA_FILES = ["date-picker/lib/hlm-date-picker.ts", "date-picker/lib/hlm-date-range-picker.ts"];

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

function walk(dir) {
  const out = [];
  for (const name of readdirSync(dir)) {
    const p = join(dir, name);
    if (statSync(p).isDirectory()) out.push(...walk(p));
    else if (p.endsWith(".ts")) out.push(p);
  }
  return out;
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

/** Rewrite `@logistics/shared/ui/primitives/<name>` imports to a relative path (flat layout). */
function relativizeImports() {
  let changed = 0;
  for (const file of walk(PRIMS_DIR)) {
    const src = readFileSync(file, "utf8");
    const next = src.replace(
      new RegExp(`(['"\`])${ALIAS}/([a-z0-9-]+)\\1`, "g"),
      (_m, q, name) => {
        const target = join(PRIMS_DIR, name);
        let rel = relative(dirname(file), target).replaceAll("\\", "/");
        if (!rel.startsWith(".")) rel = "./" + rel;
        return `${q}${rel}${q}`;
      },
    );
    if (next !== src) {
      writeFileSync(file, next);
      changed++;
    }
  }
  return changed;
}

/**
 * The generator sometimes imports type-only symbols as values, which `verbatimModuleSyntax`
 * rejects (TS1484). Promote the known offenders to `import type` / inline `type`.
 *
 * NOTE: this pass runs PRE-PRETTIER, on raw generator output. The patterns are therefore
 * quote-agnostic and whitespace-tolerant rather than assuming Prettier's double-quoted,
 * single-space formatting. Same for `stripGeneratorBugs()`.
 */
function fixTypeOnlyImports() {
  const COERCION_TYPES = /^(?:BooleanInput|NumberInput|StringInput)$/;
  let changed = 0;
  for (const file of walk(PRIMS_DIR)) {
    const src = readFileSync(file, "utf8");
    let next = src
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
      );
    if (next !== src) {
      writeFileSync(file, next);
      changed++;
    }
  }
  return changed;
}

/**
 * The `@spartan-ng/cli@1.1.0` date-picker inputs bind `[forceInvalid]="forceInvalid()"` on the
 * native `<input>`, but `forceInvalid` is a property of the enclosing `BrnDateInput` component, not
 * of the input element (a CLI/brain version mismatch → NG8002). The component already receives
 * `forceInvalid` as an input, so strip the redundant inner binding.
 *
 * Runs pre-prettier — quote-agnostic (Angular templates accept `'` or `"` around a binding value).
 */
function stripGeneratorBugs() {
  let changed = 0;
  for (const file of walk(PRIMS_DIR)) {
    const src = readFileSync(file, "utf8");
    const next = src.replace(/^[ \t]*\[forceInvalid\]=(["'])forceInvalid\(\)\1[ \t]*\r?\n/gm, "");
    if (next !== src) {
      writeFileSync(file, next);
      changed++;
    }
  }
  return changed;
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
  return src.replace(new RegExp(`\\n[ \\t]*(?:public |protected |private )?${name}\\??:[^\\n]*;`), "");
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
        const local = spec.replace(/^type\s+/, "").split(/\s+as\s+/).pop().trim();
        return new RegExp(`\\b${local}\\b`).test(body);
      });
    if (!kept.length) return "";
    if (kept.length === specs.split(",").map((s) => s.trim()).filter(Boolean).length) return whole;
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
    const original = readFileSync(file, "utf8");
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
      writeFileSync(file, src);
      report.push({ file: rel, removed });
    }
  }

  for (const { file, removed } of report) {
    console.log(`normalize-helm: stripped inert CVA from ${file} — removed ${removed.join("; ")}.`);
  }
  return report.length;
}

// ---------------------------------------------------------------------------------------------
// Pass 7 — gate on bare `spartan-*` class tokens.
// ---------------------------------------------------------------------------------------------

/**
 * Collect string / template literal spans, skipping comments (so the word "spartan" in a comment is
 * never flagged). Deliberately simple: it does not model `${}` interpolation or regex literals,
 * neither of which appears in Helm primitives.
 */
function stringLiterals(src) {
  const spans = [];
  for (let i = 0; i < src.length; ) {
    const c = src[i];
    if (c === "/" && src[i + 1] === "/") {
      while (i < src.length && src[i] !== "\n") i++;
    } else if (c === "/" && src[i + 1] === "*") {
      i += 2;
      while (i < src.length && !(src[i] === "*" && src[i + 1] === "/")) i++;
      i += 2;
    } else if (c === '"' || c === "'" || c === "`") {
      const start = ++i;
      while (i < src.length && src[i] !== c) i += src[i] === "\\" ? 2 : 1;
      spans.push({ start, text: src.slice(start, i) });
      i++;
    } else {
      i++;
    }
  }
  return spans;
}

/**
 * GATE: a bare `spartan-*` CSS class token renders unstyled (we don't ship
 * `hlm-tailwind-preset.css`). A token counts as a bare CLASS only when it starts at a class
 * boundary — start of the literal, whitespace, a nested quote, or a Tailwind variant `:`.
 *
 * Deliberately NOT flagged (all fine, none need a CSS rule):
 *   - `data-[matches-spartan-invalid=true]:ring-3`  → preceded by `-` (attribute selector)
 *   - `[attr.data-matches-spartan-invalid]`         → preceded by `-`
 *   - `var(--spartan-overlay-width)`                → preceded by `-` (CSS custom property)
 *   - `"@spartan-ng/brain/select"`                  → preceded by `@` (module specifier)
 *   - `"./lib/provide-spartan-hlm"`                 → preceded by `-` (module specifier)
 */
function assertNoBareSpartanClasses() {
  const TOKEN = /spartan-[a-z0-9]+(?:-[a-z0-9]+)*/g;
  const offenders = [];
  for (const file of walk(PRIMS_DIR)) {
    const src = readFileSync(file, "utf8");
    for (const span of stringLiterals(src)) {
      for (let m; (m = TOKEN.exec(span.text)); ) {
        const prev = m.index === 0 ? undefined : span.text[m.index - 1];
        const atClassBoundary = prev === undefined || /[\s"'`:]/.test(prev);
        if (!atClassBoundary) continue;
        const abs = span.start + m.index;
        const line = src.slice(0, abs).split("\n").length;
        offenders.push({ file: file.replaceAll("\\", "/"), line, token: m[0] });
      }
    }
  }
  if (!offenders.length) return;

  console.error(
    `\nnormalize-helm: FAILED — ${offenders.length} bare \`spartan-*\` class token(s).\n` +
      `These class names are UNDEFINED in this repo (we deliberately do not import\n` +
      `@spartan-ng/brain/hlm-tailwind-preset.css), so the component renders UNSTYLED.\n` +
      `Replace each with the equivalent inlined Tailwind utilities:\n`,
  );
  for (const o of offenders) console.error(`  ${o.file}:${o.line}  ${o.token}`);
  console.error("");
  process.exit(1);
}

/** Drop every `@logistics/shared/ui/primitives/*` entry the generator added to tsconfig paths. */
function stripTsconfigPaths() {
  const raw = readFileSync(TSCONFIG, "utf8");
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
  if (removed) writeFileSync(TSCONFIG, JSON.stringify(json, null, 2) + "\n");
  return removed;
}

const vendored = trackedPrimitives();
const { flattened, preserved, forced } = flattenSrc(vendored);
const importsFixed = relativizeImports();
const typeImportsFixed = fixTypeOnlyImports();
const bugsStripped = stripGeneratorBugs();
const cvaStripped = stripInertCva();
const pathsRemoved = stripTsconfigPaths();
console.log(
  `normalize-helm: flattened ${flattened} primitive(s), preserved ${preserved} vendored, ` +
    `force-overwrote ${forced}, relativized ${importsFixed} file(s), ` +
    `fixed ${typeImportsFixed} type-only import(s), stripped ${bugsStripped} generator bug(s), ` +
    `stripped CVA from ${cvaStripped} file(s), removed ${pathsRemoved} tsconfig path alias(es).`,
);
assertNoBareSpartanClasses();
