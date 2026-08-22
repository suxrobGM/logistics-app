---
name: cleanup
description: Review and clean up the given file(s)/folder(s)/module(s): rate organization, find dead code, duplication, coupling, over-engineering, deep nesting, structural issues, and bad comments, then produce and execute a phased refactor plan. Trigger on "clean up X", "review and refactor X", "rate the code in X", "code quality review of X", "fix the comments in X".
metadata:
  version: "1.1"
---

# Code Cleanup

Review-then-refactor of a target path (file, folder, module, or feature). Judge everything against the project's own written rules, not generic taste. The argument is the target path(s). If none given, ask.

## Working files

Create both in the scratchpad before hunting. They are the shared state between the orchestrator, subagents, and phases.

- **Conventions brief** (10 to 15 lines): a short summary of CLAUDE.md, rules files, and lint/formatter config. Must name the size ceiling, naming scheme, comment policy, and framework patterns (an auto-memoizing compiler makes manual memoization a finding, not a virtue). Paste it into every subagent prompt so no agent re-reads the docs.
- **Findings list**: one line per finding in the form `id | category | file:line | claim | evidence | status`. Status is one of CONFIRMED, UNCERTAIN, KEPT. Hunters add lines, verification settles them, phases act on them. KEPT records what was checked and deliberately left alone, so the next pass does not question it again.

## Process

### 1. Inventory and map (run in parallel)

- List the target's files with line counts, largest first. Write the conventions brief.
- Map consumers: grep for every importer outside the target. Record which symbols cross the boundary. That is the public surface that moves and renames must keep working. Map the reverse too: what the target reaches into. Flag misfiled code (judge by who uses it and what data it uses, not where it sits).
- List the **nearby plumbing**: routes/pages that render the target, providers, query functions/keys/data hooks, shared libs it owns, locale files, feature-map/docs entries. Plumbing is part of the review, not background. Dead code hides there.

### 2. Hunt through five checks

Record every finding as file:line and add it to the findings list. Skip a bullet only when it cannot apply (no locale files means no i18n bullet). For the N largest files (target and plumbing), read line by line even when under the size ceiling: big files tend to collect too many jobs.

**Check A: dead weight**

- **Dead code**: exports with zero importers, props never used or always passed the same constant, state set but never read, unreachable branches, i18n keys/assets nothing uses, dead barrel exports, dead style properties, commented-out code.
- **Over-engineering**: wrappers with one consumer and no behavior, generics used with only one type, an extra config layer with a single reader, options nobody passes. Record deliberate keeps as KEPT.
- **Plumbing rot**: query functions with zero importers (callers went direct), unused query keys, locale keys left behind by UI changes, stale feature-map/docs entries, pass-through layers that only forward data. Ask whether each plumbing layer is worth keeping.

**Check B: duplication and consistency**

