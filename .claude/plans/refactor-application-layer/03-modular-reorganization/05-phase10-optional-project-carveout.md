# Phase 10 — (Optional) Promote stable modules to standalone csproj

> **Status: PENDING (optional).** Part of group `03-modular-reorganization/`. Only worth doing once Phases 7/8 have stabilized module boundaries.
>
> **Baseline update (post-`Modules/` consolidation):** Each module folder under `src/Core/Logistics.Application/Modules/{Module}/` is now self-contained — feature subfolders own their own `Commands/`, `Queries/`, `Services/`, `Specifications/`, and (where applicable) `Constants/`. The old top-level `Services/`, `Specifications/`, and `Constants/` folders no longer exist at the Application root. Cross-module shared items live in `Modules/Common/Constants/`. **Implication for this phase:** when promoting a module to its own csproj, the carve-out is a single `git mv Modules/{Module} ...` — there are no horizontal `Services/`/`Specifications/`/`Constants/` strands at the Application root that need to be untangled first. `Modules/Common/` stays in the root Application project as the shared kernel.

## Goal

Once a module's boundary has been stable for an extended period (suggested: ≥1 quarter post-Phase 7), promote it from a folder inside `Logistics.Application` to its own csproj: `Logistics.Application.{Module}`.

## Why this is **optional**

The modular monolith with folder-based modules (Phase 7) gives ~80% of the layering value at ~10% of the cost. Project splits cost real friction:

- Shared behaviours / validators / DI registration have to be reachable from multiple projects
- Build graphs and CI cache topology change
- IDE solution loads slower
- Cross-module project references need to be designed (one-direction only, no circulars)

So only split when you have concrete evidence that folder boundaries are too weak:

- Cross-module references are visibly creeping in
- Build times grow because every Application change rebuilds the whole 880-file project
- A module is mature enough to consider a real microservice extraction

If none of these are biting, **don't split**. Re-evaluate later.

## Prerequisites

- Phase 7 done.
- Phase 5 architecture tests already enforce cross-module rules at the namespace level (added when the module-aware tests are introduced — likely a sub-task here).
- Module has been stable for ≥1 quarter (low cross-module reference count, clear public surface).

## When to split — concrete signals

| Signal                                                                        | Threshold                                                      |
| ----------------------------------------------------------------------------- | -------------------------------------------------------------- |
| Average cross-module references per module per month                          | High and trending up                                           |
| `dotnet build` of `Logistics.Application` time                                | Greater than acceptable for the team                           |
| A module has been considered for microservice extraction in concrete planning | If yes → splitting csproj first lets you defer the extraction  |
| Frequency of merge conflicts inside the module's folder                       | Low (high churn means boundary is wrong, splitting won't help) |

## Likely first candidate: Compliance

Reasons:

- Self-contained (privacy, ELD, DVIR, accidents, inspections, safety)
- Low cross-module reference count
- Has its own background processors (data export, retention) that could live in their own assembly
- The Compliance domain is regulated (GDPR, HOS) — extra layering value because regulatory boundaries are easier to enforce per assembly

Second candidate: Integrations (LoadBoard, AI dispatch, webhooks). Third: Financial (Stripe-heavy; isolation has practical security/audit value).

## Mechanics (per module)

1. **Pre-flight check:**

   ```bash
   # cross-module references INTO this module
   grep -rln "Logistics\\.Application\\.Modules\\.{ModuleName}" \
        src/Core/Logistics.Application/Modules \
     | grep -v "Modules/{ModuleName}"
   ```

   Document them all. Each is either:
   - A legitimate public surface (will need to remain public after split)
   - A leak that should be refactored before the split
   - A cross-module event handler (fine — domain events flow either direction)

