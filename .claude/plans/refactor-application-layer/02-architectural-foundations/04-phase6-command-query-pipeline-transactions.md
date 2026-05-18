# Phase 6 — `ICommand`/`IQuery` markers + split pipeline + `TransactionBehaviour`

> **Status: PARTIAL — 2026-05-13.** Markers, pipeline split, and rename landed on branch `refactor/application-abstractions` (commits `dce25226` … `5afad245`). `TransactionBehaviour` was implemented, audited, then **reverted**: the audit surfaced ~15% of handlers that need `[NoAutoTransaction]` (persist-on-Fail audit logs, blob-rollback patterns, webhooks, DDL provisioning, out-of-process side-effects), so a "centralized tx" with that opt-out rate is a leaky abstraction. Handlers continue to own `SaveChangesAsync` deterministically. The audit insights are preserved in the revert commit message.
>
> **What landed (kept):** `ICommand<T>` / `IQuery<T>` / `IMasterCommand<T>` markers replace `IAppRequest<T>`; queries now also flow through Validation + FeatureCheck behaviours; `IAppRequest` deleted; `FeatureCheckBehaviour` caches its attribute lookup.
>
> **What did not land (reverted):** `TransactionBehaviour`, `NoAutoTransactionAttribute`, `ICrossDatabaseCommand`, all `[NoAutoTransaction]` applications, all `ICrossDatabaseCommand` applications, the `SaveChangesAsync`-removal sweep, the rollback-on-failure integration test, the `No_handler_calls_SaveChangesAsync_directly` arch test.

## Goal

Replace `IAppRequest<TResponse>` with two markers (`ICommand<TResponse>`, `IQuery<TResponse>`). Split the MediatR pipeline so commands and queries get appropriate behaviours. Add a `TransactionBehaviour` that wraps every command in a Unit-of-Work transaction. Migrate **191 scattered `SaveChangesAsync` calls** out of handlers.

## Why

Today every handler returns `IAppRequest<Result>` and the entire pipeline (logging → exception → feature-check → validation) runs on every request. Two problems:

1. **Commands and queries pay the same overhead.** Validation + feature-checks belong on queries too (security rule says validate all user inputs), but transactions don't.
2. **Handlers call `SaveChangesAsync` themselves — 191 call sites.** Scattered, easy to miss, no consistent failure-vs-success policy. A handler returning `Result.Fail` after staging changes still has those changes persisted.

A `TransactionBehaviour` wrapping commands fixes both: changes commit exactly once at the boundary, and **changes are rolled back on `Result.Fail`**.

## Prerequisites

- Phase 1 fully done (Slices 1.8, 1.9-remainder, optionally Abstractions-Domain decoupling).
- Phase 2 + 3 done (clean baseline).
- Phase 5 architecture tests in place (the test that flags `SaveChangesAsync` in handlers will be **added at the end** of Phase 6, not the start).

## Design (the parts that must not change during execution)

### Markers (in `Application.Abstractions/Common/`)

```csharp
public interface ICommand<TResponse> : IRequest<TResponse> where TResponse : IResult, new();
public interface ICommand : ICommand<Result>;

public interface IQuery<TResponse> : IRequest<TResponse> where TResponse : IResult, new();

// Optional: marker for commands that mutate the master DB instead of tenant DB
public interface IMasterCommand : ICommand;
public interface IMasterCommand<TResponse> : ICommand<TResponse> where TResponse : IResult, new();

// Cross-DB command: explicitly accepts no atomicity guarantee
public interface ICrossDatabaseCommand : ICommand;

// Opt-out attribute for handlers that own their own transaction (webhooks, etc.)
[AttributeUsage(AttributeTargets.Class)]
public sealed class NoAutoTransactionAttribute : Attribute;
```

`IAppRequest` is **deleted** after the migration — not kept as a transitional base. The migration script renames every existing request type to the right marker (see "Scripted rename" below).

### Pipeline wiring (in `Application/Registrar.cs`)

| Pipeline                   | Behaviours (in order)                                                                |
| -------------------------- | ------------------------------------------------------------------------------------ |
| `ICommand` / `ICommand<T>` | `Logging` → `UnhandledException` → `Validation` → `FeatureCheck` → **`Transaction`** |
| `IQuery` / `IQuery<T>`     | `Logging` → `UnhandledException` → `Validation` → `FeatureCheck`                     |

Queries get validation + feature checks too — per `.claude/rules/backend/security.md`, validation applies to all user inputs.

### `TransactionBehaviour<TRequest, TResponse>` — full algorithm

