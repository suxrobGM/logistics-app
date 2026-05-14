# Phase 3 — Surgical fixes for the two flagrant Application-layer leaks

> **Status: DONE — 2026-05-13.** Commits `27adc83d`, `de8b4998`, `9af475f6`.
>
> `ICurrentUserService` exposes `IpAddress`/`UserAgent`; both `CurrentUserService` and `NoopCurrentUserService` implement them. `ImpersonateUserHandler` no longer injects `IHttpContextAccessor`. `Microsoft.AspNetCore.Http` package dropped from `Application.csproj`. `.Include(i => i.Tenant)` removed from `AcceptInvitationHandler` — invitation accept now lazy-loads the navigation property.

## Goal

Fix two specific layering violations discovered during the Phase 1 audit. Each is independent of the bigger reorg work and can ship in isolation.

## The two leaks

### 1. `IHttpContextAccessor` in an Application handler

**File:** [src/Core/Logistics.Application/Commands/User/ImpersonateUser/ImpersonateUserHandler.cs:20](../../../../src/Core/Logistics.Application/Commands/User/ImpersonateUser/ImpersonateUserHandler.cs)

The handler currently injects `IHttpContextAccessor` directly to read the client IP for audit logging:

```csharp
IHttpContextAccessor httpContextAccessor,  // line 20
...
var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()  // line 29
```

This is also why `Logistics.Application.csproj` still has `<PackageReference Include="Microsoft.AspNetCore.Http" Version="2.2.2" />` (an ancient 2.2.2 pin — bonus smell).

**Fix:** Move HTTP-context inspection into the `ICurrentUserService` implementation in Presentation/Persistence.

Concrete steps:

1. Add to `ICurrentUserService` (in `Abstractions/CurrentUser/`):
   ```csharp
   string? IpAddress { get; }
   string? UserAgent { get; }
   ```
2. Update the existing `CurrentUserService` implementation (in `Infrastructure.Persistence/Services/User/CurrentUserService.cs`) to read from `IHttpContextAccessor` and expose those properties.
3. Update `ImpersonateUserHandler` to inject `ICurrentUserService` and read `IpAddress`/`UserAgent` from it.
4. Remove `IHttpContextAccessor` from the handler's constructor.
5. After the build is green: **remove `<PackageReference Include="Microsoft.AspNetCore.Http" />` from `Logistics.Application.csproj`** — the package is no longer used.

### 2. `.Include()` violation against the lazy-loading rule

**File:** [src/Core/Logistics.Application/Commands/Invitation/AcceptInvitation/AcceptInvitationHandler.cs:25](../../../../src/Core/Logistics.Application/Commands/Invitation/AcceptInvitation/AcceptInvitationHandler.cs)

```csharp
using Microsoft.EntityFrameworkCore;  // line 9
...
.Include(i => i.Tenant)  // line 25
```

The project rule [`.claude/rules/backend/csharp-conventions.md`](../../../rules/backend/csharp-conventions.md) explicitly says: "Lazy loading enabled — do NOT use `.Include()` for navigation properties." This is the **only** `.Include` call in the entire Application project (verified by audit).

**Fix:**

1. Inspect _why_ `.Include` was used. If it's a real perf concern (the lazy load would fire mid-loop), document and add a non-lazy repo method:
   ```csharp
   // ITenantUnitOfWork extension or a query method on the repository
   var invitation = await uow.Repository<Invitation>().Query()
       .Where(i => i.Token == cmd.Token)
       .Select(i => new { i, i.Tenant })
       .FirstOrDefaultAsync(ct);
   ```
   But projection like this changes the consumer pattern. Simpler:
2. Just remove the `.Include` and rely on lazy loading. Add a code comment if there's a perf hot-path concern.
3. Remove the `using Microsoft.EntityFrameworkCore;` line from this file.

After fix: `grep -rn "\\.Include(\\|using Microsoft\\.EntityFrameworkCore" src/Core/Logistics.Application/` should return zero hits.

## Prerequisites

