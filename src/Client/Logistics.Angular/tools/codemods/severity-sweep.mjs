/**
 * severity-sweep.mjs — S5 phase 1: retype every PrimeNG severity PRODUCER to `UiBadgeIntent`.
 *
 *   node tools/codemods/severity-sweep.mjs --census   # report only; writes .severity-refusals.json
 *   node tools/codemods/severity-sweep.mjs --apply    # retype + fix imports, then prettier
 *   node tools/codemods/severity-sweep.mjs --check    # exit 1 if any legacy severity type remains
 *
 * Templates are NOT touched. This step is deliberately TS-only: ~60 helpers hand a severity string to
 * ~80 `<p-tag [severity]>` bindings, and if the template sweep goes first it rewrites the tags while
 * TS keeps returning a vocabulary the new component does not have.
 *
 * =============================================================================================
 * FIVE NAMES FOR ONE TYPE
 * =============================================================================================
 * The producers were annotated with, in order of popularity:
 *
 *     Tag["severity"] / PrimeTag["severity"]   the PrimeNG component's own input type
 *     TagSeverity                              tms  shared/types/index.ts
 *     SeverityLevel                            tms  shared/utils/labels.ts   <- NARROWER (no contrast)
 *     SeverityType                             tms  employees-list.ts (file-local)
 *     BadgeSeverity                            shared ui/content/badge/badge.ts
 *     ...plus ~20 hand-written inline unions, several of them narrowed ad hoc
 *
 * All of them collapse to `UiBadgeIntent`. Note `SeverityLevel` was NOT structurally identical — it
 * lacks `contrast` — so this is a widening for its four call sites, never a narrowing.
 *
 * =============================================================================================
 * WHY THE `Tag` IMPORT CANNOT JUST BE DELETED
 * =============================================================================================
 * `import { Tag } from "primeng/tag"` is BOTH a value and a type in this codebase. Three components
 * (load-type-tag, truck-type-tag, trip-status-tag) put `Tag` in `@Component.imports` — it is the
 * standalone component — AND use `Tag["severity"]` as a type. Retyping kills the type use only. So
 * the specifier is dropped ONLY when `Tag` has no remaining reference of EITHER kind, decided by
 * counting identifier nodes on the post-edit AST rather than by grepping the text.
 *
 * =============================================================================================
 * THE CONSERVATION CHECK
 * =============================================================================================
 * A retype that quietly drops a `case` arm, or rewrites a literal, changes what colour a tag paints
 * — and nothing downstream would notice, because every value in the old vocabulary is also a value in
 * the new one. So `conserveLiterals()` counts every severity string literal in the file before and
 * after and hard-fails on any difference. This codemod changes TYPES, never VALUES. (The `*-tag`
 * icon literals are a separate, deliberate value change and are NOT in this codemod's scope.)
 */

import path from "node:path";
import ts from "typescript";
import { listFiles, parseMode, readText, relative, runPrettier, writeText } from "./lib/io.mjs";
import { Splicer } from "./lib/html.mjs";
import { addImport, removeImportSpecifier } from "./lib/ts.mjs";

const REFUSALS_OUT = path.join(
  path.dirname(new URL(import.meta.url).pathname.slice(1)),
  ".severity-refusals.json",
);

/** The six values `<p-tag severity>` actually accepts. Anything else is not a badge severity. */
const VOCAB = new Set(["success", "secondary", "info", "warn", "danger", "contrast"]);

/** The legacy type names, each a structural (or narrower) duplicate of UiBadgeIntent. */
const LEGACY_TYPE_NAMES = new Set([
  "TagSeverity",
  "SeverityLevel",
  "SeverityType",
  "BadgeSeverity",
]);

