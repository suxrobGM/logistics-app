---
name: cleanup
description: 'Review and clean up the given file(s)/folder(s)/module(s) in any language or framework: rate organization, find dead code, duplication, coupling, over-engineering, structural and architectural problems, then produce and execute a phased refactor plan. May apply architecture/pattern changes when they are a net simplification. Trigger on "clean up X", "review and refactor X", "rate the code in X", "code quality review of X".'
---

# Code Cleanup

Systematic review-then-refactor of a target path (file, folder, module, package, or feature) in **any language or framework**. Every judgment is calibrated against the host project's own conventions and the ecosystem's idioms, not generic taste. Behavior-preserving by default; architecture and pattern changes are **in scope** when they pass the net-simplification test (below). The argument is the target path(s); if none given, ask.

## Process

### 1. Calibrate

- Detect the stack: languages, package manifests, build system, framework(s), and the project's own verification commands (typecheck/compile, lint, tests, formatter). Read them from project docs and manifests - never guess. These commands are the gate for every later phase.
- Read the project's conventions FIRST - CLAUDE.md/AGENTS.md, rules files, lint/formatter config, editorconfig. Note the size ceiling, naming scheme, comment policy, and framework idioms (e.g. an auto-memoizing compiler makes manual memoization a finding, not a virtue; a DI container makes `new` in handlers a finding).
- Inventory the target's files with line counts, largest first.
- Assess the safety net: does the verification harness actually cover the target? If tests are thin, prefer low-risk phases, lean harder on adversarial verification, and consider adding characterization tests before risky structural work.

### 2. Map the boundary