2. **Create the new project:**
   - `src/Core/Logistics.Application.{Module}/Logistics.Application.{Module}.csproj`
   - Target: `net10.0`, ImplicitUsings on, Nullable on
   - PackageReferences: MediatR, FluentValidation.DependencyInjectionExtensions (match the main Application's versions)
   - ProjectReferences:
     - `Application.Abstractions`
     - `Domain`
     - `Domain.Primitives`
     - `Mappings`
     - `Shared.Models`
     - Possibly `Logistics.Application` itself if there's a one-way dependency (avoid circulars)

3. **Move folder contents:**
   - `git mv src/Core/Logistics.Application/Modules/{Module} src/Core/Logistics.Application.{Module}/`
   - Update namespaces: `Logistics.Application.Modules.{Module}` → `Logistics.Application.{Module}` (or keep `.Modules.{Module}` if you prefer the longer namespace)
   - Scripted; same encoding rules as previous phases

4. **Wire MediatR scanning:**
   - In `Application/Registrar.cs`, add the new assembly to the scan list:
     ```csharp
     cfg.RegisterServicesFromAssembly(typeof(Registrar).Assembly);
     cfg.RegisterServicesFromAssembly(typeof(Logistics.Application.{Module}.ModuleMarker).Assembly);
     ```
   - Add a `ModuleMarker` empty class in the new project just for `typeof()` lookups.

5. **Reference direction:**
   - `Logistics.Application` references `Logistics.Application.{Module}` (Application is the orchestrator).
   - Sibling modules do **not** reference each other directly — they communicate via Domain events or shared abstractions.

6. **Update arch tests** (Phase 5):
   - Add rules that the new module assembly doesn't reach sideways into siblings.
   - Add rule that the new assembly references only `Application.Abstractions`, `Domain`, and (transitively) what Application references.

7. **Update `Logistics.slnx`** — add the new project under `/Core/`.

8. **Update `feature-map.md`** — every row in the moved module changes path.

9. **Update `CLAUDE.md`** to mention the new top-level project.

10. **Verification:**
    - `dotnet build Logistics.slnx` → 0 errors
    - Architecture tests pass
    - All tests pass
    - DI startup test: every command/query in the moved module still resolves and dispatches

## Order of work

1. Compliance (proves the pattern; smallest risk)
2. Watch for ≥2 weeks. Re-evaluate signals.
3. If still beneficial: Integrations
4. Watch again
5. If still beneficial: Financial

Do not split more than one module per quarter without a strong reason.

## Critical files (per module split)

- New: `src/Core/Logistics.Application.{Module}/Logistics.Application.{Module}.csproj`
- New: `src/Core/Logistics.Application.{Module}/ModuleMarker.cs`
- Moved: all `Modules/{Module}/` contents
- Updated: `Logistics.slnx`
- Updated: `Application.csproj` (adds ProjectReference to the new project)
- Updated: `Application/Registrar.cs` (adds assembly to MediatR scan)
- Updated: `feature-map.md` (paths)
- Updated: `Logistics.Architecture.Tests/BoundaryTests.cs` (cross-module rules)
- Updated: `CLAUDE.md` (project list)

## Verification

- `dotnet build Logistics.slnx` — 0 errors.
- All architecture tests pass, including new cross-module rules.
- `dotnet test` — full suite.
- DI startup smoke: a command and a query from the moved module still dispatch.

## Risks

| Risk                                                                      | Mitigation                                                                                            |
| ------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| Circular references between Application and the new module                | Application references the module, not the reverse. Module references only `Abstractions` + `Domain`. |
| FluentValidation can't find validators in the new assembly                | Register `AddValidatorsFromAssembly` on both assemblies.                                              |
| MediatR can't find handlers in the new assembly                           | `RegisterServicesFromAssembly` for the new module too.                                                |
| Build time gets worse, not better (because of CI cache misses)            | Measure before/after. If it doesn't help, revert.                                                     |
| Cross-module event handlers stop firing (subscriber assembly not scanned) | The MediatR registration scans all referenced module assemblies. Verify in startup test.              |

## Acceptance (per module split)

- [ ] New `Logistics.Application.{Module}.csproj` exists, builds, included in `Logistics.slnx`.
- [ ] Folder moved with namespace rewrite; no orphan old paths.
- [ ] MediatR + FluentValidation scan the new assembly.
- [ ] Application references the new project; sibling modules don't reference it directly.
- [ ] Architecture tests updated with cross-module rules and passing.
- [ ] `feature-map.md`, `CLAUDE.md` reflect the new project.
- [ ] One PR per module split (don't batch).
