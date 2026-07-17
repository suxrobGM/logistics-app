# CLAUDE.md

Multi-tenant fleet management platform for trucking companies (intermodal containers, vehicle transport, freight).

> **Finding code by feature?** Read [.claude/feature-map.md](.claude/feature-map.md) before grepping. It maps every feature (Loads, AI Dispatch, Stripe Connect, ELD, etc.) to its entity, handlers, infrastructure services, and frontend pages. Update it when you add a top-level feature.
>
> **Architecture deep-dive?** [docs/architecture/overview.md](docs/architecture/overview.md), [multi-tenancy.md](docs/architecture/multi-tenancy.md), [domain-model.md](docs/architecture/domain-model.md).

## Build & Run

```bash
# Local dev infrastructure (Postgres; runs the migrator once) - requires Docker
docker compose -f deploy/docker-compose.dev.yml up -d     # Postgres: 5433

# Backend only
dotnet build                                              # Build all
dotnet test                                               # Run all tests
dotnet test --filter "ClassName"                          # Filter by class
dotnet run --project src/Presentation/Logistics.IdentityServer  # Identity: https://localhost:7001
dotnet run --project src/Presentation/Logistics.API       # API: https://localhost:7000

# Frontend (Angular workspace, bun)
bun install
bun start:tms                                             # TMS Portal dev server

# Mobile (Kotlin Multiplatform)
cd src/Client/Logistics.DriverApp && ./gradlew assembleDebug
```

## Service Ports

| Service         | Port |
| --------------- | ---- |
| API             | 7000 |
| Identity Server | 7001 |
| Admin Portal    | 7002 |
| TMS Portal      | 7003 |
| Customer Portal | 7004 |
| Website         | 7005 |

## Architecture (first-pass facts)

- **DDD + CQRS**: Commands/Queries via MediatR in `src/Core/Logistics.Application/`. Requests implement `ICommand<T>` or `IQuery<T>` (in `Application.Abstractions/Common/`); handlers own their `SaveChangesAsync` calls (no auto-transaction wrapper)
- **Multi-tenant**: Master DB (tenants, subscriptions) + one DB per tenant. Tenant resolved per-request via `TenantService` (priority: MCP API key → `X-Tenant` header → JWT claim)
- **Lazy loading**: EF Core lazy loading enabled — do NOT use `.Include()` for navigation properties. The flip side: reading a navigation property inside a mapper or a list loop is an N+1. Batch the lookup and pass the value in (see [mapperly.md](.claude/rules/backend/mapperly.md))
- **Clean architecture**: Application references `Logistics.Application.Abstractions` for infrastructure ports, workflow services stay in `Logistics.Application`. Infrastructure projects depend on `Application.Abstractions` only — never on `Application`. Enforced by `test/Logistics.Architecture.Tests/`, which discovers projects rather than hard-coding lists — never reintroduce an `InlineData` roster there. Adding an infrastructure project: the csproj rule picks it up off disk automatically, but the IL-level boundary rule needs an anchor in `AssemblyAnchors.AllInfrastructure` plus a `ProjectReference` in the arch-tests csproj. Composition root in each presentation project's `Program.cs`
- **Modular infrastructure**: 14 focused projects under `src/Infrastructure/` (see [overview.md](docs/architecture/overview.md)). Shared HTTP-JSON plumbing for the third-party providers lives in `Integrations.Common` — do NOT hand-roll a fourth copy
- **Hangfire jobs bypass the MediatR pipeline**, so `[RequiresFeature]` is inert there. A job must check `IFeatureService` itself, and should fan out via `TenantJobRunner.ForEachTenantAsync` (`src/Presentation/Logistics.API/Jobs/`)

## User Roles

`SuperAdmin`, `Admin`, `Owner`, `Manager`, `Dispatcher`, `Driver`, `Customer`

## MCP Server (high-friction details)

- Endpoint: `/mcp` (Streamable HTTP)
- Auth: API key header, format `logsx_{tenantId}_{random}`. Validated by `ApiKeyAuthenticationHandler`, which sets `HttpContext.Items["McpTenantId"]` so `TenantService` resolves the tenant without an `X-Tenant` header
- Rate limit: 100 req/min per key
- Tools come from `AiDispatchToolRegistry` — single source of truth shared with the AI dispatch agent. Add a tool in one place, both surfaces pick it up
- Project: `src/Presentation/Logistics.McpServer/`
