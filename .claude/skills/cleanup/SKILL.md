---
name: cleanup
description: Review and clean up the given file(s)/folder(s)/module(s) without changing behaviour - rate organization, find dead code, duplication, over-engineering, deep nesting, oversized files and bad comments, then execute a phased refactor plan. Trigger on "clean up X", "refactor X", "split X", "deduplicate X", "simplify X", "rate the code in X", "fix the comments in X". The built-in `simplify` covers only the current diff.
---

# Cleanup

Review a target path, then fix what you find, judged against the repo's rules (`CLAUDE.md`, `.claude/rules/`, lint config), not taste. No target given: ask. Budget: one review pass, one implementation pass, one question to the user.

## Findings list

One scratchpad file, one line per finding: `id | category | file:line | claim | evidence | status` (CONFIRMED, UNCERTAIN, KEPT). An "unused" claim needs the search that proves it, including indirect uses (string-built keys, `obj[key]`, runtime translation keys). No search, no CONFIRMED.

## 1. Map

- Files with line counts, largest first.
- Run the dead-code tool first (`bun run knip` for Angular) and seed findings from it.
- Find importers outside the target; what they import is public API that must keep working.
- Note connected files (routes, providers, locale files, `feature-map.md` rows) for reference checks only.

## 2. Review

Read each file once. Look for:

- **Dead code:** unused exports, props, state, branches, keys, assets, tests and mocks; single-caller wrappers; single-type generics; single-implementation interfaces nobody substitutes (check `Substitute.For<IFoo>` first); stale docs.
- **Duplication:** near-identical functions, markup or style blocks; reimplemented utils; two ways of doing one thing (keep the repo's majority way). Extract at four copies, not three.
- **Control flow:** rules in `code-quality.md`, plus `else` after `return`, flag parameters, 5+ parameters, needless `async`, rethrow-only try/catch, sequential independent awaits, `null` and `undefined` mixed.
- **Structure:** prop drilling, repeated fetches, per-item requests where a batch exists, effects copying derivable state, unbounded lists, swallowed errors, missing loading/empty/error states, leaked subscriptions, deep wrapper nesting, reaching into another feature's internals.
- **Comments, names, types:** everything `code-quality.md` forbids; `any`, needless `!`, inline object types, hardcoded user-facing text, missing aria/alt.
- **LogisticsX:** manual `TenantId` filters, `.Include()`, navigation reads in mappers or loops (N+1), hand-copied DTOs where Mapperly exists, `FromSqlRaw` with concatenation (fix, don't tidy), static helpers taking a unit of work.

**Don't simplify away conventions:** one command/query + handler per operation, `internal sealed` handlers, repository/unit of work over `DbContext`, Mapperly, existing `partial` entity splits, `AuditableEntity`, thin validators, cascade deletes, generated code.

Before confirming a deletion, try to prove it wrong (a "redundant" fetch may warm a cache). Re-run the search for a sample of confirmed deletions yourself.

## 3. Rate and plan

Score each subfolder out of 10 with a one-line reason. Two phases, each its own commit(s):

1. **Remove:** dead code and noise comments; then moves/renames with `git mv` in a separate commit.
2. **Refactor:** structure, shared helpers, then polish.

Ask the one question now, covering: new architectural pieces, deleting UNCERTAIN items, renaming imported files, visible changes, any public-surface change (routes, DTOs, command records, selectors). Don't stop again.

## 4. Execute and report

Behaviour must not change. Run `dotnet build` / `dotnet test --filter-class`, `bun run lint` / `bun run check` once per phase. Update `feature-map.md` if a moved file had a row there. Report the ratings, line counts before and after, and findings fixed, KEPT and UNCERTAIN.

## Agents

Under ~10 files, work alone. Larger: two read-only reviewers in parallel (one for dead code, one for everything else), each given the rules summary, file list, its categories and the line format. Implementation is one agent, sequential, in the main checkout, committing per phase without pushing.
