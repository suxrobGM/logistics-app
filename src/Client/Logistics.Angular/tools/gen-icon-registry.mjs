/**
 * gen-icon-registry.mjs — the single source of truth for the icon runtime.
 *
 * INPUT   tools/codemods/icon-map.json          (hand-verified: name a call site writes -> lucide/brand name)
 * OUTPUT  projects/shared/src/lib/ui/icons/icon-registry.generated.ts
 *              - `UiIconName`  : string-literal union of every KEY in the map. With strictTemplates and
 *                                `name = input.required<UiIconName>()`, a typo is a COMPILE error rather
 *                                than a silently blank <svg>.
 *              - `ICON_ALIASES`: key -> lucide/brand kebab target (the runtime map).
 *              - `*_ICON_NAMES`: per-app name arrays (what each app actually writes).
 *              - `BASE_NG_ICONS`: the @ng-icons record every portal registers.
 *         projects/<app>/src/app/shared/icons/lucide-icons.ts
 *              - `<APP>_NG_ICONS`: only the icons THAT app uses and base does not already provide.
 *                Registering all 133 targets in all four apps would push ~55 extra SVG consts into
 *                admin-portal / customer-portal / website for nothing.
 *
 * The per-app sets are computed by SCANNING each project's own source: `<ui-icon name="x">`,
 * `icon="pi pi-x"`, `icon: "pi-x"`, `icon: "x"` (nav data), `[name]="cond ? 'x' : 'y'"`. Both the legacy
 * `pi pi-x` form and the bare `x` form resolve to the same target, because S3 (the call-site sweep) has
 * not run yet and both forms are live. The scan is deliberately generous: a missing registration renders
 * a blank glyph, an extra one costs a few hundred bytes.
 *
 *   node tools/gen-icon-registry.mjs            # write (runs as prebuild:shared)
 *   node tools/gen-icon-registry.mjs --check    # exit 1 if the generated files are stale
 */

import path from "node:path";
import { pathToFileURL } from "node:url";
import * as lucide from "@ng-icons/lucide";
import { listFiles, readText, relative, WORKSPACE_ROOT, writeText } from "./codemods/lib/io.mjs";

const MAP_FILE = path.join(WORKSPACE_ROOT, "tools/codemods/icon-map.json");
const SHARED_OUT = path.join(
  WORKSPACE_ROOT,
  "projects/shared/src/lib/ui/icons/icon-registry.generated.ts",
);

/** The three hand-vendored brand glyphs (lucide ships none). See ui/icons/brand-icons.ts. */
export const BRAND_EXPORTS = new Set(["brandFacebook", "brandLinkedin", "brandX"]);

/** project -> { dir, registryConst, registryFile } . `shared` is the base set, so it has no app registry. */
export const APPS = {
  "tms-portal": {
    const: "TMS_NG_ICONS",
    names: "TMS_ICON_NAMES",
    file: "projects/tms-portal/src/app/shared/icons/lucide-icons.ts",
  },
  "admin-portal": {
    const: "ADMIN_NG_ICONS",
    names: "ADMIN_ICON_NAMES",
    file: "projects/admin-portal/src/app/shared/icons/lucide-icons.ts",
  },
  "customer-portal": {
    const: "CUSTOMER_NG_ICONS",
    names: "CUSTOMER_ICON_NAMES",
    file: "projects/customer-portal/src/app/shared/icons/lucide-icons.ts",
  },
  website: {
    const: "WEBSITE_NG_ICONS",
    names: "WEBSITE_ICON_NAMES",
    file: "projects/website/src/app/shared/icons/lucide-icons.ts",
  },
};

/**
 * Icons the SHARED LIBRARY renders regardless of what an app writes, plus the runtime's own glyphs.
 * Scanning `projects/shared` finds most of these; this list pins the ones a scan cannot see, above all
 * `circle-alert` — the glyph `resolveNgIcon()` falls back to when a name does not resolve. If that one
 * is missing from an app's registry, the error path renders blank, which is exactly the failure the
 * error path exists to make visible.
 */
const MANDATORY_BASE = [
  "alert-circle", // -> circle-alert : the unresolved-name error glyph
  "spinner", // -> loader-circle : <ui-icon spin>, ui-spinner
  "check",
  "times",
  "circle",
  "search",
  "chevron-down",
  "chevron-up",
  "chevron-left",
  "chevron-right",
  "calendar",
  "eye",
  "eye-slash",
  "sun",
  "moon",
  "info-circle",
  "check-circle",
  "exclamation-triangle",
  "times-circle",
  "sort-up",
  "sort-down",
];

