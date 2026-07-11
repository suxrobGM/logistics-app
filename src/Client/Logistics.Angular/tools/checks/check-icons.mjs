/**
 * check-icons.mjs — the blank-icon guard.
 *
 * An icon bug is invisible to every other gate we have: the build stays green, the tests stay green,
 * lint stays green, and the page renders a 0x0 <svg> where a glyph should be. Three ways that happens,
 * one check each:
 *
 *   1. STALE REGISTRY   icon-map.json (or a project's icon usage) changed and nobody re-ran the
 *                       generator, so `UiIconName` / `ICON_ALIASES` / the per-app registries disagree
 *                       with the map.
 *   2. UNMAPPED NAME    a `<ui-icon name="literal">` whose name is not in the map. `input.required
 *                       <UiIconName>()` makes this a compile error too, but this reports ALL of them at
 *                       once with file:line, which is what the S3 sweep needs.
 *   3. UNREGISTERED     an app writes an icon whose glyph is in neither `BASE_NG_ICONS` nor that app's
 *                       `<APP>_NG_ICONS` — or the app.config.ts never spreads the registry at all.
 *                       This is THE blank-icon bug, and it is the one nothing else catches.
 *
 *   node tools/checks/check-icons.mjs      (or: bun run check:icons)
 */

import path from "node:path";
import { pathToFileURL } from "node:url";
import { listFiles, readText, relative, WORKSPACE_ROOT } from "../codemods/lib/io.mjs";
import {
  APPS,
  computeRegistries,
  run as generate,
  loadIconMap,
  scanProject,
  toNgIconName,
} from "../gen-icon-registry.mjs";

const SHARED_GENERATED = path.join(
  WORKSPACE_ROOT,
  "projects/shared/src/lib/ui/icons/icon-registry.generated.ts",
);

/** Pull the identifiers out of `export const NAME = { lucideA, lucideB };` in a generated registry. */
function registeredExports(file, constName) {
  const src = readText(file);
  const block = new RegExp(`export const ${constName}[^=]*= \\{([^}]*)\\}`).exec(src);
  if (!block) return null;
  return new Set(
    block[1]
      .split(",")
      .map((s) => s.trim())
      .filter(Boolean),
  );
}

/** A doc-comment line. The icon runtime's own docs quote `<ui-icon name="…">`; those are not call sites. */
const COMMENT_LINE = /^\s*(\/\/|\/\*|\*)/;

/** Every `<ui-icon name="literal">` in the repo, with file:line, so unmapped names report as a list. */
function uiIconLiterals() {
  const out = [];
  for (const file of listFiles({ dirs: ["projects"], ext: [".html", ".ts"] })) {
    const lines = readText(file).split("\n");
    lines.forEach((line, i) => {
      if (COMMENT_LINE.test(line)) return;
      for (const m of line.matchAll(/<ui-icon\b[^>]*?\bname="([^"]+)"/g)) {
        out.push({ file, line: i + 1, name: m[1] });
      }
    });
  }
  return out;
}

export function run() {
  console.log("\nIcons (blank-glyph guard)");

  const failures = [];
  const iconMap = loadIconMap();

  // 1. stale generated files
  const gen = generate({ check: true });
  if (!gen.ok)
    failures.push("generated icon registry is stale — run: node tools/gen-icon-registry.mjs");

  // 2. every <ui-icon name="literal"> is a known name — after the same legacy-prefix normalization
  //    resolveNgIcon applies, so a not-yet-swept prefixed spelling is not a false failure
  const bare = (name) => (name.trim().split(/\s+/).pop() ?? "").replace(/^pi-/, "");
  const literals = uiIconLiterals();
  const unmapped = literals.filter((l) => !(bare(l.name) in iconMap));
  for (const l of unmapped) {
    failures.push(
      `${relative(l.file)}:${l.line} — <ui-icon name="${l.name}"> is not in icon-map.json`,
    );
  }

  // 3. every icon an app resolves is registered for that app, and the app.config actually spreads it
  const reg = computeRegistries(iconMap);
  const base = registeredExports(SHARED_GENERATED, "BASE_NG_ICONS");
  if (!base) failures.push("BASE_NG_ICONS not found in icon-registry.generated.ts");

  const counts = [];

  for (const [project, meta] of Object.entries(APPS)) {
    const registryFile = path.join(WORKSPACE_ROOT, meta.file);
    const appExports = registeredExports(registryFile, meta.const);
    if (!appExports) {
      failures.push(`${meta.const} not found in ${meta.file}`);
      continue;
    }

    const provided = new Set([...(base ?? []), ...appExports]);
    const missing = [];
    for (const name of scanProject(project, iconMap)) {
      const exportName = toNgIconName(iconMap[name]);
      if (!provided.has(exportName)) missing.push(`${name} -> ${exportName}`);
    }
    for (const m of missing) {
      failures.push(
        `${project}: "${m}" is used but registered in neither BASE_NG_ICONS nor ${meta.const}`,
      );
    }

    // A perfect registry that app.config never provides renders exactly as many blanks as no registry.
    const config = readText(path.join(WORKSPACE_ROOT, `projects/${project}/src/app/app.config.ts`));
    const provideCall = /provideIcons\(\{([^}]*)\}\)/.exec(config);
    if (!provideCall || !provideCall[1].includes("...BASE_NG_ICONS")) {
      failures.push(`${project}/app.config.ts: provideIcons() does not spread BASE_NG_ICONS`);
    } else if (appExports.size > 0 && !provideCall[1].includes(`...${meta.const}`)) {
      failures.push(`${project}/app.config.ts: provideIcons() does not spread ${meta.const}`);
    }

    counts.push(
      `  ${project.padEnd(16)} ${String(reg.apps[project].names.length).padStart(3)} names -> ` +
        `${String(provided.size).padStart(3)} glyphs (base ${base?.size ?? 0} + ${appExports.size})`,
    );
  }

  console.log(
    `  map: ${Object.keys(iconMap).length} names -> ${new Set(Object.values(iconMap)).size} glyphs` +
      `   |   <ui-icon name="…"> call sites: ${literals.length}`,
  );
  for (const line of counts) console.log(line);

  if (failures.length > 0) {
    console.error(`\n  FAIL — ${failures.length} icon problem(s):`);
    for (const f of failures) console.error(`    - ${f}`);
    return { ok: false };
  }

  console.log("  OK — every icon resolves and is registered in every app that uses it.");
  return { ok: true };
}

// pathToFileURL — see the note in spartan-tokens.mjs.
if (import.meta.url === pathToFileURL(process.argv[1]).href) {
  const result = run();
  process.exit(result.ok ? 0 : 1);
}
