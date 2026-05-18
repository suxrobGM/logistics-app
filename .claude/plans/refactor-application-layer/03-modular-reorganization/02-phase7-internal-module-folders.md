# Phase 7 — Internal module reorganization (folders + namespaces, no project split)

> **Status: DONE — completed 2026-05-18** (executed in one session as 6 module commits + 1 cleanup-of-empty-namespaces commit folded into the Platform commit + 1 feature-map commit; namespaces went hierarchical to `Logistics.Application.Modules.{Module}.{Feature}.{Commands|Queries|Events}`. Full build clean; 497 tests pass, 0 fail). Part of group `03-modular-reorganization/`.

## Goal

Reorganize the 433 Commands files + 306 Queries files (~740 files) under `src/Core/Logistics.Application/` from a flat per-entity layout into a per-bounded-context layout. Folder + namespace move only — **no project split**.

## Why

The current layout:

```
Commands/
  Accident/ AiDispatch/ ApiKey/ BlogPost/ Contact/ Container/ Customer/
  CustomerUser/ DemoRequest/ Document/ Dvir/ Eld/ Employee/ Expense/
  Feature/ Inspection/ Invitation/ Invoice/ Load/ LoadBoard/ Maintenance/
  Messaging/ Payment/ PaymentLink/ Payroll/ Privacy/ Safety/ StripeConnect/
  Subscription/ Tax/ Tenant/ Terminal/ TimeEntry/ Tracking/ Trip/ Truck/
  UpdateNotification/ User/ Webhooks/
```

39 top-level folders, mostly per-entity. Ownership is unclear (who owns Invoice? Payroll? Both touch it). Cross-feature navigation is by string-match, not concept.

The new layout groups by **bounded context**:

```
src/Core/Logistics.Application/
  Common/
    Behaviours/        ← existing, moved
    Registrar.cs       ← orchestrator (from Phase 4)
  Modules/
    Operations/
      Loads/  Trips/  Trucks/  Containers/  Terminals/  Tracking/
    Compliance/
      Eld/  Dvir/  Accident/  Inspection/  Safety/  Privacy/
    Financial/
      Invoices/  Payments/  PaymentLinks/  Payroll/  Tax/  Expense/  StripeConnect/
    IdentityAccess/
      User/  CustomerUser/  Customer/  Employee/  Roles/  Invitation/  ApiKey/
      Tenant/  Subscription/  Feature/
    Integrations/
      LoadBoard/  Webhooks/  Eld/  AiDispatch/  Messaging/  UpdateNotification/  Document/
    Platform/
      BlogPost/  Contact/  DemoRequest/  Privacy/  Reports/  Stats/  Portal/
```

Each module group becomes the natural unit for:

- Code review ownership
- Future project split (Phase 10) if needed
- Feature scaffolding (the `scaffold-feature` skill plugs into this)
- Read-side projection extraction (Phase 8)

## Prerequisites

- Phase 1, 2, 3 done (clean baseline).
- Phase 4 done (module registrars exist as stubs; they get populated here).
- Phase 6 helpful but optional — markers (`ICommand`/`IQuery`) don't depend on folder location.
- Phase 5 architecture tests in place — they'll detect any namespace-rename mistake immediately.

## Scope

### In scope

- `git mv` files from `Commands/{Entity}/{Action}/` → `Modules/{Module}/{Feature}/Commands/{Action}/`
- `git mv` files from `Queries/{Entity}/{Action}/` → `Modules/{Module}/{Feature}/Queries/{Action}/`
- `git mv` files from `Events/{Entity}/` → `Modules/{Module}/{Feature}/Events/`
- Update namespaces inside each moved file (scripted).
- Populate module registrar bodies created by Phase 4 stubs.
- Update [`.claude/feature-map.md`](../../../feature-map.md) — every path column changes.

### Out of scope

- Splitting handlers further (e.g., extracting policies). Mechanical move only.
- Changing the public surface (commands/queries/handlers keep their type names).
- Moving Application/Services/\* (they stay flat; Phase 4 marker convention covers them).

## Sequencing (smallest module first; commit per module)

| Order | Module         | Rough size | Why this order                                              |
| ----- | -------------- | ---------- | ----------------------------------------------------------- |
| 1     | Compliance     | ~60 files  | Already self-contained; proves the pattern                  |
| 2     | IdentityAccess | ~80 files  | Self-contained, but bigger                                  |
| 3     | Financial      | ~120 files | Critical path; runs after we trust the pattern              |
| 4     | Integrations   | ~60 files  | LoadBoard, Webhooks, AI tools                               |
| 5     | Operations     | ~150 files | Largest; runs after smaller ones validate                   |
| 6     | Platform       | ~50 files  | Last; sweeps the leftovers (Reports, Stats, BlogPost, etc.) |

One module per PR.

