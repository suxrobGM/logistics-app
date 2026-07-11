/**
 * dead-primeng-imports.mjs — delete `primeng/*` imports whose component is no longer rendered.
 *
 * S8 replaced `<p-toast>` / `<p-confirmDialog>` with `ToastService` + `ui-confirm-dialog` and deleted
 * the TAGS, but left `ToastModule` / `ConfirmDialogModule` sitting in the import statement and in the
 * component's `imports: [...]`. They pull the PrimeNG module into the bundle, they keep the burndown
 * count artificially high, and they make it look like the page still uses PrimeNG when it does not.
 * The same rot exists for any other primeng module whose tag has since been swept.
 *
 * HOW A MODULE IS PROVED DEAD (this is the whole safety argument):
 *   For each `primeng/<pkg>` import in a component, take the SELECTORS that package's module exports
 *   — read from the real `primeng/<pkg>` bundle in node_modules, not from a hardcoded list — and
 *   look for any of them in that component's template. If NONE appear, the import cannot be doing
 *   anything, and it goes.
 *
 * WHY THE SELECTORS COME FROM node_modules: a hand-written tag list is a guess, and a wrong guess
 * here DELETES A LIVE IMPORT and breaks the page. `primeng/table` alone exports `p-table`,
 * `[pSortableColumn]`, `[pEditableColumn]`, `[pReorderableRow]`, `p-columnFilter`, … Attribute
 * selectors are exactly what a tag-only list misses, and they are invisible in a `<p-...>` grep.
 *
 * REFUSALS (never guessed at):
 *   - a component whose `imports: [...]` uses a spread
 *   - a `primeng/*` import in a NON-component file (a service, a type-only import, a re-export)
 *   - any package whose selectors could not be read out of node_modules
 * Those are reported and left alone.
 */

import fs from "node:fs";
import path from "node:path";
import { listFiles, parseMode, readText, relative, WORKSPACE_ROOT, writeText } from "./lib/io.mjs";

const mode = parseMode();
const refusals = [];
const changed = [];
const removals = [];

// ---------------------------------------------------------------------------------------------
// Selector table, read from the installed PrimeNG.
// ---------------------------------------------------------------------------------------------

const FESM = path.join(WORKSPACE_ROOT, "node_modules/primeng/fesm2022");

/** Every selector declared by the components/directives in `primeng/<pkg>`. */
function selectorsOf(pkg) {
  const file = path.join(FESM, `primeng-${pkg.replace(/\//g, "-")}.mjs`);
  if (!fs.existsSync(file)) {
    return null;
  }
  const source = fs.readFileSync(file, "utf8");

  const selectors = new Set();
  // Both ɵɵngDeclareComponent and ɵɵngDeclareDirective carry `selector: "..."`.
  for (const match of source.matchAll(/selector:\s*"([^"]+)"/g)) {
    for (const part of match[1].split(",")) {
      const token = part.trim();
      if (!token) continue;
      // `p-table` -> tag; `[pSortableColumn]` -> attribute; `input[pInputText]` -> attribute on tag.
      const attr = token.match(/\[([^\]]+)\]/);
      if (attr) {
        selectors.add({ kind: "attr", name: attr[1] });
      } else if (/^[a-zA-Z][\w-]*$/.test(token)) {
        selectors.add({ kind: "tag", name: token });
      }
    }
  }
  return [...selectors];
}

/** Is any selector from `pkg` actually used in `template`? */
function isUsed(template, selectors) {
  return selectors.some(({ kind, name }) => {
    if (kind === "tag") {
      // `<p-table` / `<p-table>` — but not `<p-tableFoo`.
      return new RegExp(`<${name}[\\s/>]`).test(template);
    }
    // An attribute: bare (`pSortableColumn`), bound (`[pSortableColumn]`) or evented.
    // The trailing class MUST include `/` — `<input pInputText/>` is a real, live usage, and a class
    // that stops at `>` reports it as dead and deletes the import out from under a working template.
    return new RegExp(`[\\s\\[(]${name}[\\s\\])=>/]`).test(template);
  });
}

