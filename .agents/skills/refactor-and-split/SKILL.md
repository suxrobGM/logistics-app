---
name: refactor-and-split
description: Refactor existing code without changing behavior — split oversized files, extract duplicates, simplify over-engineered patterns. Use when files exceed size thresholds, code is duplicated 4+ times, or has speculative abstractions, dead options, or wrapper indirection. Triggers "refactor", "split", "deduplicate", "simplify", "untangle", "clean up". Complements built-in `simplify` (which only reviews recent diffs).
---

# Refactor, Split, and Simplify

Proactive cleanup of existing code. Three things are in scope:

1. **Split** — break oversized files into smaller cohesive ones
2. **Deduplicate** — extract recurring patterns into shared helpers
3. **Simplify** — remove speculative abstractions, dead options, wrapper indirection, and impossible-state defensive code

Behavior must not change. Verify by running tests after each step.

## When this skill applies

Trigger this skill when **any** of these are true:

- A class file exceeds the size threshold for its kind (see below)
- Three or more places have nearly identical code that's diverged subtly
- A file mixes unrelated concerns (entity + factory + status machine + events all in one)
- Code has signs of over-engineering (see "Simplification smells" below)
- The user asks to "refactor", "split", "deduplicate", "simplify", "untangle", "clean up", "remove dead code"

Don't use it as a generic style polish — that's what linters and `simplify` (built-in) are for.

## Size thresholds (project-specific)

| File kind                | Soft threshold | Hard threshold |
| ------------------------ | -------------- | -------------- |
| Domain entity            | 150 lines      | 250 lines      |
| EF configuration         | 100 lines      | 200 lines      |
| Command/Query handler    | 200 lines      | 400 lines      |
| Controller               | 250 lines      | 400 lines      |
| Application service      | 300 lines      | 500 lines      |
| Infrastructure service   | 400 lines      | 600 lines      |
| Angular component (.ts)  | 200 lines      | 350 lines      |
| Angular template (.html) | 150 lines      | 300 lines      |
| Static utility class     | 200 lines      | 400 lines      |

Above the **soft** threshold = consider splitting if the file mixes cohesive concerns. Above the **hard** threshold = split now.

## Step-by-step

### 1. Read the file fully and inventory its concerns

Before any edit, identify the **distinct concerns** the file mixes. A file can be long without being bad — what matters is whether it does one thing.

For C# entities the existing project pattern is `partial class` split by concern:

- `Load.cs` — properties, constructor, navigation
- `Load.Status.cs` — status transition methods
- `Load.Factory.cs` — static factory methods
- `Load.Events.cs` — domain event raising

For services, split by **interface method group** or **side-effect domain** (e.g., `StripeSubscriptionService` vs `StripeConnectService` vs `StripeCustomerService`).

For Angular components, split a large component into:

- The component (state + template hookups)
- A separate `*-store.ts` for `@ngrx/signals` state
- Sub-components for repeated template fragments
- Pure helper functions in a sibling `*.utils.ts`

### 2. Plan the change before touching code

Write the proposed file boundaries / extractions / simplifications in your reply. The user gets a chance to redirect before you make 5 edits that are hard to review together.

Example plan output:

```
Plan: split Load.cs (412 lines) into 4 partials and remove dead options:
- Load.cs (90): properties, constructor, navigations
- Load.Status.cs (140): UpdateStatus / Dispatch / ConfirmPickup / etc.
- Load.Factory.cs (45): Create methods
- Load.Events.cs (60): RaiseProximityChangedEvent and event helpers
- Drop unused `IsSplitLoad` property (no readers, no migration plan)
- LoadStatusMachine.cs already lives separately — leave alone
```

### 3. Choose the mechanism per change

#### Split mechanisms

