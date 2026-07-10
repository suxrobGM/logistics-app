#!/usr/bin/env node
/**
 * normalize-helm.mjs — post-processor for `ng g @spartan-ng/cli:ui <primitive>`.
 * ============================================================================
 * This is NOT a reimplementation of the Spartan CLI (that was the old
 * `vendor-spartan-helm.mjs`, now deleted). The CLI remains the single source of
 * truth for the Helm component code. This script only fixes ONE monorepo
 * incompatibility in the generator's output:
 *
 *   The generated Helm imports its siblings via the package self-alias
 *   `@logistics/shared/ui/primitives/<name>` and the generator adds matching
 *   `paths` entries to tsconfig.json. When ng-packagr builds `@logistics/shared`
 *   it treats those subpaths as (missing) secondary entry points and fails
 *   `ng build shared`. We consume the library from source, so we rewrite those
 *   imports to plain relative paths and drop the generator's tsconfig additions.
 *
 * Run it right after generating a primitive:
 *   bunx ng g @spartan-ng/cli:ui <primitive> --defaults --no-interactive
 *   bun run ui:normalize          # (== node tools/normalize-helm.mjs)
 *   bunx prettier --write "projects/shared/src/lib/ui/primitives/**" tsconfig.json
 *
 * Idempotent: re-running it on already-relative code is a no-op.
 */
import { readFileSync, writeFileSync, readdirSync, statSync } from "node:fs";
import { join, relative, dirname } from "node:path";

const ALIAS = "@logistics/shared/ui/primitives";
const PRIMS_DIR = "projects/shared/src/lib/ui/primitives";
const TSCONFIG = "tsconfig.json";

function walk(dir) {
  const out = [];
  for (const name of readdirSync(dir)) {
    const p = join(dir, name);
    if (statSync(p).isDirectory()) out.push(...walk(p));
    else if (p.endsWith(".ts")) out.push(p);
  }
  return out;
}

/** Rewrite `@logistics/shared/ui/primitives/<name>` imports to a relative path to `<name>/src`. */
function relativizeImports() {
  let changed = 0;
  for (const file of walk(PRIMS_DIR)) {
    const src = readFileSync(file, "utf8");
    const next = src.replace(
      new RegExp(`(['"\`])${ALIAS}/([a-z0-9-]+)\\1`, "g"),
      (_m, q, name) => {
        const target = join(PRIMS_DIR, name, "src");
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

const importsFixed = relativizeImports();
const pathsRemoved = stripTsconfigPaths();
console.log(
  `normalize-helm: relativized ${importsFixed} file(s), removed ${pathsRemoved} tsconfig path alias(es).`,
);