const HEADER = (
  from,
) => `// ---------------------------------------------------------------------------------------------
// GENERATED FILE — DO NOT EDIT.
// Source: ${from}
// Regenerate: node tools/gen-icon-registry.mjs   (runs automatically on \`bun run build:shared\`)
// ---------------------------------------------------------------------------------------------`;

// ---------------------------------------------------------------------------------------------
// name resolution — MUST stay in lock-step with toNgIconName() in ui/icons/ui-icons.ts
// ---------------------------------------------------------------------------------------------

const pascal = (parts) => parts.map((p) => p.charAt(0).toUpperCase() + p.slice(1)).join("");

/** `chevron-down` -> `lucideChevronDown`, `building-2` -> `lucideBuilding2`, `brand-x` -> `brandX`. */
export function toNgIconName(kebab) {
  const parts = kebab.split("-");
  if (parts[0] === "brand") return `brand${pascal(parts.slice(1))}`;
  return `lucide${pascal(parts)}`;
}

/** Reads icon-map.json and fails loudly if any target does not exist in @ng-icons/lucide or the brand set. */
export function loadIconMap() {
  const raw = JSON.parse(readText(MAP_FILE));
  const icons = raw.icons;
  const unresolved = [];

  for (const [name, target] of Object.entries(icons)) {
    const exportName = toNgIconName(target);
    const known = exportName.startsWith("brand")
      ? BRAND_EXPORTS.has(exportName)
      : exportName in lucide;
    if (!known) unresolved.push(`${name} -> ${target} (${exportName})`);
  }

  if (unresolved.length > 0) {
    throw new Error(
      `icon-map.json has ${unresolved.length} target(s) that do not exist:\n  ` +
        unresolved.join("\n  "),
    );
  }
  return icons;
}

// ---------------------------------------------------------------------------------------------
// the scanner
// ---------------------------------------------------------------------------------------------

/** `pi pi-check` / `pi-check` / `pi pi-spin pi-spinner` -> the tokens after each `pi-`. */
const RE_PI = /\bpi-([a-z0-9-]+)/g;
/** `<ui-icon ... name="check">` (static literal only — dynamic bindings are handled below). */
const RE_UI_ICON = /<ui-icon\b[^>]*?\bname="([^"]+)"/g;
/** `icon="pi pi-check"`, `icon: "check"`, `[icon]="'check'"`, `closeIcon="pi pi-times"`, … */
const RE_ICON_PROP =
  /\b(?:icon|iconName|leadingIcon|trailingIcon|menuIcon|toggleIcon|expandIcon|collapseIcon|removeIcon|filterIcon|clearIcon|dropdownIcon|chooseIcon|uploadIcon|cancelIcon|closeIcon|prevIcon|nextIcon)\b\]?\s*[:=]\s*["']([^"'`]*)["']/g;
/** any quoted literal inside an icon-ish binding: `[name]="revealed() ? 'eye-slash' : 'eye'"`. */
const RE_ICON_BINDING = /\[(?:name|icon)\]="([^"]*)"/g;

/**
 * `IconName` in a type position — the marker that a file's bare string literals are icon names.
 *
 * S3 retyped the status->icon lookup tables (`getToolIcon(t): IconName { … return "box"; }`) and the
 * icon inputs (`input<IconName>("inbox")`). Their names live in a bare `return "box";` or an
 * `input<…>(…)` default — no `pi-` prefix and no `icon:` key, so NOTHING above sees them. Before the
 * sweep those same names read `return "pi pi-box"` and RE_PI caught them; the rewrite would otherwise
 * have silently dropped them out of the per-app registries, i.e. re-introduced exactly the blank-glyph
 * bug this generator exists to prevent — and check-icons.mjs, which shares this scanner, would have
 * passed vacuously.
 */