| Mechanism                         | When to use                                                      |
| --------------------------------- | ---------------------------------------------------------------- |
| `partial class Foo` across files  | C# entities, services with distinct concerns                     |
| Extract sibling class             | Concern is reusable beyond the original (e.g., a status machine) |
| Extract pure static helper        | Logic has no dependencies, reusable across handlers              |
| Move to a different project       | Concern crosses architectural layers (rare, requires care)       |
| Sub-components / child components | Angular templates with repeated complex blocks                   |
| `@ngrx/signals` store extraction  | Component holding 5+ signals or non-trivial derived state        |

#### Simplification mechanisms (see next section for what to look for)

| Mechanism                             | When to use                                                      |
| ------------------------------------- | ---------------------------------------------------------------- |
| Inline single-call helper             | Helper is called once and isn't named more clearly than its body |
| Drop unused parameter / property      | Tooling confirms zero readers across the solution                |
| Replace interface with concrete class | One implementation, no test double in use, no DI substitution    |
| Remove unreachable branch             | Type system or earlier guard makes the branch impossible         |
| Replace LINQ chain with simple loop   | The loop is shorter, more readable, and not on a hot path        |
| Collapse pass-through wrapper         | Wrapper just delegates without adding behavior                   |
| Drop async                            | Method has no `await` and isn't part of an async interface       |

### 4. Identify duplicates first

Before splitting, scan for near-duplicates that should be extracted:

```bash
# Quick same-line search
grep -rn "the obvious duplicated string" src/Core/

# Look for similar method shapes by signature
grep -rn "public async Task<.*> Handle.*Command.*Cancellation" src/Core/Logistics.Application/Commands/
```

If duplicates exist, extract them **before** splitting — that way each split partial pulls from one canonical source.

### 5. Make the edits in small, verifiable steps

Each step ends with a build/test green light:

1. Simplify (drop dead code, inline single-use helpers, collapse wrappers) — build + test
2. Extract duplicates into a shared helper — build + test
3. Split file into partials, no logic changes — build + test
4. (Optional) Move now-isolated concerns to better-fitting projects — build + test

If a step would touch 10+ files, stop and break it further.

### 6. Behavior preservation checks

After every refactor:

- `dotnet build` — compiles
- Relevant test slice passes (`dotnet test --filter "{Class}Tests"`)
- For Angular: `bun run lint` and the page renders (eyeball check)
- No `using` becomes unused — clean imports
- No public API surface changed (DTOs, controller routes, MediatR command/query records all stable)

If a refactor _requires_ changing public surface, that's no longer behavior-preserving — flag it and ask the user before continuing.

## Simplification smells

Code shapes that almost always benefit from simplification. Each item lists what to look for and what to do.

### Speculative abstractions

- **One-implementation interface with no test double** — a `IFooService` with a single `FooService` impl, never substituted in tests, never registered with multiple implementations. **Action:** delete the interface, depend on the concrete class. Bring it back when a second impl actually lands.
- **Factory with one producer** — `IFooFactory.Create()` that always `return new Foo(...)`. **Action:** inline the constructor at the call site.
- **Generic method used with one type** — `T Map<T>(...)` where every call site passes the same `T`. **Action:** replace with the concrete type.
- **Strategy / Pattern with one strategy** — a strategy interface, one implementation, no plan to add a second. **Action:** collapse into a single class.

### Indirection without value

- **Pass-through wrapper service** — class A calls only into class B with no transformation, validation, or extra behavior. **Action:** delete A, depend on B.
- **Repository on top of UnitOfWork on top of DbContext** — adding a third layer when two suffice. **Action:** stop at the level that already exists in this project (`IRepository<T>` over `IUnitOfWork`).
- **Mapper that copies fields 1:1** when Mapperly already does it — the manual mapper isn't using `MapperIgnoreSource` or transformation logic. **Action:** delete the manual mapper, let Mapperly generate it via `[Mapper]`.

### Defensive code for impossible states

- **`if (param is null)` for a non-nullable parameter** the C# compiler already forbids — **Action:** delete the check.
- **Re-validating a DTO that FluentValidation already validated** at the controller boundary — **Action:** delete the duplicated checks; validation lives in the validator.
- **`try/catch` that catches a type the call cannot throw** — **Action:** drop the try/catch.
- **`try/catch` that swallows or rethrows the same exception** — **Action:** delete the try/catch entirely.