/** `Tag["severity"]`, `PrimeTag["severity"]` — the PrimeNG input type used as our vocabulary. */
const isPrimeSeverityIndexedAccess = (node, sf) =>
  ts.isIndexedAccessTypeNode(node) && /^\w*Tag\["severity"\]$/.test(node.getText(sf).replace(/'/g, '"'));

/**
 * A declaration whose NAME says it carries a badge severity. The name guard is what keeps this
 * codemod away from the many unrelated `severity` unions in the repo — the toast service's
 * "info"|"success"|"warn"|"error", ui-alert's intents, and the DOMAIN severities that come off the
 * API (AccidentSeverity, DefectSeverity: "minor"|"major"|"critical"). Those are PARAMETERS to the
 * producers, never their return type, and they must not be touched.
 */
const NAME_SAYS_SEVERITY = /severity|tier|badge|intent/i;

/** An inline union that is entirely badge-severity literals (plus optional undefined/null). */
function isVocabUnion(node, sf) {
  if (!ts.isUnionTypeNode(node)) return false;
  let literals = 0;
  for (const m of node.types) {
    if (m.kind === ts.SyntaxKind.UndefinedKeyword || m.kind === ts.SyntaxKind.NullKeyword) continue;
    if (!ts.isLiteralTypeNode(m) || !ts.isStringLiteral(m.literal)) return false;
    if (!VOCAB.has(m.literal.text)) return false;
    literals++;
  }
  return literals >= 2;
}

/** The nearest named declaration enclosing a type node, for the NAME_SAYS_SEVERITY guard. */
function ownerName(node, sf) {
  for (let n = node.parent; n; n = n.parent) {
    if (
      ts.isMethodDeclaration(n) ||
      ts.isFunctionDeclaration(n) ||
      ts.isPropertyDeclaration(n) ||
      ts.isPropertySignature(n) ||
      ts.isVariableDeclaration(n) ||
      ts.isGetAccessorDeclaration(n) ||
      ts.isTypeAliasDeclaration(n)
    ) {
      return n.name?.getText(sf) ?? "";
    }
    // `computed<Tag["severity"]>(...)` — the type arg's owner is the variable it initialises.
    if (ts.isCallExpression(n)) continue;
  }
  return "";
}

const refusals = [];
const refuse = (file, reason, snippet = "") =>
  refusals.push({ file: relative(file), reason, snippet: snippet.replace(/\s+/g, " ").slice(0, 110) });

const stats = { files: 0, indexedAccess: 0, legacyName: 0, inlineUnion: 0, aliasesDeleted: 0, importsAdded: 0, tagImportsDropped: 0 };
const retypedPerFile = new Map();

// -----------------------------------------------------------------------------------------------
// literal conservation — this codemod changes TYPES, never VALUES
// -----------------------------------------------------------------------------------------------

/**
 * Count severity strings in VALUE position only.
 *
 * The obvious `src.match(/"warn"/g)` is wrong, and wrong in the direction that matters: a type
 * annotation is made of string literals too (`"danger" | "info"`), so collapsing a union to
 * `UiBadgeIntent` legitimately destroys literals and a text count reads it as a lost value. Ask the
 * AST which literals are types and which are data — only the data must be conserved.
 */
function countValueLiterals(file, src) {
  const sf = ts.createSourceFile(file, src, ts.ScriptTarget.Latest, true, ts.ScriptKind.TS);
  let n = 0;
  const walk = (node) => {
    if (ts.isStringLiteral(node) && VOCAB.has(node.text) && !ts.isLiteralTypeNode(node.parent)) n++;
    ts.forEachChild(node, walk);
  };
  ts.forEachChild(sf, walk);
  return n;
}

function conserveLiterals(file, before, after) {
  const b = countValueLiterals(file, before);
  const a = countValueLiterals(file, after);
  if (b !== a) {
    console.error(
      `\nFATAL: ${relative(file)} — severity VALUE count changed ${b} -> ${a}.\n` +
        `This codemod retypes; it must never rewrite a value. Refusing to write.`,
    );
    process.exit(1);
  }
}

// -----------------------------------------------------------------------------------------------
// the import specifier for UiBadgeIntent, from wherever the file lives
// -----------------------------------------------------------------------------------------------

function intentImportSpecifier(file) {
  const rel = relative(file);
  if (!rel.startsWith("projects/shared/")) return "@logistics/shared/ui";
  const from = path.dirname(file);
  const to = path.join(
    path.dirname(file).split("projects")[0],
    "projects/shared/src/lib/ui/content/badge/badge-intent",
  );
  let spec = path.relative(from, to).split(path.sep).join("/");
  if (!spec.startsWith(".")) spec = `./${spec}`;
  return spec;
}

// -----------------------------------------------------------------------------------------------
// one file
// -----------------------------------------------------------------------------------------------

function sweepFile(file, source) {
  const sf = ts.createSourceFile(file, source, ts.ScriptTarget.Latest, true, ts.ScriptKind.TS);
  const splicer = new Splicer(source, relative(file));
  const retyped = [];
  let touchedAlias = null;

  /** Type nodes we rewrote, so the import pass can tell what became unused. */
  const visit = (node) => {
    // --- `export type TagSeverity = "success" | ...` — the alias declarations themselves.
    if (ts.isTypeAliasDeclaration(node) && LEGACY_TYPE_NAMES.has(node.name.getText(sf))) {
      if (!isVocabUnion(node.type, sf)) {
        refuse(file, `alias ${node.name.getText(sf)} is not a plain vocabulary union — hand-convert`, node.getText(sf));
        return;
      }
      // Delete the whole statement (plus its leading jsdoc/comment block and trailing newline).
      const start = node.getFullStart();
      let end = node.getEnd();
      while (end < source.length && (source[end] === ";" || source[end] === "\r" || source[end] === "\n")) end++;
      splicer.remove(start, end, `delete alias ${node.name.getText(sf)}`);
      touchedAlias = node.name.getText(sf);
      stats.aliasesDeleted++;
      retyped.push(`(alias) ${node.name.getText(sf)} deleted`);
      return;
    }

    // --- `Tag["severity"]` / `PrimeTag["severity"]`
    if (isPrimeSeverityIndexedAccess(node, sf)) {
      splicer.replace(node.getStart(sf), node.getEnd(), "UiBadgeIntent", "indexed access");
      stats.indexedAccess++;
      retyped.push(`${ownerName(node, sf) || "?"}: ${node.getText(sf)}`);
      return;
    }

    // --- a bare reference to one of the legacy names, in a TYPE position
    if (ts.isTypeReferenceNode(node) && LEGACY_TYPE_NAMES.has(node.typeName.getText(sf))) {
      splicer.replace(node.getStart(sf), node.getEnd(), "UiBadgeIntent", "legacy type name");
      stats.legacyName++;
      retyped.push(`${ownerName(node, sf) || "?"}: ${node.typeName.getText(sf)}`);
      return;
    }

    // --- a hand-written inline union of vocabulary literals
    if (isVocabUnion(node, sf)) {
      const owner = ownerName(node, sf);
      if (!NAME_SAYS_SEVERITY.test(owner)) {
        refuse(file, `union of severity literals on "${owner}" — name does not say severity; hand-check`, node.getText(sf));
        return;
      }
      const hasUndefined = node.types.some(
        (m) => m.kind === ts.SyntaxKind.UndefinedKeyword || m.kind === ts.SyntaxKind.NullKeyword,
      );
      // Keep an explicit `| undefined` — dropping it silently narrows a signature a caller may rely
      // on. tsc will tell us if it was vestigial; guessing here is how a null slips through.
      const text = hasUndefined ? "UiBadgeIntent | undefined" : "UiBadgeIntent";
      splicer.replace(node.getStart(sf), node.getEnd(), text, "inline union");
      stats.inlineUnion++;
      retyped.push(`${owner}: ${node.getText(sf).replace(/\s+/g, " ")}`);
      return;
    }

    ts.forEachChild(node, visit);
  };
  ts.forEachChild(sf, visit);

  const hasTypeEdits = splicer.size > 0;
  let out = hasTypeEdits ? splicer.apply() : source;
  if (hasTypeEdits) conserveLiterals(file, source, out);

  // --- imports ------------------------------------------------------------------------------
  // This pass runs whether or not we retyped anything in THIS run. Gating it on `splicer.size > 0`
  // (the obvious shape) makes the codemod un-idempotent in the one case that matters: a file whose
  // types are already converted but whose stale import survived a bug in an earlier run can never be
  // healed by re-running, because it now has no type edits to trigger the cleanup.
  let importsChanged = false;
  const drop = (mod, name) => {
    const r = removeImportSpecifier(out, mod, name);
    if (r.changed) {
      out = r.src;
      importsChanged = true;
    }
  };
  for (const [mod, name] of [
    ["@/shared/types", "TagSeverity"],
    ["@/shared/utils", "SeverityLevel"],
    ["@logistics/shared/ui", "BadgeSeverity"],
  ]) {
    drop(mod, name);
  }

  // Add UiBadgeIntent only if the file actually mentions it, and never in the file that defines it.
  if (/\bUiBadgeIntent\b/.test(out) && !file.endsWith("badge-intent.ts")) {
    const added = addImport(out, "UiBadgeIntent", intentImportSpecifier(file));
    if (added.changed) {
      stats.importsAdded++;
      importsChanged = true;
    }
    out = added.src;
  }

  // Drop `Tag` / `PrimeTag` from primeng/tag ONLY if nothing references it any more — as a VALUE
  // (`@Component.imports: [Tag]`) or as a type. Decided on the post-edit AST, not by grep.
  const after = ts.createSourceFile(file, out, ts.ScriptTarget.Latest, true, ts.ScriptKind.TS);
  for (const alias of ["Tag", "PrimeTag"]) {
    if (!new RegExp(`\\b${alias}\\b`).test(out)) continue;
    let uses = 0;
    const countRefs = (n) => {
      if (ts.isImportDeclaration(n)) return; // don't count the import statement itself
      if (ts.isIdentifier(n) && n.text === alias) uses++;
      ts.forEachChild(n, countRefs);
    };
    ts.forEachChild(after, countRefs);
    if (uses > 0) continue;
    const dropped = removeImportSpecifier(out, "primeng/tag", "Tag");
    if (dropped.changed) {
      out = dropped.src;
      stats.tagImportsDropped++;
      importsChanged = true;
    }
  }

  if (!hasTypeEdits && !importsChanged) return null;
  if (retyped.length > 0) retypedPerFile.set(relative(file), retyped);
  return { out, touchedAlias };
}

// -----------------------------------------------------------------------------------------------
// main
// -----------------------------------------------------------------------------------------------

const mode = parseMode();
const touched = new Map();

for (const file of listFiles({ dirs: ["projects"], ext: [".ts"] })) {
  const rel = relative(file);
  if (rel.includes("/generated/") || rel.endsWith(".spec.ts")) continue;
  const source = readText(file);
  if (!/severity|Severity|TagSeverity|BadgeSeverity/.test(source)) continue;

  const res = sweepFile(file, source);
  if (!res) continue;
  stats.files++;
  touched.set(file, res.out);
}

if (mode === "apply") {
  for (const [file, src] of touched) writeText(file, src);
  runPrettier([...touched.keys()]);
}

// -----------------------------------------------------------------------------------------------
// report
// -----------------------------------------------------------------------------------------------

const line = "=".repeat(86);
console.log(`\nseverity-sweep --${mode}\n${line}`);
console.log(`files retyped          ${String(stats.files).padStart(4)}`);
console.log(`  Tag["severity"]      ${String(stats.indexedAccess).padStart(4)}`);
console.log(`  legacy type names    ${String(stats.legacyName).padStart(4)}`);
console.log(`  inline unions        ${String(stats.inlineUnion).padStart(4)}`);
console.log(`  alias decls deleted  ${String(stats.aliasesDeleted).padStart(4)}`);
console.log(`  UiBadgeIntent import ${String(stats.importsAdded).padStart(4)}`);
console.log(`  primeng Tag dropped  ${String(stats.tagImportsDropped).padStart(4)}`);

console.log(`\n${line}\nPRODUCERS RETYPED`);
for (const [f, list] of [...retypedPerFile].sort()) {
  console.log(`  ${f}`);
  for (const r of list) console.log(`      ${r}`);
}

console.log(`\n${line}\nREFUSALS (${refusals.length})`);
for (const r of refusals) console.log(`  ${r.file}\n      ${r.reason}\n      ${r.snippet}`);

writeText(REFUSALS_OUT, `${JSON.stringify({ generatedBy: "tools/codemods/severity-sweep.mjs", mode, stats, refusals }, null, 2)}\n`);

if (mode === "check") {
  const remaining = [];
  for (const file of listFiles({ dirs: ["projects"], ext: [".ts"] })) {
    const rel = relative(file);
    if (rel.includes("/generated/") || rel.endsWith(".spec.ts")) continue;
    const src = readText(file).replace(/\/\*[\s\S]*?\*\//g, "").replace(/\/\/[^\n]*/g, "");
    const hits = [];
    if (/\w*Tag\["severity"\]/.test(src)) hits.push('Tag["severity"]');
    for (const n of LEGACY_TYPE_NAMES) if (new RegExp(`\\b${n}\\b`).test(src)) hits.push(n);
    if (hits.length) remaining.push(`${rel}  ${hits.join(", ")}`);
  }
  if (remaining.length > 0) {
    console.error(`\n--check FAILED: ${remaining.length} file(s) still carry a legacy severity type:`);
    for (const r of remaining) console.error(`  ${r}`);
    process.exit(1);
  }
  console.log("\n--check OK: UiBadgeIntent is the only severity vocabulary left.");
}