/**
 * Remove one symbol from a standalone component's `imports: [...]`, by parsing the array rather than
 * pattern-matching around it — a regex that eats the wrong comma produces a file that still parses
 * and silently drops a DIFFERENT directive.
 */
function dropFromComponentImports(src, symbol) {
  const open = src.indexOf("imports:");
  if (open === -1) return { src, changed: false };

  const lb = src.indexOf("[", open);
  if (lb === -1) return { src, changed: false };

  let depth = 0;
  let rb = -1;
  for (let i = lb; i < src.length; i++) {
    if (src[i] === "[") depth++;
    else if (src[i] === "]" && --depth === 0) {
      rb = i;
      break;
    }
  }
  if (rb === -1) return { src, changed: false };

  const names = src
    .slice(lb + 1, rb)
    .split(",")
    .map((n) => n.trim())
    .filter(Boolean);

  if (!names.includes(symbol)) return { src, changed: false };

  const kept = names.filter((n) => n !== symbol);
  return { src: `${src.slice(0, lb + 1)}${kept.join(", ")}${src.slice(rb)}`, changed: true };
}

// ---------------------------------------------------------------------------------------------
// Sweep
// ---------------------------------------------------------------------------------------------

const IMPORT_RE =
  /^import\s+(?:type\s+)?\{([^}]*)\}\s*from\s*["']primeng\/([^"']+)["'];?[ \t]*\r?\n/gm;

for (const file of listFiles({ ext: [".ts"] })) {
  let src = readText(file);
  if (!src.includes("primeng/")) continue;

  const templateMatch = /templateUrl:\s*["'`]([^"'`]+)["'`]/.exec(src);
  if (!templateMatch) {
    // No template: a service, a token, a type re-export. Nothing to prove here — leave it.
    continue;
  }

  const templatePath = path.resolve(path.dirname(file), templateMatch[1]);
  if (!fs.existsSync(templatePath)) {
    refusals.push(`${relative(file)} — templateUrl does not resolve; not touching its imports.`);
    continue;
  }
  const template = readText(templatePath);

  const statements = [...src.matchAll(IMPORT_RE)];
  if (statements.length === 0) continue;

  const dead = [];
  for (const statement of statements) {
    const [, names, pkg] = statement;
    const selectors = selectorsOf(pkg);
    if (selectors === null) {
      refusals.push(`${relative(file)} — cannot read selectors for 'primeng/${pkg}'.`);
      continue;
    }
    if (selectors.length === 0 || isUsed(template, selectors)) {
      continue;
    }
    dead.push({
      statement: statement[0],
      pkg,
      symbols: names
        .split(",")
        .map((n) => n.trim())
        .filter(Boolean),
    });
  }

  if (dead.length === 0) continue;

  if (/imports:\s*\[[^\]]*\.\.\./s.test(src)) {
    refusals.push(`${relative(file)} — imports array uses a spread; not editing.`);
    continue;
  }

  for (const { statement, pkg, symbols } of dead) {
    src = src.replace(statement, "");
    for (const symbol of symbols) {
      const bare = symbol
        .replace(/^type\s+/, "")
        .split(" as ")[0]
        .trim();
      const dropped = dropFromComponentImports(src, bare);
      // A type-only symbol never appears in `imports: [...]`; that is not a failure.
      if (dropped.changed) {
        src = dropped.src;
      }
    }
    removals.push(`${relative(file)} — primeng/${pkg} (${symbols.join(", ")})`);
  }

  if (mode === "apply") {
    if (writeText(file, src)) changed.push(file);
  }
}

for (const line of removals) {
  console.log(`DEAD     ${line}`);
}
for (const line of refusals) {
  console.error(`REFUSED  ${line}`);
}

if (mode === "check") {
  if (removals.length > 0) {
    console.error(`\ndead-primeng-imports --check: ${removals.length} dead import(s) remain.`);
    process.exit(1);
  }
  console.log("dead-primeng-imports --check: clean.");
  process.exit(0);
}

console.log(
  `dead-primeng-imports --${mode}: ${removals.length} dead import(s), ` +
    `${refusals.length} refusal(s).`,
);