## Mechanics

### Scripted namespace rewrite

After `git mv` of all files in a module:

```powershell
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

function Rewrite-Namespace($file, $oldRoot, $newRoot) {
    $bytes = [System.IO.File]::ReadAllBytes($file)
    $hadBom = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    $text = [System.IO.File]::ReadAllText($file)
    if ($text.Length -gt 0 -and $text[0] -eq [char]0xFEFF) { $text = $text.Substring(1) }
    $new = $text.Replace($oldRoot, $newRoot)
    $enc = if ($hadBom) { New-Object System.Text.UTF8Encoding($true) } else { $utf8NoBom }
    [System.IO.File]::WriteAllText($file, $new, $enc)
}

# Example: Compliance/Privacy
$old = "Logistics.Application.Commands.Privacy"
$new = "Logistics.Application.Modules.Compliance.Privacy.Commands"
Get-ChildItem "src\Core\Logistics.Application\Modules\Compliance\Privacy\Commands" -Recurse -Filter *.cs |
    ForEach-Object { Rewrite-Namespace $_.FullName $old $new }
```

**Encoding rule (do not skip):** PowerShell 5.1's `-Encoding utf8` adds BOM. The project's `.cs` files are UTF-8 no-BOM. Use `[System.IO.File]::WriteAllText` with `UTF8Encoding($false)`. Phase 1 hit a BOM regression — don't repeat.

Then scan for stragglers in consumers (handlers in other modules, validators, tests):

```bash
grep -rln "Logistics\\.Application\\.Commands\\.{OldEntity}" src/ test/
```

Add the new namespace to consumers' `using` lines (don't blindly find-and-replace — old namespaces shouldn't exist after Phase 7 but a consumer importing `Logistics.Application.Commands.X` needs the new `Logistics.Application.Modules.Y.X.Commands` import).

### Module registrar bodies

The Phase 4 stub for, e.g., `ComplianceModuleRegistrar` becomes:

```csharp
public static IServiceCollection AddComplianceModule(this IServiceCollection services)
{
    // MediatR + validators already scanned at the assembly level.
    // Add module-specific decorators or named instances here.
    return services;
}
```

Typically minimal unless the module has decorator chains.

## `feature-map.md` update

Every row in [`.claude/feature-map.md`](../../../feature-map.md) currently has columns like `Commands path: Commands/Load/CreateLoad/`. After Phase 7 these all change to `Modules/Operations/Loads/Commands/CreateLoad/`. One mechanical edit, ideally scripted:

```powershell
$content = Get-Content ".claude/feature-map.md" -Raw
$content = $content -replace 'Commands/Load/',     'Modules/Operations/Loads/Commands/'
$content = $content -replace 'Queries/Load/',      'Modules/Operations/Loads/Queries/'
# ... etc for each entity ...
Set-Content ".claude/feature-map.md" $content -Encoding utf8NoBOM
```

Verify by eye — feature-map is load-bearing for navigation.

## Critical files

- ~740 `.cs` files moved + namespace-rewritten
- 6 module registrar files (populated)
- `.claude/feature-map.md` — every path updated
- Scrutor in Phase 4 already scans the assembly, so it picks up new locations automatically

## Verification per module PR

- `dotnet build Logistics.slnx` — 0 errors.
- `dotnet test` — full suite passes.
- MediatR assembly scanning still finds all handlers (it doesn't care about namespaces).
- Architecture tests from Phase 5 pass.
- Manual spot-check: pick 3 handlers from the moved module, find them in the new path, verify the namespace.

## Risks

| Risk                                                                                                            | Mitigation                                                                                            |
| --------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| Namespace rewrite breaks XML doc-comment `<see cref="X" />` references                                          | Scripted rewrite handles them too (the text is in source files). Verify with `dotnet build` warnings. |
| A test fixture imports `Logistics.Application.Commands.{OldEntity}` and isn't covered by the consumer-grep pass | The build will surface it. Fix the using lines.                                                       |
| `feature-map.md` falls behind reality                                                                           | Script the rewrite same as code; verify by clicking 2-3 links after each module PR.                   |
| Encoding regression introducing BOM                                                                             | Use UTF8Encoding($false). Same rule as Phase 1.                                                       |
| Trying to do all 6 modules in one PR                                                                            | Don't. One module per PR, smallest first.                                                             |

## Acceptance

- [x] All 740-ish files moved into `Modules/{Module}/{Feature}/{Commands|Queries|Events}/`.
- [x] Namespaces match the new locations.
- [x] `Commands/` and `Queries/` top-level folders (the old layout) are empty/deleted.
- [x] `feature-map.md` updated; every row points at the new path.
- [x] Module registrars exist and are wired in `Registrar.cs`.
- [x] All tests pass.
- [x] Architecture tests pass.
- [x] 6 commits (one per module).