```
1. If [NoAutoTransaction] is on TRequest type:
       return await next(ct);

2. Pick UoW:
   - Default: ITenantUnitOfWork
   - If TRequest implements IMasterCommand: IMasterUnitOfWork
   - If TRequest implements ICrossDatabaseCommand: both — no atomicity guarantee.
     Log warning at startup that this command spans DBs.

3. await uow.BeginTransactionAsync(ct);   // idempotent — verified in UnitOfWork.cs

4. TResponse result;
   try {
       result = await next(ct);
   } catch (Exception) {
       try { await uow.RollbackTransactionAsync(ct); } catch { /* swallow rollback errors */ }
       throw;
   }

5. if (result is IResult r && !r.IsSuccess) {
       await uow.RollbackTransactionAsync(ct);
       return result;                       // do NOT SaveChanges
   }

6. await uow.CommitTransactionAsync(ct);    // commits + SaveChangesAsync internally
   return result;
```

**Key invariants:**

- A `Result.Fail` from the handler does **not** persist changes (Codex's catch on an earlier draft — easy to miss).
- `CommitTransactionAsync` already calls `SaveChangesAsync` (verified in `UnitOfWork.cs:50`), so handlers MUST stop calling `SaveChangesAsync` themselves.
- Cross-DB commands either opt out (`[NoAutoTransaction]`) or explicitly mark `ICrossDatabaseCommand` and accept best-effort sequencing.

### Performance polish (do alongside the markers)

`FeatureCheckBehaviour` currently reflects on `typeof(TRequest)` on every call. Cache the lookup with a `ConcurrentDictionary<Type, RequiresFeatureAttribute?>`. Same for any other behaviour that does runtime attribute scanning.

## `[NoAutoTransaction]` candidates that need explicit review

(From a quick audit of `src/Core/Logistics.Application/Commands/`. Re-verify during execution.)

- Everything under `Commands/Webhooks/`
- `Commands/StripeConnect/*` (callbacks from Stripe Connect onboarding)
- `Commands/Eld/ProcessEldWebhook/`
- `Commands/Tenant/CreateTenant/` — touches master + tenant DBs + DB provisioning
- `Commands/Invitation/AcceptInvitation/` — multi-step + email side-effects
- `Commands/User/ImpersonateUser/` — auth flow
- `Commands/Employee/DeleteEmployee/` — cascade across multiple aggregates + identity
- `Commands/Load/BulkAssignLoads/`, `BulkDeleteLoads/`, `BulkDispatchLoads/` — review one-vs-many transaction policy

Each gets a short audit during the migration: either `[NoAutoTransaction]` with a justification comment, or accept default auto-transactional behaviour.

## Scripted rename (the `IAppRequest` → `ICommand`/`IQuery` sweep)

Do this as **one PR** with the script committed alongside. Classify each request type by folder:

```powershell
$cmdRoot   = "src\Core\Logistics.Application\Commands"
$queryRoot = "src\Core\Logistics.Application\Queries"

Get-ChildItem $cmdRoot -Recurse -Filter *.cs | ForEach-Object {
    $bytes = [System.IO.File]::ReadAllBytes($_.FullName)
    $hadBom = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    $text = [System.IO.File]::ReadAllText($_.FullName)
    if ($text.Length -gt 0 -and $text[0] -eq [char]0xFEFF) { $text = $text.Substring(1) }
    $new = $text `
        -replace ': IAppRequest<([^>]+)>', ': ICommand<$1>' `
        -replace ': IAppRequest\b',         ': ICommand'
    if ($new -ne $text) {
        $enc = if ($hadBom) { New-Object System.Text.UTF8Encoding($true) } else { New-Object System.Text.UTF8Encoding($false) }
        [System.IO.File]::WriteAllText($_.FullName, $new, $enc)
    }
}

Get-ChildItem $queryRoot -Recurse -Filter *.cs | ForEach-Object {
    $bytes = [System.IO.File]::ReadAllBytes($_.FullName)
    $hadBom = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    $text = [System.IO.File]::ReadAllText($_.FullName)
    if ($text.Length -gt 0 -and $text[0] -eq [char]0xFEFF) { $text = $text.Substring(1) }
    $new = $text -replace ': IAppRequest<([^>]+)>', ': IQuery<$1>'
    if ($new -ne $text) {
        $enc = if ($hadBom) { New-Object System.Text.UTF8Encoding($true) } else { New-Object System.Text.UTF8Encoding($false) }
        [System.IO.File]::WriteAllText($_.FullName, $new, $enc)
    }
}
```

**Encoding rules:** PowerShell 5.1's `-Encoding utf8` adds BOM. Original files in this repo are UTF-8 **no-BOM**. Always use `[System.IO.File]::ReadAllText` + `WriteAllText` with explicit `UTF8Encoding($false)`. (This bit the Phase 1 work — don't repeat.)

After the script, surface stragglers:

```powershell
Get-ChildItem src -Recurse -Filter *.cs | Select-String -Pattern 'IAppRequest' -List
```

Fix any edge cases manually (request types defined outside `Commands/Queries`, handlers shared between command and query, etc.).

**Final step:** delete `IAppRequest.cs`. Arch test in Phase 5 (or a new test added here) forbids reintroduction.

## `SaveChangesAsync` migration (191 call sites)

Land in stages, feature-flagged:

