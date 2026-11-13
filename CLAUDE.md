# CLAUDE.md

Multi-tenant fleet management platform for trucking companies (intermodal containers, vehicle transport, freight).

- **Before grepping for a feature**, read [.claude/feature-map.md](.claude/feature-map.md): the folder conventions plus the features that break them. Add a row only when a feature doesn't follow the conventions.
- Multi-phase work keeps a follow-up list in [.claude/notes/](.claude/notes/); tick items off as they land.
- Deep dives: [docs/](docs/README.md) ([architecture](docs/architecture/overview.md), [multi-tenancy](docs/architecture/multi-tenancy.md), [domain model](docs/architecture/domain-model.md), [AI dispatch](docs/ai-dispatch.md), [roadmap](docs/roadmap/README.md)).
- [private/](private/README.md) is a closed-source submodule (web portals, driver app) with its own `CLAUDE.md`. Empty in a public clone.

## Working rules

- **Never push** in this repo or `private/`. The user reviews and pushes.
- Commits: focused, one concern each, on the current branch, no `Co-Authored-By` or other footer.
- Never use git worktrees. Implementation agents run one at a time in the main checkout; only read-only agents run in parallel.
- Agents don't see the user's memory, so briefs must carry any preference not written in `.claude/rules/`.

## Build & Run

```bash
dotnet build
dotnet test                                                     # xUnit v3 on Microsoft.Testing.Platform
dotnet test --filter-class "*ClassName"
dotnet run --project src/Presentation/Logistics.IdentityServer  # http://localhost:7001
dotnet run --project src/Presentation/Logistics.API             # http://localhost:7000
bun start:tms                                                   # TMS portal, from the repo root
```

Dev uses a native Postgres on `:5432`; every `appsettings.json` points there. `deploy/docker-compose.dev.yml` runs Postgres on `:5433` plus the migrator for those without one.

| Service | Port | | Service | Port |
| --- | --- | --- | --- | --- |
| API | 7000 | | TMS Portal | 7003 |
| Identity Server | 7001 | | Customer Portal | 7004 |
| Admin Portal | 7002 | | Website | 7005 |

## Architecture

- **DDD + CQRS** through the in-repo `Logistics.Mediator`. Requests implement `ICommand<T>` / `IQuery<T>` (`Application.Abstractions/Common/`). Handlers call `SaveChangesAsync` themselves; there is no transaction wrapper.
- **Multi-tenant:** a master DB (tenants, subscriptions) plus one DB per tenant. `CurrentTenantAccessor` resolves the tenant per request: MCP API key, then `X-Tenant` header, then JWT claim.
- **Lazy loading is on.** Never `.Include()`. A navigation read in a mapper or loop is an N+1.
- **Infrastructure** is 8 projects under `src/Infrastructure/`. Shared HTTP-JSON plumbing, webhook signatures and keyed provider registration (`AddKeyedProvider` / `GetRequiredProvider`) are in `Infrastructure.Integrations/Common/`; don't hand-roll another copy.
- **Outbound HTTP** gets the standard resilience handler from `LogisticsHost` (no retries for unsafe methods). Never stack a second retry handler. The `llm` client opts out because the 30s total would cut agent turns.
- **Hangfire jobs bypass the request pipeline**, so `[RequiresFeature]` is inert there. Jobs check `IFeatureService` themselves and fan out with `TenantJobRunner.ForEachTenantAsync`.

### Layer boundaries

Application uses `Logistics.Application.Abstractions` for infrastructure ports. Infrastructure projects depend on `Application.Abstractions` only, never `Application`. Each host's composition root is `Setup.cs`; `Program.cs` is a thin `LogisticsHost.Run` shell.

`test/Logistics.Architecture.Tests/` discovers projects from disk; never reintroduce a hand-kept `InlineData` roster. A new infrastructure project still needs an anchor in `AssemblyAnchors.AllInfrastructure` and a `ProjectReference` in the arch-tests csproj, or it goes silently unchecked. A new `src/Core` project needs an anchor, an entry in `LoadAssemblies` and a rule pinning its place (see `Logistics.Mediator`).

## Roles

`SuperAdmin`, `Admin`, `Owner`, `Manager`, `Dispatcher`, `Driver`, `Customer`.

## MCP server

`src/Presentation/Logistics.McpServer/`, endpoint `/mcp` (Streamable HTTP). API keys look like `logsx_{tenantId}_{random}`; `ApiKeyAuthenticationHandler` sets `HttpContext.Items["McpTenantId"]`, so no `X-Tenant` header is needed. 100 requests/min per key. Tools come from `AgentToolCatalog`, shared with the AI agents.
