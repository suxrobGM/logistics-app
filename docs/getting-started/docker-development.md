# Docker Development

Run local infrastructure (PostgreSQL + the database migrator) in Docker, then run the .NET services and Angular apps on the host.

## Prerequisites

- Docker Desktop running
- .NET 10 SDK
- Bun (for the Angular apps)

## Quick Start

```bash
git clone https://github.com/suxrobgm/logistics-app.git
cd logistics-app

# Start Postgres and run the migrator once (creates DBs + seeds dev data)
docker compose -f deploy/docker-compose.dev.yml up -d
```

Then run the backend and a frontend on the host:

```bash
dotnet run --project src/Presentation/Logistics.IdentityServer   # https://localhost:7001
dotnet run --project src/Presentation/Logistics.API              # https://localhost:7000

cd src/Client/Logistics.Angular
bun install
bun start:tms        # also: start:admin, start:customer, start:website
```

## What the dev compose runs

From `deploy/docker-compose.dev.yml`:

| Service  | URL / Port            | Notes                                                     |
| -------- | --------------------- | --------------------------------------------------------- |
| postgres | localhost:5433        | Postgres 18; data in the `logistics-pg-data` volume       |
| migrator | runs once, then exits | Migrates master + tenant DBs, seeds the `us`/`eu` tenants |

The migrator builds from `src/Presentation/Logistics.DbMigrator/Dockerfile` and creates the `master_logisticsx`, `us_logisticsx`, and `eu_logisticsx` databases.

## Service URLs

| Service         | URL                            |
| --------------- | ------------------------------ |
| API (Swagger)   | https://localhost:7000/swagger |
| Identity Server | https://localhost:7001         |
| Admin Portal    | http://localhost:7002          |
| TMS Portal      | http://localhost:7003          |
| Customer Portal | http://localhost:7004          |
| Website         | http://localhost:7005          |

## App configuration / secrets

The .NET services read connection strings and secrets from their own `appsettings.Development.json` / user-secrets. The dev compose only provides the database; point the API and Identity Server at `Host=localhost;Port=5433` (matching the published Postgres port).

## Re-running migrations

```bash
docker compose -f deploy/docker-compose.dev.yml up migrator
```

## Stopping / cleaning up

```bash
docker compose -f deploy/docker-compose.dev.yml down        # stop
docker compose -f deploy/docker-compose.dev.yml down -v     # stop + drop the DB volume
```

## Next Steps

- [Test Credentials](test-credentials.md) — login credentials for testing
- [Local Development](local-development.md) — running everything on the host without Docker