### Dead options

- **Configuration class field with no reader** — `XOptions.Foo` set in appsettings, never injected anywhere. **Action:** remove the field; remove from appsettings.
- **Method parameter never used inside the method** — **Action:** delete the parameter; update call sites.
- **Unused public methods** — search the solution for callers; if none, **delete**. (Be careful with `internal sealed` types whose callers are tests — those are alive.)
- **Unused enum values** — verify with `Find Usages` before removal; some are referenced by string in JSON / config.

### Async theatre

- **`async` method without `await`** that isn't part of an async interface — **Action:** drop `async`, return `Task.FromResult(...)` or change the signature to sync.
- **`await Task.FromResult(...)` chains** — **Action:** unwrap, return directly.
- **`.Result` / `.Wait()`** in async context — these block the thread pool. **Action:** propagate `await`.

### Mapping / construction noise

- **Object initializers that re-set the property to its default** — `new Foo { Status = LoadStatus.Draft }` when `Draft` is the field's default. **Action:** drop the initializer line.
- **Manual property-by-property copy** when Mapperly is already wired — **Action:** use `entity.ToDto()` and override only the computed fields with `with { ComputedField = ... }`.
- **Building a DTO inside a handler when a Mapperly mapper already exists** — **Action:** call the mapper.

### Comments

- **Comments that restate the identifier**: `// Get user by id` above `public Task<User> GetUserByIdAsync(...)`. **Action:** delete.
- **Comments referencing a fixed bug** ("// Workaround for issue #123, fixed in v2.4") — **Action:** delete; the fix is in git history.
- **`// removed code` comments** — **Action:** delete; git knows.
- **Multi-paragraph docstrings on internal types** that no consumer reads — **Action:** trim to one line max, or delete.

### LINQ that obscures intent

- **`.ToList().ForEach(x => ...)` after a deferred query** — **Action:** plain `foreach` loop. Materialize once.
- **Nested `Select` + `SelectMany` + `ToList` chains** longer than 3 operators on a hot path — **Action:** consider rewriting as an explicit loop with named locals.
- **`.Where(x => x != null).Select(x => x!.Foo)` in a list-of-nullable** — **Action:** `OfType<T>()` removes the bang and the predicate.

### Project-specific (LogisticsX)

- **Manual tenant filter** like `.Where(x => x.TenantId == tenantId)` — `TenantDbContext` already scopes queries per tenant. **Action:** drop the redundant `Where`.
- **`.Include(...)` for navigation properties** — lazy loading is enabled. **Action:** delete the `Include`; let lazy load fire.
- **MediatR `Send` from inside another handler in the same module** when a service call would do — usually fine to keep (convention), but flag if the chain is 3+ deep.
- **String-concatenated SQL in `FromSqlRaw`** — security smell. **Action:** convert to `FromSqlInterpolated`. Don't just simplify — fix.

## Duplication patterns to watch for

LogisticsX-specific patterns that recur and are worth de-duplicating:

| Pattern                                                           | Extract to                                         |
| ----------------------------------------------------------------- | -------------------------------------------------- |
| Same `where` filter used in 3+ specifications                     | A base spec or `IQueryable` extension              |
| Mapping logic outside Mapperly                                    | Mapperly mapper in `Logistics.Mappings`            |
| Same auth / tenant validation in multiple controllers             | Custom `[AuthorizeAttribute]` or pipeline behavior |
| HTTP error → DTO conversion pasted in many handlers               | Shared `Result.FromException` helper               |
| Stripe webhook event-type switch arms reused in command handlers  | `IStripeEventDispatcher` with handler-per-type     |
| Same Angular `inject(...)` cluster across components in a feature | Feature-scoped service that bundles them           |
| Repeated `<ui-form-field>` validators on the same field type      | Shared validator factory in `@logistics/shared`    |
| Date / currency formatting hand-rolled in components              | `Labels` utility or pipe in `@logistics/shared`    |

