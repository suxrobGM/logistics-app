/**
 * Vendors spartan/ui "Helm" components into `projects/shared/src/lib/spartan/`.
 *
 * WHY THIS EXISTS
 * ---------------
 * Helm is code-in-repo by design; `@spartan-ng/cli` is only a copier. We do not install that CLI
 * because it depends on the entire Nx toolchain (nx, @nx/angular, @nx/devkit, @nx/js,
 * @nx/workspace) plus a second, one-major-behind @schematics/angular — into a plain Angular CLI +
 * bun workspace. So we do the copy ourselves.
 *
 * WHAT THE REAL GENERATOR DOES (and why a naive copy fails)
 * --------------------------------------------------------
 * A Helm template's `classes(() => 'spartan-input …')` string is a PLACEHOLDER, not a real class.
 * The generator parses the chosen `style-<theme>.css`, harvests every `.spartan-x { @apply … }`
 * rule into a map, substitutes each placeholder with its utilities, and merges the result with
 * `tailwind-merge`. Copy a template verbatim and you get a component that compiles and renders
 * completely unstyled.
 *
 * WHAT WE ADD
 * -----------
 * spartan's colour tokens collide with this codebase's Tailwind theme on `primary`, `secondary`,
 * `muted` and `accent` — with INVERTED meanings (`text-primary` is dark body text here, brand cyan
 * there). So after inlining we prefix every spartan colour token with `sp-`, matching the
 * `@theme inline` block in `projects/shared/src/styles/spartan.css`. App code is untouched, and a
 * token we fail to prefix renders unstyled (visible) rather than silently wrong.
 *
 * USAGE
 * -----
 *   bun tools/vendor-spartan-helm.mjs utils input
 *   bun tools/vendor-spartan-helm.mjs --list
 *   bun tools/vendor-spartan-helm.mjs --style=nova utils input textarea
 *
 * Re-running is idempotent: it overwrites the target directory for each named primitive. Review the
 * diff — these files are ours now, and local edits WILL be clobbered. Upstream ships tabs and single
 * quotes, so follow up with:
 *
 *   bunx prettier --write "projects/shared/src/lib/spartan/**"
 */

import { execFileSync } from "node:child_process";
import {
  existsSync,
  mkdirSync,
  mkdtempSync,
  readdirSync,
  readFileSync,
  rmSync,
  statSync,
  writeFileSync,
} from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join, relative, resolve } from "node:path";
import { twMerge } from "tailwind-merge";

const CLI_PACKAGE = "@spartan-ng/cli";
const CLI_VERSION = "1.1.0";
const OUT_ROOT = resolve("projects/shared/src/lib/spartan");

/** Tokens the upstream generator intentionally leaves as literal `spartan-*` classes. */
const PLACEHOLDER_ALLOWLIST = new Set([
  "spartan-invalid",
  "spartan-menu-target",
  "spartan-logical-sides",
]);

/**
 * Primitives whose brain form-state wiring we strip.
 *
 * ARCHITECTURAL STANCE: Helm is used for presentation; our `FormValueControl` wrappers own
 * form-state policy. Brain's `BrnInput`/`BrnFieldControl` inject the ambient `NgControl` — which
 * Signal Forms' `[formField]` provides as an `InteropNgControl` — and host-bind `aria-invalid` and
 * `data-invalid` from the RAW `control.invalid`. That is true from form creation, so a required,
 * untouched field would announce itself invalid on page load, and the directive's host binding
 * beats the wrapper's own `[attr.aria-invalid]`. (Verified: `text-field.spec.ts`.)
 *
 * So for these primitives we drop the `hostDirectives` and re-point the styling hook from brain's
 * `data-matches-spartan-invalid` attribute onto the `aria-invalid` the wrapper already sets from
 * `showInvalid() = invalid() && (touched() || dirty())`. Visuals and a11y then agree, and there is
 * one source of truth for "is this field showing an error".
 */
const STRIP_FORM_STATE = new Set(["input", "textarea"]);

/** `data-[matches-spartan-invalid=true]:ring-3` -> `aria-[invalid=true]:ring-3`. */
function repointInvalidHook(text) {
  return text.replaceAll("data-[matches-spartan-invalid=true]", "aria-[invalid=true]");
}

