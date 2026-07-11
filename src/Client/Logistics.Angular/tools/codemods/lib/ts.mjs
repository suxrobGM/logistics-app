/**
 * ts.mjs — small, conservative TypeScript source edits.
 *
 * Scope note: these are regex-with-verification helpers, NOT a TS refactoring engine. The TS side of
 * this migration (swapping component imports, deleting CVA boilerplate, rewriting constructors) is
 * done by agents reading the file, because it needs judgement. These helpers exist only for the
 * mechanical import bookkeeping that follows a template codemod — the part that is boring, high
 * volume, and easy to get subtly wrong by hand.
 *
 * Every function here VERIFIES before it edits and returns the source UNCHANGED if the shape is not
 * exactly what it expected. Same contract as the template codemods: refuse, never guess. Each
 * returns { src, changed, reason } so the caller can record a refusal.
 */

const ok = (src, changed, reason = null) => ({ src, changed, reason });

/** Escape a string for use inside a RegExp. */
const esc = (s) => s.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");

/**
 * Rewrite the module specifier of an import: `from 'primeng/button'` -> `from '@lib/ui'`.
 * Leaves the imported specifiers alone. No-op (changed:false) if `from` is not imported.
 */
export function replaceImport(src, from, to) {
  const re = new RegExp(`(from\\s*['"])${esc(from)}(['"])`, "g");
  if (!re.test(src)) return ok(src, false, `no import from '${from}'`);
  return ok(src.replace(re, `$1${to}$2`), true);
}

/**
 * Remove one named specifier from an import statement.
 *   - if other specifiers remain, only that specifier goes
 *   - if it was the last one, the whole import statement (and its trailing newline) goes
 * Refuses on `import * as x` / default imports / type-only edge shapes it does not understand.
 */
export function removeImportSpecifier(src, module, specifier) {
  const stmt = new RegExp(
    `import\\s+(type\\s+)?\\{([^}]*)\\}\\s*from\\s*['"]${esc(module)}['"];?[ \\t]*\\r?\\n?`,
    "g",
  );
  const match = stmt.exec(src);
  if (!match) return ok(src, false, `no braced import from '${module}'`);

  const names = match[2]
    .split(",")
    .map((s) => s.trim())
    .filter(Boolean);

  if (!names.some((n) => n === specifier || n.startsWith(`${specifier} as `))) {
    return ok(src, false, `'${specifier}' not imported from '${module}'`);
  }

  const kept = names.filter((n) => n !== specifier && !n.startsWith(`${specifier} as `));
  const typeKw = match[1] ?? "";
  const replacement =
    kept.length === 0 ? "" : `import ${typeKw}{ ${kept.join(", ")} } from '${module}';\n`;

  return ok(
    src.slice(0, match.index) + replacement + src.slice(match.index + match[0].length),
    true,
  );
}

/**
 * Add a symbol to a standalone component's `imports: [...]` array.
 *
 * Refuses (changed:false) unless it finds exactly one `@Component({...})` with an `imports: [` array,
 * because a file with two decorators, or one whose imports array is spread/computed
 * (`imports: [...BASE]`), is a shape this helper does not understand — and a template that renders
 * `<ui-icon>` without the import fails at build time, which is the outcome we want over a silent
 * mangling. Idempotent: a no-op when the symbol is already in the array.
 */
export function addToComponentImports(src, symbol) {
  const decorators = [...src.matchAll(/@Component\s*\(\s*\{/g)];
  if (decorators.length === 0) return ok(src, false, "no @Component decorator");
  if (decorators.length > 1) return ok(src, false, "multiple @Component decorators");

  // Find `imports: [` after the decorator opens, then walk to its matching `]`.
  const open = src.indexOf("imports:", decorators[0].index);

  // No imports array at all — a standalone component that so far needed none. Create one right after
  // `templateUrl:`, which every component in this repo has (templateUrl-only is a repo convention).
  if (open === -1) {
    const tpl = /templateUrl:\s*["'`][^"'`]+["'`],/.exec(src.slice(decorators[0].index));
    if (!tpl) return ok(src, false, "@Component has neither an imports array nor a templateUrl to anchor one");
    const at = decorators[0].index + tpl.index + tpl[0].length;
    return ok(`${src.slice(0, at)}\n  imports: [${symbol}],${src.slice(at)}`, true);
  }

  const lb = src.indexOf("[", open);
  if (lb === -1) return ok(src, false, "imports is not an array literal");

  let depth = 0;
  let rb = -1;
  for (let i = lb; i < src.length; i++) {
    if (src[i] === "[") depth++;
    else if (src[i] === "]") {
      depth--;
      if (depth === 0) {
        rb = i;
        break;
      }
    }
  }
  if (rb === -1) return ok(src, false, "unbalanced imports array");

  const body = src.slice(lb + 1, rb);
  if (body.includes("...")) return ok(src, false, "imports array uses a spread");

  const names = body
    .split(",")
    .map((s) => s.trim())
    .filter(Boolean);
  if (names.includes(symbol)) return ok(src, false, "already in imports");

  const merged = [...names, symbol].sort((a, b) => a.localeCompare(b));
  // Re-emit flat; prettier re-wraps it to the project's width.
  return ok(`${src.slice(0, lb + 1)}${merged.join(", ")}${src.slice(rb)}`, true);
}

/**
 * Add a named specifier to an import from `module`, merging into an existing import when one is
 * present, otherwise inserting a new statement after the last import in the file.
 * Idempotent: if the specifier is already imported from that module, it is a no-op.
 */
export function addImport(src, specifier, module) {
  const stmt = new RegExp(
    `import\\s+(type\\s+)?\\{([^}]*)\\}\\s*from\\s*['"]${esc(module)}['"];?`,
    "g",
  );
  const match = stmt.exec(src);

  if (match) {
    const names = match[2]
      .split(",")
      .map((s) => s.trim())
      .filter(Boolean);
    if (names.includes(specifier)) return ok(src, false, "already imported");

    const merged = [...names, specifier].sort((a, b) => a.localeCompare(b));
    const replacement = `import ${match[1] ?? ""}{ ${merged.join(", ")} } from '${module}';`;
    return ok(
      src.slice(0, match.index) + replacement + src.slice(match.index + match[0].length),
      true,
    );
  }

  // No existing import from this module — insert after the last import statement in the file.
  const imports = [...src.matchAll(/^import\s[^\n]*?;[ \t]*$/gm)];
  const line = `import { ${specifier} } from '${module}';`;
  if (imports.length === 0) return ok(`${line}\n\n${src}`, true);

  const last = imports.at(-1);
  const at = last.index + last[0].length;
  return ok(`${src.slice(0, at)}\n${line}${src.slice(at)}`, true);
}