- On `refactor/application-abstractions` at or after commit `00ddc2a1`.
- Phase 2 (Stripe/Tax mapper moves) helpful but not required.
- Slice 1.9-remainder may affect `CurrentUserService` registration; coordinate.

## Step-by-step

1. **Leak 1 — `IHttpContextAccessor`:**
   - Add `IpAddress` and `UserAgent` properties to `ICurrentUserService`.
   - Update the concrete `CurrentUserService` to read from `IHttpContextAccessor`.
   - Update `NoopCurrentUserService` if it exists — return `null` for both.
   - Update `ImpersonateUserHandler` to inject `ICurrentUserService` instead of `IHttpContextAccessor`.
   - `dotnet build` → green.
   - Commit.

2. **Leak 1 follow-up — drop the package:**
   - Remove `<PackageReference Include="Microsoft.AspNetCore.Http" Version="2.2.2" />` from `src/Core/Logistics.Application/Logistics.Application.csproj`.
   - `dotnet build` → green. If anything else in Application used `Microsoft.AspNetCore.Http`, the build will surface it. Fix or revert.
   - Commit.

3. **Leak 2 — `.Include()`:**
   - Remove the `.Include(i => i.Tenant)` call.
   - Remove `using Microsoft.EntityFrameworkCore;`.
   - Test that invitation-acceptance still works.
   - Commit.

## Critical files

- `src/Core/Logistics.Application.Abstractions/CurrentUser/ICurrentUserService.cs` — add 2 properties
- `src/Infrastructure/Logistics.Infrastructure.Persistence/Services/User/CurrentUserService.cs` — implement the new properties
- `src/Infrastructure/Logistics.Infrastructure.Persistence/Services/User/NoopCurrentUserService.cs` — null impl
- `src/Core/Logistics.Application/Commands/User/ImpersonateUser/ImpersonateUserHandler.cs` — swap injection
- `src/Core/Logistics.Application/Logistics.Application.csproj` — drop package
- `src/Core/Logistics.Application/Commands/Invitation/AcceptInvitation/AcceptInvitationHandler.cs` — remove `.Include`

## Verification

- `grep -rn "IHttpContextAccessor" src/Core/Logistics.Application --include='*.cs'` → 0
- `grep -rn "\\.Include(" src/Core/Logistics.Application --include='*.cs'` → 0
- `grep -rn "using Microsoft\\.EntityFrameworkCore" src/Core/Logistics.Application --include='*.cs'` → 0
- `grep -n "AspNetCore\\.Http" src/Core/Logistics.Application/Logistics.Application.csproj` → 0
- `dotnet build Logistics.slnx` → 0 errors.
- Manual: log in as SuperAdmin, impersonate another user; verify audit log row captures IP and User-Agent.
- Manual: invitation accept flow still works end-to-end.

## Risks

| Risk                                                                                  | Mitigation                                                                                                       |
| ------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| `Connection.RemoteIpAddress` vs `X-Forwarded-For` — reverse-proxy IP detection broken | Keep existing detection logic, just move it into `CurrentUserService` impl. Don't redesign.                      |
| Lazy-loading the Tenant for invitations triggers an extra DB roundtrip per accept     | Acceptable — invitation accept is a low-frequency action. If profiling shows it matters, add a projection later. |
| Removing the AspNetCore.Http v2.2.2 package surfaces some other implicit usage        | Build will fail explicitly; fix or restore package.                                                              |

## Acceptance

- [ ] `IHttpContextAccessor` not referenced anywhere in `src/Core/Logistics.Application/`.
- [ ] `ICurrentUserService` exposes `IpAddress`, `UserAgent`.
- [ ] `Microsoft.AspNetCore.Http` package reference removed from `Logistics.Application.csproj`.
- [ ] `.Include()` not present anywhere in `src/Core/Logistics.Application/`.
- [ ] `using Microsoft.EntityFrameworkCore` not present in `src/Core/Logistics.Application/` (except where intentionally needed — verify).
- [ ] Manual impersonation + invitation flows still work.
- [ ] 3 commits (CurrentUser change, package removal, .Include removal).