const RE_ICON_NAME_TYPE = /\bIconName\b/;
/** `return "box";` — only trusted in a file that traffics in `IconName`. */
const RE_RETURN_LITERAL = /\breturn\s+["']([a-z][a-z0-9-]*)["']/g;
/** `input<IconName>("inbox")`, `signal<IconName>("check")`, `"x" as IconName`. */
const RE_ICON_TYPED_LITERAL =
  /<IconName[^>]*>\s*\(\s*["']([a-z][a-z0-9-]*)["']|["']([a-z][a-z0-9-]*)["']\s+as\s+IconName\b/g;

/**
 * Every icon name `project` writes, resolved through the map. Names that are not map keys are dropped:
 * the same regexes legitimately catch non-icon strings (`icon: "success"` severities, `pi-spin`), and a
 * literal that IS meant to be an icon but is not in the map is caught by check-icons.mjs, not silently
 * registered here.
 *
 * Deliberately generous. Over-registering costs one unused glyph in a bundle; under-registering renders
 * a blank icon in production, which no other gate catches.
 */
export function scanProject(project, iconMap) {
  const files = listFiles({ dirs: [`projects/${project}/src`], ext: [".ts", ".html"] });
  const names = new Set();

  // Mirrors resolveNgIcon(): `pi pi-clock` / `pi-clock` / `clock` all mean `clock`.
  const consider = (raw) => {
    const last = raw.trim().split(/\s+/).pop() ?? "";
    const name = last.replace(/^pi-/, "");
    if (name in iconMap) names.add(name);
  };

  for (const file of files) {
    if (file.endsWith(".generated.ts")) continue;
    const src = readText(file);

    for (const m of src.matchAll(RE_PI)) consider(m[1]);
    for (const m of src.matchAll(RE_UI_ICON)) consider(m[1]);
    for (const m of src.matchAll(RE_ICON_PROP)) for (const tok of m[1].split(/\s+/)) consider(tok);
    for (const m of src.matchAll(RE_ICON_BINDING)) {
      for (const lit of m[1].matchAll(/'([a-z][a-z0-9-]*)'/g)) consider(lit[1]);
    }

    if (RE_ICON_NAME_TYPE.test(src)) {
      for (const m of src.matchAll(RE_RETURN_LITERAL)) consider(m[1]);
      for (const m of src.matchAll(RE_ICON_TYPED_LITERAL)) consider(m[1] ?? m[2]);
    }
  }

  return names;
}

/** The full picture: base names + per-app extra names, all sorted, plus the ng-icon export sets. */
export function computeRegistries(iconMap = loadIconMap()) {
  const sorted = (set) => [...set].sort();

  const base = new Set([...scanProject("shared", iconMap), ...MANDATORY_BASE]);
  for (const name of MANDATORY_BASE) {
    if (!(name in iconMap))
      throw new Error(`MANDATORY_BASE icon "${name}" is not in icon-map.json`);
  }

  const baseExports = new Set(sorted(base).map((n) => toNgIconName(iconMap[n])));
  const apps = {};

  for (const project of Object.keys(APPS)) {
    const used = scanProject(project, iconMap);
    const extraNames = sorted(used).filter((n) => !base.has(n));
    // Key on the EXPORT, not the name: `alert-circle` and `exclamation-circle` both land on
    // lucideCircleAlert, and base may already provide it under the other alias.
    const extraExports = sorted(
      new Set(extraNames.map((n) => toNgIconName(iconMap[n])).filter((e) => !baseExports.has(e))),
    );
    apps[project] = { names: sorted(used), extraNames, extraExports };
  }

  return { base: sorted(base), baseExports: sorted(baseExports), apps };
}

// ---------------------------------------------------------------------------------------------
// emit
// ---------------------------------------------------------------------------------------------

const quote = (s) => `"${s}"`;
/** Prettier's default `quoteProps: "as-needed"` strips quotes off identifier-safe keys — match it. */
const propKey = (s) => (/^[A-Za-z_$][\w$]*$/.test(s) ? s : quote(s));
const split = (exports) => ({
  lucide: exports.filter((e) => !e.startsWith("brand")),
  brand: exports.filter((e) => e.startsWith("brand")),
});
const lucideImport = (names) =>
  names.length > 0
    ? [`import {\n${names.map((n) => `  ${n},`).join("\n")}\n} from "@ng-icons/lucide";`]
    : [];

const objectLiteral = (name, exports, type = "") =>
  exports.length > 0
    ? `export const ${name}${type} = {\n${exports.map((e) => `  ${e},`).join("\n")}\n};`
    : `export const ${name}: Record<string, string> = {};`;

function emitShared(iconMap, reg) {
  const names = Object.keys(iconMap);
  const { lucide: lucideExports, brand: brandExports } = split(reg.baseExports);

  const nameArray = (constName, list, doc) =>
    [
      doc,
      `export const ${constName}: readonly UiIconName[] = [`,
      ...list.map((n) => `  ${quote(n)},`),
      "];",
    ].join("\n");

  return [
    HEADER("tools/codemods/icon-map.json (via tools/gen-icon-registry.mjs)"),
    "",
    // Import order matches prettier's sort-imports plugin: third-party, then relative.
    ...lucideImport(lucideExports),
    ...(brandExports.length > 0
      ? [`import { ${brandExports.join(", ")} } from "./brand-icons";`]
      : []),
    "",
    "/**",
    " * Every icon name a call site may write. A name outside this union is a COMPILE error at",
    ' * `<ui-icon name="...">` (strictTemplates + `input.required<IconName>()`), which is the whole',
    " * point: an unmapped icon used to render a silently blank <svg>.",
    " */",
    `export type UiIconName =`,
    ...names.map((n, i) => `  | ${quote(n)}${i === names.length - 1 ? ";" : ""}`),
    "",
    "/**",
    " * What an icon-name input accepts. Identical to {@link UiIconName}: S3 swept every call site onto",
    " * the bare canonical name, so the transitional `pi-`-prefixed spelling (LegacyPiIconName) is gone.",
    " * The alias is kept because it reads better at the call sites that already reference it.",
    " */",
    "export type IconName = UiIconName;",
    "",
    "/** Call-site name -> lucide (or hand-vendored `brand-*`) kebab name. The runtime map. */",
    "export const ICON_ALIASES: Record<UiIconName, string> = {",
    ...names.map((n) => `  ${propKey(n)}: ${quote(iconMap[n])},`),
    "};",
    "",
    nameArray(
      "BASE_ICON_NAMES",
      reg.base,
      "/** Icons every portal registers: what the shared library itself renders, plus the error glyph. */",
    ),
    "",
    ...Object.entries(APPS).flatMap(([project, meta]) => [
      nameArray(
        meta.names,
        reg.apps[project].names,
        `/** Every icon name \`${project}\` writes (base included). Used by check-icons.mjs and the ui-lab. */`,
      ),
      "",
    ]),
    "/**",
    " * The icons every portal must register. Merge with the app's own set:",
    " *   provideIcons({ ...BASE_NG_ICONS, ...TMS_NG_ICONS })",
    " */",
    objectLiteral("BASE_NG_ICONS", reg.baseExports),
    "",
  ].join("\n");
}

function emitApp(project, meta, reg) {
  const { extraExports, extraNames } = reg.apps[project];
  const { lucide: lucideExports, brand: brandExports } = split(extraExports);

  return [
    HEADER(`tools/codemods/icon-map.json + a scan of projects/${project}/src`),
    "",
    // "@logistics/…" sorts before "@ng-icons/…" — keep prettier's sort-imports plugin happy.
    ...(brandExports.length > 0
      ? [`import { ${brandExports.join(", ")} } from "@logistics/shared/ui";`]
      : []),
    ...lucideImport(lucideExports),
    "",
    "/**",
    ` * Icons \`${project}\` uses that \`BASE_NG_ICONS\` does not already provide.`,
    ` * Registered in app.config.ts:  provideIcons({ ...BASE_NG_ICONS, ...${meta.const} })`,
    " *",
    ` * ${extraNames.length} name(s) -> ${extraExports.length} distinct glyph(s).`,
    " */",
    objectLiteral(meta.const, extraExports),
    "",
  ].join("\n");
}

// ---------------------------------------------------------------------------------------------

export function run({ check = false } = {}) {
  const iconMap = loadIconMap();
  const reg = computeRegistries(iconMap);

  const outputs = [[SHARED_OUT, emitShared(iconMap, reg)]];
  for (const [project, meta] of Object.entries(APPS)) {
    outputs.push([path.join(WORKSPACE_ROOT, meta.file), emitApp(project, meta, reg)]);
  }

  if (check) {
    const stale = outputs.filter(([file, contents]) => {
      let current = null;
      try {
        current = readText(file);
      } catch {
        return true;
      }
      return current !== contents;
    });

    if (stale.length > 0) {
      console.error("\n  Icon registry is STALE — icon-map.json or a project's icons changed:");
      for (const [file] of stale) console.error(`    - ${relative(file)}`);
      console.error("\n  Fix: node tools/gen-icon-registry.mjs\n");
      return { ok: false, reg };
    }
    return { ok: true, reg };
  }

  let changed = 0;
  for (const [file, contents] of outputs) {
    if (writeText(file, contents)) {
      changed++;
      console.log(`  wrote ${relative(file)}`);
    }
  }

  console.log(
    `\nIcon registry: ${Object.keys(iconMap).length} names -> ${
      new Set(Object.values(iconMap)).size
    } glyphs. base=${reg.baseExports.length}` +
      Object.entries(APPS)
        .map(
          ([p, m]) =>
            ` ${m.const.replace("_NG_ICONS", "").toLowerCase()}=+${reg.apps[p].extraExports.length}`,
        )
        .join("") +
      `. ${changed} file(s) changed.\n`,
  );

  return { ok: true, reg };
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) {
  const result = run({ check: process.argv.includes("--check") });
  process.exit(result.ok ? 0 : 1);
}