1. **Land `TransactionBehaviour` behind an env-var flag** that defaults to OFF. Build green; behaviour exists but doesn't run.
2. **Per-feature-area PRs.** Pick a Commands/{Area} folder (e.g., `Commands/Invoice/`), flip the flag for that area's tests, delete every `SaveChangesAsync` call in those handlers, run tests, commit. Repeat for each area.
3. After all areas migrated, **remove the flag**, **enable `TransactionBehaviour` globally**, **add the arch test** `No_handler_calls_SaveChangesAsync_directly` from Phase 5's catalog.

## Step-by-step

1. **Add markers + behaviours**:
   - Create `Application.Abstractions/Common/ICommand.cs`, `IQuery.cs`, `IMasterCommand.cs`, `ICrossDatabaseCommand.cs`, `NoAutoTransactionAttribute.cs`.
   - Create `Application/Behaviours/TransactionBehaviour.cs` (full algorithm above).
   - Cache attribute lookup in `FeatureCheckBehaviour`.
   - Commit.
2. **Wire pipeline split** in `Registrar.cs`:
   - Commands pipeline: Logging, UnhandledException, Validation, FeatureCheck, **Transaction** (last).
   - Query pipeline: Logging, UnhandledException, Validation, FeatureCheck.
   - At first, `TransactionBehaviour` is feature-flagged off so nothing breaks.
   - Commit.
3. **Run the rename script** (`IAppRequest` → `ICommand`/`IQuery`). **One PR.**
4. **Delete `IAppRequest.cs`.** Verify build.
5. **Audit `[NoAutoTransaction]` candidates** (list above). For each, decide: opt-out or accept default. Add the attribute with a justification comment where opt-out is correct.
6. **Per-area `SaveChangesAsync` deletion**, feature-flagged. One PR per Commands/{Area}.
7. **Enable transaction behaviour by default**, remove the env-var flag.
8. **Add the arch test `No_handler_calls_SaveChangesAsync_directly`** (extends Phase 5).

## Critical files

- New: `src/Core/Logistics.Application.Abstractions/Common/ICommand.cs`, `IQuery.cs`, `IMasterCommand.cs`, `ICrossDatabaseCommand.cs`, `NoAutoTransactionAttribute.cs`
- New: `src/Core/Logistics.Application/Behaviours/TransactionBehaviour.cs`
- Updated: `src/Core/Logistics.Application/Registrar.cs` — pipeline split
- Updated: `src/Core/Logistics.Application/Behaviours/FeatureCheckBehaviour.cs` — cache attribute lookup
- ~700 handler files touched by rename script
- ~191 handlers touched by `SaveChangesAsync` removal
- Updated: `tests/Logistics.Architecture.Tests/HandlerSaveChangesTests.cs` (new)

## Verification

- `dotnet build Logistics.slnx` → 0 errors at every PR boundary.
- `grep -rn "IAppRequest\\b" src/ --include='*.cs'` → 0 after step 4.
- `grep -rn "SaveChangesAsync" src/Core/Logistics.Application --include='*.cs'` → 0 after step 7.
- Integration test (write one): a handler stages `Repository.Add(...)`, then returns `Result.Fail(...)`. Assert the row was NOT persisted.
- Manual webhook smoke: a Stripe webhook with `[NoAutoTransaction]` still commits explicitly.
- Tests pass at every PR.

## Risks

| Risk                                                                                 | Mitigation                                                                                                 |
| ------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------- |
| `TransactionBehaviour` ordering wrong (commits before validation runs)               | Pipeline order in registration is explicit; tests assert it.                                               |
| A handler returns `Result.Fail` but data was committed anyway (Codex's main concern) | Integration test for the rollback-on-failure path. Treat as P0 if it fails.                                |
| Cross-DB command silently double-commits one side                                    | `ICrossDatabaseCommand` + `[NoAutoTransaction]` recommendation; log warning at startup so they're visible. |
| 700-file rename PR is unreviewable                                                   | Commit the script in the same PR; reviewer re-runs it locally and confirms diff is mechanical.             |
| Encoding regression (BOM mismatch) during the rename                                 | Use `UTF8Encoding($false)`. Phase 1 hit this; learn from it.                                               |
| Feature flag left on/off accidentally in production                                  | Step 7 removes the flag entirely.                                                                          |

## Acceptance

- [ ] `IAppRequest` deleted from the codebase.
- [ ] Every command implements `ICommand` or `ICommand<T>`.
- [ ] Every query implements `IQuery<T>`.
- [ ] `TransactionBehaviour` wired only on the command pipeline.
- [ ] Validation + FeatureCheck run on both pipelines.
- [ ] `SaveChangesAsync` not called in any handler (verified by arch test and grep).
- [ ] Rollback-on-failure integration test green.
- [ ] All `[NoAutoTransaction]` candidates audited; opt-outs have justification comments.
- [ ] One commit per logical step (per the step-by-step above).