## Don't refactor

- Code that's about to be deleted (check git for pending PRs).
- Generated code (`projects/shared/src/lib/api/generated/`, EF migrations after they're applied).
- Tests, unless splitting tests reveals shared fixtures that belong in a base class.
- Tight loops where duplication is intentional (perf-sensitive paths — leave a comment if you find one).
- Three nearly-similar lines. Three is not enough to abstract — it usually creates worse code than it removes. Wait until 4+ and the divergence isn't accidental.

### Don't simplify (LogisticsX-specific)

- **MediatR command/query handlers** even for one-line operations — the project convention requires this shape so cross-cutting behaviors (validation, logging, auditing) work uniformly. Don't replace with direct service calls.
- **`internal sealed` modifiers on handlers** — required by convention even though `public` would compile.
- **`IRepository<T>` / `IUnitOfWork`** — these are convention layers; don't bypass to `DbContext` directly even when "simpler".
- **Mapperly partial classes / `[Mapper]` attribute** — the pattern is repo-wide; don't replace with manual mapping.
- **Existing `partial class` splits** for entities (Load, Trip, Container) — these were intentional. Don't recombine.
- **`AuditableEntity` inheritance** when an entity could "just be" a record — auditing is enforced via interceptor.
- **FluentValidation validators** even when they look thin — they participate in the MediatR pipeline.
- **Standalone components / signals / native control flow** in Angular — these are project-mandated.

When in doubt, check `.claude/rules/` — if a pattern is documented as a convention there, don't simplify it away.

## Commit boundaries

One refactor = one commit, ideally. Order them so reviewers can read each independently:

```
refactor: drop unused IFoo interface (single impl, no substitution)
refactor: extract LoadStatusMachine guards into shared helper
refactor: split Load.cs into partial classes by concern
```

Use the `commit` skill afterwards.

## Verification checklist

- [ ] Plan was stated before edits started
- [ ] Each edit step ends with a green build + test
- [ ] No public API changed (controller routes, DTO shapes, command/query records, exported component selectors)
- [ ] Imports are clean (no unused `using`)
- [ ] File sizes now under the relevant soft thresholds
- [ ] Duplicates extracted have at least 4 call sites
- [ ] Removed code has no readers (`Find Usages` clean across the solution)
- [ ] Project conventions preserved (handlers still MediatR, mappers still Mapperly, etc.)
- [ ] One concern per file
- [ ] Commits split logically (simplify → dedup → split)

## Common mistakes

- **Splitting a file that's actually cohesive** — long doesn't mean bad. A 350-line entity that's all one aggregate's behavior is fine.
- **Premature abstraction**: extracting a "shared helper" with one caller. If only one caller exists, leave it inline.
- **Removing a "single-impl" interface that DI uses for testing** — check the test project for `Substitute.For<IFoo>()` before deletion.
- **Removing config that's set in production but unused locally** — search appsettings.\*.json AND the deployment env-var docs before deletion.
- **Renaming while refactoring**: changing names in the same commit hides the move. Rename in a follow-up commit.
- **Splitting along wrong seams** — if two halves still need to touch the same private state, the split is wrong. Use `partial class` or step back.
- **Leaving stale namespaces or `internal` modifiers** — when moving types, the visibility may need to change.
- **Forgetting to update `feature-map.md`** if a feature's primary file moves to a new path.
- **Simplifying away a convention** — see "Don't simplify" above. If a rule file documents the pattern, leave it.

## Related

- Built-in `simplify` skill — review recent changes for quality/reuse (complementary, narrower scope: only diffs)
- `.claude/rules/backend/csharp-conventions.md` — file-scoped namespaces, one type per file matching filename, async/cancellation conventions
- `.claude/rules/backend/mapperly.md` — when manual mapping is appropriate vs. Mapperly
- `feature-map.md` — update if a feature's primary file paths change
