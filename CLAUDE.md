# CLAUDE.md

Multi-tenant fleet management platform for trucking companies (intermodal containers, vehicle transport, freight).

> **Finding code by feature?** Read [.claude/feature-map.md](.claude/feature-map.md) before grepping. It maps every feature (Loads, AI Dispatch, Stripe Connect, ELD, etc.) to its entity, handlers, infrastructure services, and frontend pages. Update it when you add a top-level feature.
>
> **Architecture deep-dive?** [docs/architecture/overview.md](docs/architecture/overview.md), [multi-tenancy.md](docs/architecture/multi-tenancy.md), [domain-model.md](docs/architecture/domain-model.md).

## Build & Run

```bash
# Full stack (recommended) - requires Docker
dotnet run --project src/Aspire/Logistics.Aspire.AppHost  # Dashboard: http://localhost:7100

# Backend only
dotnet build                                              # Build all
dotnet test                                               # Run all tests
dotnet test --filter "ClassName"                          # Filter by class
dotnet run --project src/Presentation/Logistics.API       # API: https://localhost:7000

# Frontend (Angular workspace, bun)
bun install
bun start:tms                                             # TMS Portal dev server

# Mobile (Kotlin Multiplatform)
cd src/Client/Logistics.DriverApp && ./gradlew assembleDebug
```

## Service Ports

| Service          | Port |
| ---------------- | ---- |
| API              | 7000 |
| Identity Server  | 7001 |
| Admin Portal     | 7002 |
| TMS Portal       | 7003 |
| Customer Portal  | 7004 |
| Website          | 7005 |
| Aspire Dashboard | 7100 |

## Architecture (first-pass facts)

- **DDD + CQRS**: Commands/Queries via MediatR in `src/Core/Logistics.Application/`
- **Multi-tenant**: Master DB (tenants, subscriptions) + one DB per tenant. Tenant resolved per-request via `TenantService` (priority: MCP API key → `X-Tenant` header → JWT claim)
- **Lazy loading**: EF Core lazy loading enabled — do NOT use `.Include()` for navigation properties
- **Clean architecture**: Application references only interfaces; implementations in `src/Infrastructure/{module}/`. Composition root in each presentation project's `Program.cs`
- **Modular infrastructure**: 9 focused projects under `src/Infrastructure/` (see [overview.md](docs/architecture/overview.md))

## User Roles

`SuperAdmin`, `Admin`, `Owner`, `Manager`, `Dispatcher`, `Driver`, `Customer`

## MCP Server (high-friction details)

- Endpoint: `/mcp` (Streamable HTTP)
- Auth: API key header, format `logsx_{tenantId}_{random}`. Validated by `ApiKeyAuthenticationHandler`, which sets `HttpContext.Items["McpTenantId"]` so `TenantService` resolves the tenant without an `X-Tenant` header
- Rate limit: 100 req/min per key
- Tools come from `DispatchToolRegistry` — single source of truth shared with the AI dispatch agent. Add a tool in one place, both surfaces pick it up
- Project: `src/Presentation/Logistics.McpServer/`
