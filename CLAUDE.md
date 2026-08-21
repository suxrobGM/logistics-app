# CLAUDE.md

Multi-tenant fleet management platform for trucking companies (intermodal containers, vehicle transport, freight).

## How guidance is organized

| Where                                            | What it holds                                                                                                                                                                                                                             | When to read it                                                                                     |
| ------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| **This file**                                    | Repo-wide architecture, commands, ports                                                                                                                                                                                                   | Always                                                                                              |
| [.claude/feature-map.md](.claude/feature-map.md) | Every feature → its entity, handlers, services, pages                                                                                                                                                                                     | **Before grepping for a feature.** Update it when you add or move a top-level feature               |
| Nested `CLAUDE.md`                               | Local guidance for a subtree, e.g. [the Angular workspace](src/Client/Logistics.Angular/CLAUDE.md)                                                                                                                                        | When working in that directory                                                                      |
| [.claude/rules/](.claude/rules/)                 | Conventions and traps, scoped by `paths:` frontmatter                                                                                                                                                                                     | Auto-loaded by Claude Code when you touch a matching file. **Other agents must read them manually** |
| [.claude/skills/](.claude/skills/)               | Step-by-step recipes for multi-file tasks (feature slice, webhook, job, provider, permission, migration)                                                                                                                                  | Before hand-rolling a task a skill already covers                                                   |
| [docs/](docs/index.md)                           | Deep dives - [architecture](docs/architecture/overview.md), [multi-tenancy](docs/architecture/multi-tenancy.md), [domain model](docs/architecture/domain-model.md), [AI dispatch](docs/ai-dispatch.md), [roadmap](docs/roadmap/README.md) | When you need the full picture, not just the location                                               |

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
- **Multi-tenant**: Master DB (tenants, subscriptions) + one DB per tenant. Tenant resolved per-request via `CurrentTenantAccessor` (`ICurrentTenantAccessor`) (priority: MCP API key → `X-Tenant` header → JWT claim)
- **Lazy loading**: EF Core lazy loading enabled - do NOT use `.Include()` for navigation properties. The flip side: reading a navigation property inside a mapper or a list loop is an N+1. Batch the lookup and pass the value in (see [mapperly.md](.claude/rules/backend/mapperly.md))
- **Modular infrastructure**: 14 focused projects under `src/Infrastructure/` (see [overview.md](docs/architecture/overview.md)). Shared HTTP-JSON plumbing, webhook signature validation (`WebhookSignature`), and the provider factory base for the third-party providers live in `Integrations.Common` - do NOT hand-roll a fourth copy
- **Hangfire jobs bypass the MediatR pipeline**, so `[RequiresFeature]` is inert there. A job must check `IFeatureService` itself, and should fan out via `TenantJobRunner.ForEachTenantAsync` (`src/Presentation/Logistics.API/Jobs/`)

### Layer boundaries

Application references `Logistics.Application.Abstractions` for infrastructure ports; workflow services stay in `Logistics.Application`. Infrastructure projects depend on `Application.Abstractions` only - **never** on `Application`. The composition root is each host's `Setup.cs` (`ConfigureServices`); `Program.cs` is a thin `LogisticsHost.Run` shell (`src/Presentation/Logistics.HostDefaults/`).

`test/Logistics.Architecture.Tests/` enforces this by **discovering** projects off disk - never reintroduce an `InlineData` roster there, since a hand-maintained list silently skips whatever nobody remembered to add.

Adding an infrastructure project: the csproj rule finds it automatically, but the IL-level boundary rule also needs an anchor in `AssemblyAnchors.AllInfrastructure` **plus** a `ProjectReference` in the arch-tests csproj. Miss those and the project is simply unchecked - nothing fails.

## User Roles

`SuperAdmin`, `Admin`, `Owner`, `Manager`, `Dispatcher`, `Driver`, `Customer`

## MCP Server (high-friction details)

- Endpoint: `/mcp` (Streamable HTTP)
- Auth: API key header, format `logsx_{tenantId}_{random}`. Validated by `ApiKeyAuthenticationHandler`, which sets `HttpContext.Items["McpTenantId"]` so `CurrentTenantAccessor` resolves the tenant without an `X-Tenant` header
- Rate limit: 100 req/min per key
- Tools come from `AgentToolCatalog`, which discovers them from the tool classes and is shared with the AI dispatch agent. Add a tool class and every surface picks it up
- Project: `src/Presentation/Logistics.McpServer/`