/** Remove `hostDirectives: [...]` and the brain form-state imports it needed. */
function stripFormStateWiring(text) {
  return text
    .replace(/^\s*hostDirectives: \[[\s\S]*?\],\r?\n/m, "")
    .replace(/^import \{ BrnInput \} from '@spartan-ng\/brain\/input';\r?\n/m, "")
    .replace(
      /^import \{ BrnFieldControlDescribedBy \} from '@spartan-ng\/brain\/field';\r?\n/m,
      "",
    );
}

/** Every shadcn colour token spartan's theme registers. Longest first so `primary-foreground` wins. */
const COLOR_TOKENS = [
  "sidebar-primary-foreground",
  "sidebar-accent-foreground",
  "sidebar-foreground",
  "sidebar-primary",
  "sidebar-accent",
  "sidebar-border",
  "sidebar-ring",
  "sidebar",
  "primary-foreground",
  "secondary-foreground",
  "popover-foreground",
  "accent-foreground",
  "muted-foreground",
  "card-foreground",
  "destructive",
  "background",
  "foreground",
  "secondary",
  "popover",
  "primary",
  "accent",
  "muted",
  "border",
  "input",
  "card",
  "ring",
].sort((a, b) => b.length - a.length);

/** Tailwind utility prefixes that take a colour value. */
const COLOR_UTILITIES = [
  "bg",
  "text",
  "border-x",
  "border-y",
  "border-t",
  "border-r",
  "border-b",
  "border-l",
  "border-s",
  "border-e",
  "border",
  "ring-offset",
  "inset-ring",
  "ring",
  "outline",
  "fill",
  "stroke",
  "divide",
  "caret",
  "decoration",
  "shadow",
  "from",
  "via",
  "to",
].sort((a, b) => b.length - a.length);

const TOKEN_PREFIX_RE = new RegExp(
  `(^|[\\s:'"\`\\[])((?:${COLOR_UTILITIES.join("|")}))-(${COLOR_TOKENS.join("|")})(?=$|[\\s/'"\`\\]])`,
  "g",
);

function fetchCli() {
  const dir = mkdtempSync(join(tmpdir(), "spartan-cli-"));
  execFileSync("npm", ["pack", `${CLI_PACKAGE}@${CLI_VERSION}`], {
    cwd: dir,
    stdio: "pipe",
    shell: true,
  });
  const tgz = readdirSync(dir).find((f) => f.endsWith(".tgz"));
  if (!tgz) throw new Error(`npm pack produced no tarball in ${dir}`);
  execFileSync("tar", ["-xzf", tgz], { cwd: dir, stdio: "pipe" });
  return join(dir, "package", "src", "generators");
}

/** Harvest `.spartan-x { @apply … }` into { 'spartan-x': 'utility list' }. */
function buildStyleMap(styleCss) {
  const map = {};
  const re = /\.(spartan-[a-z0-9-]+)\s*\{\s*@apply\s+([^;]+);\s*\}/g;
  for (const [, key, utilities] of styleCss.matchAll(re)) {
    map[key] = utilities.replace(/\s+/g, " ").trim();
  }
  return map;
}

