#!/usr/bin/env node
/**
 * normalize-helm.mjs — post-processor for `ng g @spartan-ng/cli:ui <primitive>`.
 * ============================================================================
 * This is NOT a reimplementation of the Spartan CLI (that was the old
 * `vendor-spartan-helm.mjs`, now deleted). The CLI remains the single source of
 * truth for the Helm component code. This script fixes the monorepo
 * incompatibilities in the generator's output so `ng build shared` (ng-packagr)
 * succeeds and the layout stays consistent with the hand-authored primitives:
 *
 *   1. Flatten `<name>/src/**` → `<name>/**`. The generator emits a `src/`-nested
 *      layout, but our first primitives (input, textarea, utils) are flat
 *      (`<name>/index.ts`, `<name>/lib/*.ts`). We keep one layout.
 *   2. Drop the regenerated `utils/src`. `utils` is hand-canonicalised (its
 *      `classes()` / `provideSpartanHlm()` are load-bearing); the generator
 *      re-emits an identical copy each run — discard it, keep the flat one.
 *   3. Relativize `@logistics/shared/ui/primitives/<name>` self-alias imports to
 *      plain relative paths. ng-packagr rejects tsconfig path aliases that
 *      resolve outside the entry point; we consume the library from source.
 *   4. Drop the generator's `@logistics/shared/ui/primitives/*` tsconfig paths.
 *
 * Run it right after generating a primitive:
 *   bunx ng g @spartan-ng/cli:ui <primitive> --no-interactive
 *   bun run ui:normalize          # (== node tools/normalize-helm.mjs)
 *   bunx prettier --write "projects/shared/src/lib/ui/primitives/**" tsconfig.json
 *
 * Idempotent: re-running it on already-flat, already-relative code is a no-op.
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
import { join, relative, dirname } from "node:path";

const ALIAS = "@logistics/shared/ui/primitives";
const PRIMS_DIR = "projects/shared/src/lib/ui/primitives";
const TSCONFIG = "tsconfig.json";

/**
 * Hand-customised primitives. `utils` is load-bearing; `input`/`textarea` were deliberately stripped
 * of brain's `BrnInput` / `BrnFieldControlDescribedBy` host-directives (which inject the ambient
 * `NgControl` and drive `aria-invalid` from the raw, ungated control state — the pristine-invalid
 * bug). The generator re-emits stock copies when it pulls them in as dependencies; discard those and
 * keep our committed flat versions.
 */
const PRESERVE = new Set(["utils", "input", "textarea"]);

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

/** Flatten `<name>/src/**` up into `<name>/**`; discard the regenerated `utils/src`. */
function flattenSrc() {
  let flattened = 0;
  for (const name of readdirSync(PRIMS_DIR)) {
    const src = join(PRIMS_DIR, name, "src");
    if (!existsSync(src) || !statSync(src).isDirectory()) continue;
    if (PRESERVE.has(name)) {
      // Hand-customised — drop the regenerated copy, keep our flat version.
      rmSync(src, { recursive: true, force: true });
      continue;
    }
    for (const entry of readdirSync(src)) {
      moveInto(join(src, entry), join(PRIMS_DIR, name, entry));
    }
    rmSync(src, { recursive: true, force: true });
    flattened++;
  }
  return flattened;
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
 */
function fixTypeOnlyImports() {
  let changed = 0;
  for (const file of walk(PRIMS_DIR)) {
    const src = readFileSync(file, "utf8");
    let next = src
      .replace(
        /import \{ BooleanInput \} from "@angular\/cdk\/coercion";/g,
        'import type { BooleanInput } from "@angular/cdk/coercion";',
      )
      .replace(/import \{ ClassValue \} from "clsx";/g, 'import type { ClassValue } from "clsx";')
      .replace(/import \{ ButtonVariants, HlmButtonImports \}/g, "import { type ButtonVariants, HlmButtonImports }");
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
 */
function stripGeneratorBugs() {
  let changed = 0;
  for (const file of walk(PRIMS_DIR)) {
    const src = readFileSync(file, "utf8");
    const next = src.replace(/^\s*\[forceInvalid\]="forceInvalid\(\)"\n/gm, "");
    if (next !== src) {
      writeFileSync(file, next);
      changed++;
    }
  }
  return changed;
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

const flattened = flattenSrc();
const importsFixed = relativizeImports();
const typeImportsFixed = fixTypeOnlyImports();
const bugsStripped = stripGeneratorBugs();
const pathsRemoved = stripTsconfigPaths();
console.log(
  `normalize-helm: flattened ${flattened} primitive(s), relativized ${importsFixed} file(s), ` +
    `fixed ${typeImportsFixed} type-only import(s), stripped ${bugsStripped} generator bug(s), ` +
    `removed ${pathsRemoved} tsconfig path alias(es).`,
);