- **Duplication**: near-identical components/functions, repeated JSX or style blocks, data-reshaping code that redoes what an existing util does (check the project's utils first), copy-pasted loading/empty/error shells, repeated inline constants.
- **Missing extractions**: files with too many jobs, no shared helper where 3+ siblings repeat a pattern, god-files past the project's size ceiling.
- **Consistency**: two patterns solving the same problem within the target (mixed dialog patterns, mixed data-fetch patterns, mixed styling). Find the pattern the project uses most and switch everything to it. Near-identical route files differing in a handful of values count here.

**Check C: architecture and flow**

- **Coupling and drilling**: props passed through layers unchanged, pass-through components, N-prop bundles that should be one object, the same object fetched or computed in many places, code that reaches into another feature, circular imports.
- **Data flow and performance**: N+1 requests where a batch exists, the same data fetched by multiple siblings, effects that copy data into state when it can be computed during render, render-time work that belongs in the data layer, lists that grow without limit and have no cleanup or pagination, heavy imports for one function.
- **Error handling**: swallowed errors that make failure look like a valid state, missing error/loading/empty states, different handling across siblings, missing cleanup of timers/subscriptions/observers, race conditions from stale closures or missing awaits.
- **Deep nesting**: subcomponents in the same file that grew too big (split past the size ceiling OR when a subcomponent gains a second consumer, not by default), `renderX()` closures that should be components, JSX nested 4+ wrapper levels deep.

**Check D: surface quality**

- **Comment quality**: delete comments that describe the next line, restate the name, reference tasks/PRs, or mark removed code. Delete section banners and boxed dividers ("// ---- Helpers ----", "// Step 1"); if a file needs signposts, it needs extraction instead. Shorten long multi-line comments to their one-line WHY. Rewrite comments full of jargon or hard to follow in plain words a newcomer can understand. A comment that contradicts the code is a finding either way: fix the comment, or mark the code UNCERTAIN if the comment looks right. If a comment explains WHAT confusing code does, rename or extract until the comment is not needed, then delete it.
- **Type quality**: `any`/`unknown` casts, non-null assertions where narrowing works, inline anonymous types in signatures, hand-written types duplicating inferred/generated/derived ones.
- **Structure and naming**: loose root files, folder names that don't match contents, redundant filename prefixes (the directory is the namespace), trivial barrels, public surface exposing internals.
- **i18n and accessibility**: hardcoded user-facing strings, keys missing in some locales, hand-rolled date/number formatting where locale-aware helpers exist, missing alt/aria on interactive or image elements.
- **Convention violations**: anything that breaks the conventions brief, judged strictly against it.

**Check E: dependencies and surface area** (only when the target owns a package manifest, API endpoints, or config)

- **Dependency cleanup**: packages in the manifest with zero imports, two libraries doing the same job (keep the one the project uses most), a heavy dependency imported for one function that a platform API or existing util replaces.
- **Unused API surface**: endpoints no client calls, DTO/response fields no consumer reads, responses that return more data than callers use. Grep the client side for each endpoint and field before claiming it dead.
- **Stale flags and config**: feature flags always on or always off, env vars nobody reads, config options with one hardcoded value everywhere, settings whose reader was deleted.

### 3. Verify before rating

- **Batch the greps.** Collect every "unused" or "dead" claim from the findings list and check them in one sweep, not one grep at a time. Grep for dynamic access too: string-built keys, `obj[key]` lookups, i18n key construction. Static import greps miss these.
- **Try to disprove the risky ones.** For every finding that deletes code or touches behavior, try to disprove it: a visual double-render may be intentional (current vs next state), a "redundant" fetch may be a cache warmer. When fanned out, give the disprove batch to a fresh skeptic agent that did not write the findings. Only findings that survive this become CONFIRMED. Everything unproven stays UNCERTAIN and blocks the phase that touches it.

### 4. Rate

Score each subfolder /10 with a one-line reason. This makes the review easy to scan and points effort at the lowest scores.

### 5. Plan in phases, where each phase builds and commits on its own

0. **Delete dead code and noise comments** (first, so later phases touch less).
1. **Pure moves/renames** via `git mv`: zero logic change, so history follows and review is trivial. Never mix moves with logic edits in one commit.
2. **Structural changes**: contexts/providers, API reshaping, drilling removal.
3. **Dedupe extractions**: shared shells, hooks, helpers.
4. **Polish**: types, i18n, magic numbers, error handling, comment rewrites.

### 6. Execute and verify

After each phase, run the project's own commands (typecheck/lint/tests, read from project docs, don't guess). Run them once per phase, not per file. After all phases, test the affected screens or flows end to end. Close with a short report: the rating table, LOC change, findings fixed vs KEPT vs still UNCERTAIN.

## Scale

- Target under ~10 files: run steps 1 to 3 inline, all checks yourself.
- Larger: fan out hunter subagents, one per check that applies (A to E), in a single parallel batch. Each prompt gets the conventions brief, the file list, its check's bullets, and the finding line format. Hunters return finding lines only. Do not spawn one agent per bullet: that splits the reading into too many small pieces. Consumer mapping runs as a fifth parallel agent during step 1.
- Verification: send the batched risky findings to one or two skeptic agents (step 3). Dedupe findings by file:line before checking them.
- Implementation: hand each phase to a subagent, one at a time. The orchestrator reviews each diff, runs verification, and commits before starting the next phase.

## Ask the user before

- Introducing new architectural pieces (providers, contexts, new shared layers).
- Deleting anything still marked UNCERTAIN.
- Renaming or moving files consumed outside the target.
- Any fix that changes user-visible behavior (error rendering, empty states, labels).
