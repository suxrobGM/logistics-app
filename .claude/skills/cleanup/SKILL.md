---
name: cleanup
description: Review and clean up the given file(s)/folder(s)/module(s): rate organization, find dead code, duplication, coupling, over-engineering, deep nesting, structural issues, and bad comments, then produce and execute a phased refactor plan. Trigger on "clean up X", "review and refactor X", "rate the code in X", "code quality review of X", "fix the comments in X".
metadata:
  version: "1.2"
---

# Code Cleanup

Review a target path (file, folder, module, or feature), then fix what you found. Judge everything against the project's own written rules (CLAUDE.md, rules files, lint config), not general taste. The argument is the target path(s). If none given, ask.

Limits: one review pass, one implementation pass, one question to the user. Do not add more.

## Findings list

One file in the scratchpad, one line per finding: `id | category | file:line | claim | evidence | status`. Status is CONFIRMED, UNCERTAIN, or KEPT. For any "unused" or "dead" claim, the evidence is the search that proved it. Search for indirect uses too: keys built from strings, `obj[key]` lookups, translation keys assembled at runtime. A claim without a search stays UNCERTAIN. KEPT records what was checked and deliberately left alone.

## Process

### 1. Map the target (you, no agents)

- List the target's files with line counts, largest first.
- If the project has a dead-code tool (knip, ts-prune, depcheck, an unused-imports lint rule), run it on the target first and seed the findings list from its output. Manual searching then covers only what the tool cannot see: keys built at runtime, translation keys, response fields.
- Search once for files outside the target that import from it. The names they import are the public API that moves and renames must keep working.
- Note connected files: routes or pages that render the target, providers, query functions and keys, locale files, docs or feature index entries. Search them for references only. Do not review them in full.
- Write a 5-line summary of the project rules from the docs already in context: file size limit, naming, comment policy, framework habits (if the framework already memoizes, as React Compiler does, manual memoization counts as an issue), and the check commands (typecheck, lint, tests). Paste it into every agent prompt.

### 2. Review

Read each target file once, fully. Record every finding as a line in the findings list. Skip a bullet only when it cannot apply.

**Unused code**: exports nothing imports, props never used or always given the same value, state set but never read, unreachable branches, unused translation keys, assets, or style properties, commented-out code, wrappers with a single caller that add no behavior, generics only ever used with one type, a config layer read from one place, query functions or keys nobody imports, interfaces, base classes, or strategy patterns with a single implementation, tests for removed code, duplicated tests, unused mocks, outdated docs or feature index entries.

**Repeated code and mixed patterns**: near-identical functions, components, markup, or style blocks; data reshaping that redoes an existing util (check the project's utils first); copy-pasted loading, empty, and error blocks; the same constant written inline in several places; two ways of solving one problem inside the target (switch all to the one the project uses most); files past the size limit or doing too many jobs.

**Control flow and function shape**: nested or chained ternaries, deep if/else that early returns would flatten, `else` after `return`, `if (x) return true; else return false`, negated conditions with swapped branches; boolean flag parameters that switch behavior (split the function), five or more parameters, different return shapes on different paths, one-line helpers called once (inline them); sequential awaits with no dependency between them, `.then` chains mixed with `await`, try/catch that only rethrows, `async` on functions that never await; `x ? x : y` where `??` fits, the same default applied at several layers, `null` and `undefined` both used for absence.

**Structure and data flow**: props passed through layers unchanged, many props that belong together in one object, the same data fetched or computed in several places, one request per item where a batch request exists, effects that copy data into state when it could be computed directly, lists that grow with no limit or pagination, a large library imported for one function, errors caught and ignored, missing error, loading, or empty states, timers or subscriptions never cleaned up, race conditions from outdated closures or missing awaits, `renderSomething()` helpers that should be components, markup nested four or more wrapper levels deep, importing another feature's internals, circular imports.

**Comments, types, naming, text**: comments that describe the next line, restate the name, mention tasks or PRs, or mark removed code; divider comments like `// ---- Helpers ----`; long comments that should be one line saying why; comments explaining what confusing code does (rename or extract instead, then delete the comment); comments that contradict the code. Names that promise one thing while the code does another, booleans that don't read as yes/no questions, one concept under two names in the module, abbreviations nobody else uses. `any` or `unknown` casts, `!` assertions where a type check would do, anonymous object types written inline in signatures, hand-written types that duplicate what the code already infers or generates. Files sitting in the folder root that belong in a subfolder, filename prefixes that repeat the folder name, index files that only re-export. Hardcoded user-facing text, translation keys missing in some locales, missing alt or aria attributes. Anything that breaks the project rules summary.

**Packages and API endpoints** (only when the target owns a package manifest, endpoints, or config): packages with zero imports, two libraries doing one job, endpoints no client calls, response fields no consumer reads, feature flags that are always on or always off, environment variables nobody reads.

Before a finding that deletes code becomes CONFIRMED, try to prove it wrong: a double render may be intentional, a "redundant" fetch may preload a cache on purpose. Before planning, re-run the search for a sample of the CONFIRMED deletions yourself.

### 3. Rate and plan

Score each subfolder (or the whole target, if it has none) out of 10 with a one-line reason.

Two phases, each with its own commit(s):

1. **Remove**: dead code and noise comments, then file moves and renames with `git mv` as a separate commit (never mix a move with a content edit).
2. **Refactor**: structural changes, pulling repeated code into shared helpers, then polish (types, translations, unexplained numbers, error handling, comment rewrites).

Skip a phase with nothing in it. Gather everything that needs the user's decision into one question, asked now: new architectural pieces (providers, contexts, shared layers), deleting anything still UNCERTAIN, renaming or moving files that other code imports, changes to what the user sees. Do not stop again after this.

### 4. Execute and report

Run the project's check commands (typecheck, lint, tests, taken from the project docs) once per phase, not per file. Test screens or flows by hand only when a change affects what the user sees. Report: the rating table, line count before and after, and findings fixed, KEPT, and still UNCERTAIN.

## When to use agents

- **Under ~10 files**: do everything yourself, no agents.
- **Larger**: two reviewer agents, started at the same time. Reviewer 1 takes unused code and packages (mostly searching). Reviewer 2 takes repeated code, control flow, structure, and comments/types/naming/text (mostly reading). Each gets the rules summary, the file list, its bullets, and the line format. Each verifies its own claims and reports back only finding lines. Never one agent per bullet or per file.
- **Implementation**: one agent runs both phases in order, running the checks and committing after each. Review the full set of changes once at the end, not per phase.
