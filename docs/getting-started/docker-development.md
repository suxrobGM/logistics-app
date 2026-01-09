# Docker Development with Aspire

Run the entire stack with a single command using .NET Aspire.

## Prerequisites

- Docker Desktop running
- .NET 10 SDK installed

## Quick Start

```bash
# Clone repository
git clone https://github.com/suxrobgm/logistics-app.git
cd logistics-app

# Run everything
dotnet run --project src/Aspire/Logistics.Aspire.AppHost
```

That's it! Aspire will:
- Start PostgreSQL in a container
- Run database migrations
- Start all .NET services
- Start the Angular app
- Open the Aspire dashboard

## Aspire Dashboard

Access the dashboard at **http://localhost:7100**

The dashboard provides:
- **Resources**: All running services and their status
- **Console**: Live console output from each service
- **Traces**: Distributed tracing across services
- **Metrics**: Performance metrics

## Service URLs

Once running, access:

| Service | URL |
|---------|-----|
| Aspire Dashboard | http://localhost:7100 |
| API (Swagger) | https://localhost:7000/swagger |
| Identity Server | https://localhost:7001 |
| Admin App | https://localhost:7002 |
| Office App | https://localhost:7003 |
| pgAdmin | http://localhost:5050 |

## What Aspire Runs

From `src/Aspire/Logistics.Aspire.AppHost/Program.cs`:

```
┌─────────────────────────────────────────────────────────────┐
│                    Aspire AppHost                           │
├─────────────────────────────────────────────────────────────┤
│  PostgreSQL Container (port 5432)                          │
│    ├── master_logistics database                           │
│    └── default_logistics database                          │
│                                                             │
│  pgAdmin Container (port 5050)                             │
│                                                             │
│  Logistics.DbMigrator (runs migrations, then exits)        │
│                                                             │
│  Logistics.IdentityServer (port 7001)                      │
│                                                             │
│  Logistics.API (port 7000)                                 │
│    └── Depends on: IdentityServer, PostgreSQL              │
│                                                             │
│  Logistics.AdminApp (port 7002)                            │
│    └── Depends on: API, IdentityServer                     │
│                                                             │
│  Logistics.OfficeApp via Bun (port 7003)                   │
│    └── Depends on: API, IdentityServer                     │
│                                                             │
│  Stripe CLI (optional, webhook listener)                   │
└─────────────────────────────────────────────────────────────┘
```

## Configure Stripe (Optional)

To enable payment testing:

1. Update `src/Aspire/Logistics.Aspire.AppHost/appsettings.json`:

   ```json
   {
     "Stripe": {
       "SecretKey": "sk_test_..."
     }
   }
   ```

2. Update `src/Presentation/Logistics.API/appsettings.json` with your Stripe keys

3. Restart Aspire - it will automatically run Stripe CLI

## Hot Reload

Changes to .NET code trigger automatic hot reload. For Angular:

```bash
# Angular watches for changes automatically
# If you need to restart just the Angular app:
cd src/Client/Logistics.OfficeApp
bun run start
```

## Debugging

### VS Code

1. Run Aspire normally
2. Use "Attach to Process" and select the service you want to debug

### Visual Studio

1. Open the solution
2. Set `Logistics.Aspire.AppHost` as startup project
3. Press F5 to debug

### Viewing Logs

In the Aspire dashboard, click on any service to see its console output.

Or via CLI:

```bash
docker logs logistics-postgres -f
```

## Stopping Services

Press `Ctrl+C` in the terminal running Aspire.

To clean up Docker resources:

```bash
docker compose down -v
```

## Troubleshooting

### Port Conflicts

If ports are in use, stop conflicting services or modify ports in `Program.cs`.

### Docker Not Running

Ensure Docker Desktop is running before starting Aspire.

### Database Not Ready

Aspire uses `WaitFor()` to ensure PostgreSQL is ready before starting dependent services. If issues persist:

```bash
# Check PostgreSQL container
docker ps | grep postgres
docker logs logistics-postgres
```

### Clear Everything and Restart

```bash
# Stop all containers
docker stop $(docker ps -aq)

# Remove logistics containers
docker rm $(docker ps -aq --filter "name=logistics")

# Remove volumes
docker volume rm $(docker volume ls -q --filter "name=logistics")

# Restart
dotnet run --project src/Aspire/Logistics.Aspire.AppHost
```

## Next Steps

- [Test Credentials](test-credentials.md) - Login credentials for testing
- [Local Development](local-development.md) - Manual setup without Docker