- Find every consumer outside the target (grep the target's path/package/symbol names). Record which symbols cross the boundary - the public surface that moves/renames must preserve. If the target is a published library, the surface includes consumers you cannot see: treat its exported API as frozen unless the user says otherwise.
- **Invisible callers**: enumerate symbols invoked without an import - framework-registered routes/handlers/lifecycle hooks, DI/IoC registrations, reflection and dynamic dispatch (`getattr`, `Method.invoke`, message selectors), serialization/ORM field names, config- or convention-referenced classes, CLI entry points, FFI exports, template references, scheduled jobs, migrations. Grep alone cannot prove these dead.
- Map the reverse direction (what the target reaches into) and flag misfiled code: anything inside the target consumed only by a different feature, judged by who calls it and whose data it touches.
- Enumerate **adjacent plumbing** - out-of-tree files that wire the target in: routes/pages/handlers that mount it, DI wiring, build/config entries, data-access helpers and cache keys, localization files, docs/feature-map entries, CI steps. Plumbing is review scope, not just context: dead code hides there, and moves inside the target often require updating it.

### 3. Hunt - parallel fan-out, every finding cited as `file:line`

Lenses, phrased stack-neutrally (translate each to the detected ecosystem):

- **Dead code** - exports/functions with zero callers, parameters never used or always passed the same constant, state written but never read, unreachable branches, orphaned assets/locale keys/config entries, re-export indirection nobody imports through, commented-out code, feature flags whose losing branch shipped long ago.
- **Duplication** - near-identical functions/types/templates, repeated data-massaging that reimplements an existing util or stdlib call (check the project's utils first), copy-pasted error/empty/loading shells, repeated inline constants (thresholds, colors, magic numbers, key lists).
- **Coupling** - values threaded through layers unchanged (prop drilling, parameter plumbing, context objects passed everywhere), pass-through wrappers, N-argument bundles that should be one object, the same thing fetched/derived in many places, cross-feature reach-ins into another module's internals, circular imports, feature envy (a function that mostly manipulates another module's data).
- **Over-engineering** - wrappers with one consumer and no behavior, generics/interfaces/traits with one instantiation, config indirection with a single reader, plugin points nobody plugs into, options nobody passes, speculative "future-proofing". Record what was evaluated and deliberately KEPT so the next pass doesn't re-litigate.
- **Under-abstraction** - the inverse: god-files/classes past the project's ceiling, the same concept implemented twice, missing extraction where 3+ siblings repeat a pattern.
- **Architecture & patterns** - the design itself is the wrong shape: layers that only forward calls, a pattern mismatched to the problem (inheritance where composition fits, singleton hiding dependencies, event indirection between two fixed parties, sync/async or push/pull mismatch), module boundaries that force shotgun surgery (one conceptual change = edits in many files), abstractions inverted from the dependency direction the domain wants, state owned in the wrong place. Propose the replacement shape, not just the complaint.
- **Structure & naming** - loose root files, folder names that don't match contents, redundant filename prefixes (the directory is the namespace), inconsistent conventions within a folder, trivial re-export indirection, public surface exposing internals.
- **Contract quality** - in typed languages: `any`/casts/non-null assertions where narrowing works, inline anonymous types in signatures, hand-written types duplicating inferred/generated ones. In dynamic languages: missing validation at trust boundaries, stringly-typed dispatch, dicts-as-structs where the ecosystem has a record idiom (dataclass, Struct, TypedDict).
- **Data flow & performance** - N+1 calls (per-item queries/requests where a batch exists), the same data fetched or computed by multiple siblings, stored state where derivation works, work done per-render/per-request that belongs at a colder layer, unbounded collections without eviction/pagination, heavy imports for one function, sequential awaits on independent operations that should run concurrently.
- **Error handling & resilience** - swallowed errors masking failure as a valid state, missing error/empty paths, inconsistent error strategy across siblings, missing cleanup of timers/handles/subscriptions/connections, race conditions from stale closures or unawaited sequencing, missing timeouts on external calls.
- **Consistency** - two patterns solving the same problem within the target (mixed data-access idioms, mixed styling, mixed dialog/CLI-output/logging patterns): identify the project-dominant idiom and converge on it.
- **Convention violations** - breaches of the project's own written rules (comments, naming, styling, framework idioms), judged strictly against the docs read in step 1.
- **Micro-simplification** - line-level shrink: early-return/invert-if to kill nesting, redundant conditionals (`if (x) return true; return false`), boolean-flag parameters that should be two functions, switch/if-chains that should be lookup tables, loops reimplementing map/filter/stdlib, needless `else` after return, needless async/wrapping.
- **Dependency hygiene** - unused dependencies in the manifest, two libraries doing the same job (two HTTP clients, two date libs), a heavy dependency used for one function the stdlib covers, vendored copies of what a dependency provides, polyfills/compat shims for environments no longer supported, deprecated APIs with a drop-in modern replacement.
- **Test suite** - tests are code: permanently-skipped tests, duplicated setup that should be fixtures/helpers, tests pinning implementation details so refactors churn them, over-mocked tests that only exercise the mocks, dead test helpers, assertions that can't fail.
- **Nesting & layering** - files housing multiple internal units that outgrew co-location (split when the file passes the size ceiling OR a unit gains a second consumer - otherwise co-location is good; don't split reflexively), helper closures that should be named units, call chains where each layer adds only forwarding.
- **Adjacent plumbing** - run the dead-code, duplication, and consistency lenses over the plumbing from step 2: helpers with zero callers (callers went direct), unused cache/config keys, near-identical wiring files differing in a handful of values, mappings re-hardcoded per call site instead of using the target's own helpers, locale keys orphaned by UI changes, stale docs entries.
- **Large & plumbed files** - rank the N largest files (target AND plumbing) and read them line-by-line even when under the ceiling: size correlates with responsibility accumulation. Separately ask of each pure-plumbing layer whether it earns its existence.

**Fan-out**: target ≤ ~10 files - hunt inline. Larger - dispatch parallel Explore/read-only subagents, each owning a **grouped bundle of lenses** over the whole target (e.g. ① dead code + over-engineering + dependency hygiene, ② duplication + consistency + convention violations, ③ coupling + data flow + nesting + micro-simplification, ④ architecture + under-abstraction + error handling + contract quality, ⑤ plumbing + large files + test suite). One agent per lens over-fragments the reading; one agent for everything loses the benefit of independent angles. Run the consumer/boundary map (step 2) as its own agent in the same batch. Each agent returns findings as `file:line - claim - evidence`, plus a keep-list of things it considered and cleared.

### 4. Rate

Score each subfolder (or file, for small targets) /10 with a one-line justification. This makes the review scannable and directs refactor effort to the lowest scores.

### 5. Verify adversarially

Findings from a single reader are hypotheses, not facts.

- Grep every "unused" claim yourself before scheduling a deletion; for anything on the invisible-callers list from step 2, demand positive evidence of deadness (e.g. the registration is itself dead), not absence of imports.
- For risky findings - deletions, architecture changes, behavior-adjacent edits - spawn skeptic subagents prompted to **refute** the finding, not confirm it. A finding survives only if the skeptic fails to kill it. Batch skeptics in parallel; one skeptic can take several related findings.
- A visual or structural double may be intentional (current-state vs next-state, A/B arms, per-tenant variants) - check semantics, not just similarity.
- Mark anything unproven UNCERTAIN; it does not enter a phase until resolved or explicitly approved by the user.

### 6. Plan in phases - each independently buildable and committable

1. **Delete dead code** (first, so later phases touch less).
2. **Pure moves/renames** via `git mv` - zero logic change, so history follows and review is trivial. Never mix moves with logic edits in one commit.
3. **Architecture & pattern changes** - reshape the design per the surviving architecture findings. Each must pass the **net-simplification test**: after the change there are fewer concepts, fewer layers, or fewer places to edit for a known kind of future change, and the diff's churn is proportionate to that win. A restructure that merely trades one shape for an equally complex one fails the test - drop it. Stage big reshapes as a sequence of small, individually-green commits (strangler-style: introduce the new seam, migrate callers, delete the old shape) rather than one big-bang diff.
4. **Dedupe extractions** - shared shells, helpers, hooks/mixins/traits.
5. **Smells & polish** - contracts/types, magic numbers, error handling, naming, consistency convergence.

Order within each phase by blast radius, smallest first. If the safety net is thin (step 1), pull characterization tests forward as phase 0 for anything phase 3 will reshape.

### 7. Execute - orchestrator + implementer subagents

- Small targets: implement inline, phase by phase.
- Larger: delegate each phase to an implementer subagent with the exact finding list, the conventions from step 1, and the public-surface freeze list from step 2. Phases run **sequentially** (each builds on the last commit); within a phase, split across parallel subagents only when their file sets are disjoint.
- After each phase the orchestrator - not the implementer - reviews the diff, runs the project's verification commands, and commits before dispatching the next phase. A red gate stops the line: fix or revert before proceeding, never stack a phase on a broken base.

### 8. Verify and report

After all phases: run the full gate once more, then exercise the affected flows end-to-end the way a user or caller would (run the app/CLI/tests-of-consumers, not just the compiler). Close with a short report: rating table (before scores), what changed per phase, net metrics (files, LOC, exports/public symbols before → after), findings deliberately kept, and anything left UNCERTAIN or out of scope.

## Ask the user before

- Applying an architecture change whose net-simplification case is arguable, or that alters an API consumed outside the repo.
- Deleting anything still marked UNCERTAIN.
- Renaming/moving files consumed outside the target.
- Any fix that changes externally observable behavior (error rendering, empty states, wire formats, CLI output, labels).

Everything else - including net-simplifying pattern/architecture changes that preserve behavior and the public surface - proceeds without asking.