/** Expand every `spartan-*` placeholder in `text`, then tailwind-merge each touched class string. */
function inlineStyles(text, styleMap) {
  const keys = Object.keys(styleMap)
    .filter((k) => !PLACEHOLDER_ALLOWLIST.has(k))
    .sort((a, b) => b.length - a.length);
  if (keys.length === 0) return { text, used: [] };

  const used = new Set();
  // Only rewrite inside quoted strings — that is where class lists live.
  const stringRe = /(['"`])((?:\\.|(?!\1)[^\\])*)\1/g;
  const out = text.replace(stringRe, (whole, quote, body) => {
    let changed = false;
    let next = body;
    for (const key of keys) {
      const re = new RegExp(`(?<![a-z0-9-])${key}(?![a-z0-9-])`, "g");
      if (re.test(next)) {
        next = next.replace(re, styleMap[key]);
        used.add(key);
        changed = true;
      }
    }
    if (!changed) return whole;
    const merged = twMerge(next.replace(/\s+/g, " ").trim());
    // Re-quote as double so inner single quotes in attribute selectors survive verbatim.
    return merged.includes("'") ? JSON.stringify(merged) : `${quote}${merged}${quote}`;
  });
  return { text: out, used: [...used] };
}

/** `bg-primary` -> `bg-sp-primary`, `dark:ring-destructive/40` -> `dark:ring-sp-destructive/40`. */
function prefixColorTokens(text) {
  return text.replace(
    TOKEN_PREFIX_RE,
    (_, lead, utility, token) => `${lead}${utility}-sp-${token}`,
  );
}

function walk(dir) {
  const out = [];
  for (const entry of readdirSync(dir)) {
    const full = join(dir, entry);
    if (statSync(full).isDirectory()) out.push(...walk(full));
    else out.push(full);
  }
  return out;
}

/**
 * Templates import siblings as `<%- importAlias %>/utils`, so the alias must resolve to the
 * `spartan/` root. `spartan/input/index.ts` is 1 level down (`..`); `spartan/input/lib/hlm-input.ts`
 * is 2 (`../..`). `relPath` is relative to the primitive's own directory, hence the `+ 1`.
 */
function importAliasFor(relPath) {
  const depth = relPath.split(/[\\/]/).length - 1;
  return "../".repeat(depth + 1).slice(0, -1);
}

function main() {
  const args = process.argv.slice(2);
  const styleArg = args.find((a) => a.startsWith("--style="));
  const style = styleArg ? styleArg.split("=")[1] : "nova";
  const primitives = args.filter((a) => !a.startsWith("--"));
  const list = args.includes("--list");

  const generators = fetchCli();
  const libsDir = join(generators, "ui", "libs");

  if (list || primitives.length === 0) {
    const available = readdirSync(libsDir).filter((d) => statSync(join(libsDir, d)).isDirectory());
    console.log(`Available primitives (${available.length}):\n  ${available.join(" ")}`);
    if (!list) console.error("\nerror: name at least one primitive to vendor.");
    process.exit(list ? 0 : 1);
  }

  const styleCss = readFileSync(join(generators, "ui", `style-${style}.css`), "utf8");
  const styleMap = buildStyleMap(styleCss);
  console.log(`style-${style}.css -> ${Object.keys(styleMap).length} spartan-* rules`);

  for (const primitive of primitives) {
    const filesDir = join(libsDir, primitive, "files");
    if (!existsSync(filesDir)) {
      console.error(`error: unknown primitive "${primitive}"`);
      process.exit(1);
    }

    const target = join(OUT_ROOT, primitive);
    rmSync(target, { recursive: true, force: true });

    const usedAll = new Set();
    for (const src of walk(filesDir)) {
      const rel = relative(filesDir, src).replace(/\.template$/, "");
      let text = readFileSync(src, "utf8");

      text = text.replaceAll("<%- importAlias %>", importAliasFor(rel));
      const { text: inlined, used } = inlineStyles(text, styleMap);
      used.forEach((u) => usedAll.add(u));
      text = prefixColorTokens(inlined);
      if (STRIP_FORM_STATE.has(primitive)) {
        text = stripFormStateWiring(repointInvalidHook(text));
      }

      const dest = join(target, rel);
      mkdirSync(dirname(dest), { recursive: true });
      writeFileSync(dest, text, "utf8");
      console.log(`  ${primitive}/${rel.replace(/\\/g, "/")}`);
    }

    // Only a token that IS a style-map key can be an unexpanded placeholder. Anything else matching
    // /spartan-\w+/ is incidental — a filename (`provide-spartan-hlm`) or a package (`@spartan-ng/…`).
    const leftover = readdirSync(target, { recursive: true })
      .filter((f) => typeof f === "string" && f.endsWith(".ts"))
      .flatMap((f) =>
        [
          ...readFileSync(join(target, f), "utf8").matchAll(/(?<![a-z0-9-])spartan-[a-z0-9-]+/g),
        ].map((m) => m[0]),
      )
      .filter((c) => styleMap[c] !== undefined && !PLACEHOLDER_ALLOWLIST.has(c));
    if (leftover.length) {
      console.error(
        `  !! unresolved placeholders in ${primitive}: ${[...new Set(leftover)].join(", ")}`,
      );
      process.exit(1);
    }
    console.log(`  ${primitive}: inlined [${[...usedAll].join(", ") || "none"}]`);
  }
}

main();
