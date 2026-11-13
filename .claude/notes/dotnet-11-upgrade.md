# .NET 11 / C# 15 upgrade

Started 2026-11-11. Phase 1 is done; phase 2 waits on third-party packages.

## Done (phase 1)

- Every project targets `net11.0` from `Directory.Build.props`; SDK pinned to 11.0.100 in `global.json`.
  CI (`build.yml`) and the three Dockerfiles use 11.0.
- ASP.NET Core / Microsoft.Extensions on 11.0.0. Open.IdentityServer 3.1.5, Anthropic 13.8
  (API version `2026-10-21`), Stripe.net 53.1 (API version `2026-09-30.endive`).
- `double`/`float` → `decimal` goes through `ToDecimal()` (`FloatingPointExtensions`): .NET 11
  casts keep the exact binary value, which skewed driver share, payroll and rate math.
- xUnit v3 on Microsoft.Testing.Platform (`global.json` `test.runner`), `coverlet.MTP`.
- Built-in OpenAPI (`Microsoft.AspNetCore.OpenApi`) replaced Swashbuckle on 2026-11-12, with Scalar at
  `/scalar`. MVC and `Http.Json` share one `ConfigureJson`, with numbers kept strict. The document is pinned to
  3.1 because ng-openapi-gen can't read 3.2. Three transformers in `Logistics.API/Serialization/` fix generator gaps:
  date-time types behind the custom converter, `null` leaking into shared enums, and enum query/route/form
  fields listed by C# name. Generic schemas use the default names (`PagedResponseOfLoadDto`). Clients
  regenerated with ng-openapi-gen 1.1 and openapi-generator 7.27.
- C#: collection expressions (incl. C# 15 `with(...)`), primary constructors, extension blocks,
  `[GeneratedRegex]`, UUIDv7 primary keys. Style rules enforced in the build.
- Private: outreach tool on `net11.0` (its own `private/Directory.Packages.props`).

## Phase 2: EF Core 11

Blocked until these ship GA (check NuGet):

- [ ] `Npgsql.EntityFrameworkCore.PostgreSQL` 11.0.0 (11.0.0-rc.3 pins EF Core to rc.3)
- [ ] `EFCore.NamingConventions` 11.0.0 (10.0.1 requires EF Core < 11)

Then, in one change:

- [ ] Bump to 11.0.x: `Microsoft.EntityFrameworkCore*`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`,
  `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore`, `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore`,
  `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`, `MockQueryable.NSubstitute`,
  Npgsql, NamingConventions. All of their 11.0 builds require EF Core 11.
- [ ] Add `Microsoft.EntityFrameworkCore.Design` (`PrivateAssets=all`) to Persistence: EF 11 Tools no
  longer brings it in.
- [ ] `MigrateAsync()` now throws when a context has no migrations (`RelationalEventId.MigrationsNotFound`).
  All three contexts have migrations today; keep it that way for tenant bootstrap.
- [ ] Npgsql 11 targets PostgreSQL 16 by default (we run 18 — fine).
- [ ] Run `dotnet ef migrations has-pending-model-changes` for Master, Tenant and PersistedGrant.
- [ ] Update the `dotnet-ef` global tool to 11 (it is on 10.0.2).
- [ ] Optional: `.config/dotnet-ef.json` for default `--project`/`--startup-project`; `MaxByAsync`/`FullJoin`.

## Before the next production deploy

- [ ] **Stripe:** move the webhook endpoint's API version in the Stripe dashboard to `2026-09-30.endive`.
  `StripeWebhookParser` passes `throwOnApiVersionMismatch: false`, so events still parse, but
  payloads keep the old shape until the endpoint is moved.
- [ ] **Anthropic:** one live request through `AnthropicLlmProvider` on `2026-10-21` (not run; costs money).
  The SDK now sends the key as `Authorization: Bearer` — nothing here reads `X-Api-Key`.
  License changed MIT → Apache 2.0 (update license tracking if any).
- [ ] **Open.IdentityServer 3.x:** the PAR endpoint is on by default. Disable it
  (`Endpoints.EnablePushedAuthorizationRequestEndpoint = false`) if no client will use it.
- [ ] **Hardware:** .NET 11 needs x86-64-v2 CPUs. Confirm the production host.

## Decided against (revisit only with a reason)

- **IDE0305** (`.ToList()` → `[.. ]`): wrapping long LINQ chains reads worse. Left at its default.
- **xUnit1051** (pass `TestContext.Current.CancellationToken`): off for `test/**`. Auto-fixing would pin
  NSubstitute setups and `Received()` checks to one token.
- **Not swept:** `DateTime.UtcNow` → `TimeProvider` (338 sites), `[LoggerMessage]` (485 calls),
  `IExceptionHandler`/ProblemDetails (would change the `Result` contract), HybridCache, Runtime Async
  (still preview), `required` members on DTOs (STJ would start rejecting payloads).
- **C# 15 unions / `closed`:** use them for new "one of N" types. Never `closed` on EF entities —
  lazy-loading proxies subclass them.

## Open follow-ups

- [ ] `EnforceCodeStyleInBuild` is on in `Directory.Build.props`; full build ≈ 30 s. Move it to CI only
  (`-p:EnforceCodeStyleInBuild=true`) if local builds feel slow.
- [ ] 46 extension methods stay classic: 37 Mapperly `partial` mappers (its generator can't implement
  partials inside an extension block — retry when Mapperly 5 ships) and 9 with generic receivers or
  `ref`/attributed receivers.
- [ ] Sync-over-async: `CurrentTenantAccessor.GetCurrentTenant()` (`.GetAwaiter().GetResult()` on every
  request), `GeotabEldService:45`, `TelegramBot/Registrar.cs:106`, `IdentityServer/Setup.cs:201`
  (with `Thread.Sleep`).
- [ ] Comment pass outside `test/Logistics.Architecture.Tests`: same pattern of history narration and
  references to plans that no longer exist. The `cleanup` skill covers it.
- [ ] `Logistics.slnx` has an uncommitted entry adding `private/tools/outreach` — commit or drop it.
- [ ] Bump the `private` submodule pointer (outreach commit is local, not pushed).
- [ ] When Mapperly 5.0 is GA: re-run the extension-block conversion on the mappers.
