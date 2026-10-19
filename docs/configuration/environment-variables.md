# Environment Variables

Configuration reference for LogisticsX Docker deployment.

## Docker Compose Environment (.env)

The `.env` file alongside `deploy/docker-compose.yml` (copied from `deploy/.env.example`)
configures all services. In CI it is created from the `DOCKER_ENV` GitHub secret.

There are no `appsettings.Production.json` files - `.env` (plus the few literals in
`docker-compose.yml`) is the only source of production configuration. The `appsettings.json`
committed with each service holds development defaults; everything environment-specific is
layered on top as `Section__Key` variables.

### Images and Ports

Images are selected by `GITHUB_REPOSITORY` and `IMAGE_TAG`, which the deploy workflow
appends to `.env` automatically. The compose file defaults to `suxrobgm/logistics-app`
and `latest` if they are unset.

```bash
API_PORT=7000
IDENTITY_SERVER_PORT=7001
```

| Variable | Description |
| --- | --- |
| `GITHUB_REPOSITORY` | GHCR image owner/repo (set by CI; default suxrobgm/logistics-app) |
| `IMAGE_TAG` | Image tag to pull (set by CI; default latest) |
| `API_PORT` | Port for the API service (default: 7000) |
| `IDENTITY_SERVER_PORT` | Port for the Identity Server (default: 7001) |

### Database (External PostgreSQL)

Production uses an external (installed) PostgreSQL instance instead of a containerized one.

```bash
ConnectionStrings__MasterDatabase="Host=localhost;Port=5432;Database=master_logisticsx;Username=postgres;Password=your-secure-password"
ConnectionStrings__USTenantDatabase="Host=localhost;Port=5432;Database=us_logisticsx;Username=postgres;Password=your-secure-password"
```

| Variable | Description |
| --- | --- |
| `ConnectionStrings__MasterDatabase` | Full connection string for the master database |
| `ConnectionStrings__USTenantDatabase` | Fallback tenant database the API registers `TenantDbContext` with |

### Stripe Integration

```bash
Stripe__SecretKey="sk_live_xxx"
Stripe__WebhookSecret="whsec_xxx"
```

| Variable | Description |
| --- | --- |
| `Stripe__SecretKey` | Stripe API secret key |
| `Stripe__WebhookSecret` | Webhook signature verification secret |

### Google reCAPTCHA (Optional)

```bash
GoogleRecaptcha__SiteKey="your-site-key"
GoogleRecaptcha__SecretKey="your-secret-key"
```

### Resend Email

```bash
Resend__ApiKey="re_your_api_key_here"
Resend__SenderEmail="noreply@logisticsx.app"
Resend__SenderName="LogisticsX"
```

| Variable | Description |
| --- | --- |
| `Resend__ApiKey` | Resend API key from resend.com dashboard |
| `Resend__SenderEmail` | Sender email address (must be from a verified domain) |
| `Resend__SenderName` | Display name for the sender |

### Notification Email

```bash
NotificationEmail="you@example.com"
```

Website demo requests and contact messages are emailed here, with the sender as the reply-to.

### Mapbox (Optional)

```bash
Mapbox__AccessToken="pk.xxx"
```

### LLM API (Optional - AI Dispatch)

```bash
Llm__Providers__OpenAi__ApiKey="sk-xxx"
```

| Variable | Description |
| --- | --- |
| `Llm__Providers__OpenAi__ApiKey` | OpenAI API key, used by the default dispatch model (`gpt-6-luna`) |
| `Llm__Providers__Anthropic__ApiKey` | Anthropic API key (alternative provider) |
| `Llm__Providers__DeepSeek__ApiKey` | DeepSeek API key (alternative provider) |
| `Llm__DefaultProvider` | Default LLM provider: `Anthropic`, `OpenAI`, `DeepSeek`, `Glm` (default: `OpenAI`) |
| `Llm__DefaultReasoningEffort` | `None`, `Low`, `Medium`, `High`, `XHigh` or `Max` (default: `Medium`) |
| `Llm__PolicyLearningModel` | Model for the nightly dispatch-policy learning pass (default: `deepseek-flash`) |

### FMCSA Broker Credit Lookup (Optional)

```bash
Fmcsa__WebKey="your-fmcsa-webkey"
```

| Variable | Description |
| --- | --- |
| `Fmcsa__WebKey` | Free webKey from <https://mobile.fmcsa.dot.gov/QCDevsite/>. Load board broker vetting skips FMCSA checks when it's unset. |

### Portals (Runtime)

The portal Docker images use runtime environment variable substitution. These values are injected at container startup by the shared SPA entrypoint script (`private/src/Client/Logistics.Angular/deploy/docker-entrypoint-spa.sh`), which all three portals run.

```bash
# Mapped from Mapbox__AccessToken in docker-compose.yml
MAPBOX_TOKEN="pk.xxx"
```

| Variable | Description |
| --- | --- |
| `MAPBOX_TOKEN` | Mapbox public access token for maps (TMS portal) |
| `SENTRY_DSN`, `SENTRY_ENVIRONMENT`, `SENTRY_RELEASE` | Mapped by compose from the Sentry variables in `deploy/.env.example` |

### Sentry (Optional - Error Tracking)

Off unless a DSN is set. The variables and their defaults are listed in the Sentry block of
[`deploy/.env.example`](../../deploy/.env.example); setup, scrubbing and the tenant tag are in
[Observability and Backups](../deployment/observability-and-backups.md#error-tracking-sentry).

### Database Backups (Optional)

Read only by `deploy/docker-compose.backup.yml`. The variables are listed in the backups block of
[`deploy/.env.example`](../../deploy/.env.example); setup and restore are in
[Observability and Backups](../deployment/observability-and-backups.md#database-backups).

### Database Migrator (run separately)

These are read by `Logistics.DbMigrator`, not the API container. The migrator is not part
of the production stack - run it manually (or via `deploy/docker-compose.dev.yml` locally)
to apply migrations and seed the super-admin account. The local dev infra lives in `deploy/docker-compose.dev.yml`.

```bash
SuperAdmin__Email="admin@example.com"
SuperAdmin__Password="YourSecurePassword123#"
SuperAdmin__FirstName="Admin"
SuperAdmin__LastName="Admin"
TenantDatabaseDefaults__Password="your-secure-tenant-db-password"
```

| Variable | Description |
| --- | --- |
| `SuperAdmin__*` | Initial super admin account credentials (synced on each run) |
| `TenantDatabaseDefaults__Password` | Password for every tenant database. A tenant stores only its database name; host, port and credentials come from `TenantDatabaseDefaults__*` (API, Identity Server and migrator) |

#### Running the migrator against production

`deploy/run-prod-migrator.ps1` is the supported way to migrate production. It exists so a
production migration is never one stray `dotnet run` away: the migrator has no
`appsettings.Production.json` to fall back on, and its committed `appsettings.json` points at
`localhost`.

```powershell
powershell -NoProfile -File .\deploy\run-prod-migrator.ps1
```

The script (PowerShell 5.1 compatible, resolves everything from its own folder):

1. Parses `deploy/.env` and exports every entry as a process-scoped environment variable.
2. Forces `DOTNET_ENVIRONMENT=Production` and `ASPNETCORE_ENVIRONMENT=Production`.
3. Prints a red banner naming the target database **host** (parsed out of
   `ConnectionStrings__MasterDatabase`; the password is never echoed) and waits for you to type
   `migrate-prod`. Any other input aborts before a connection is opened.
4. Runs `dotnet run --project src/Presentation/Logistics.DbMigrator -- --exit` and exits with
   the migrator's exit code.

Because it reads `.env`, the production values for `ConnectionStrings__MasterDatabase`,
`TenantDatabaseDefaults__*` and `SuperAdmin__*`
below must all be present there - `.env.example` marks which ones the migrator needs.

#### Demo tenants (`Tenants[]`)

The migrator seeds one demo tenant per entry in the `Tenants` array of its
`appsettings.json`. `DemoTenantsSeeder` reads the whole array, so adding an entry is all it takes to
get a new demo company; there is no other list to keep in sync.

| Field | Required | Description |
| --- | --- | --- |
| `Name` | yes | Tenant slug, lowercase. This is the value clients send in the `X-Tenant` header, and the `{tenant}` token in the `{tenant}_logisticsx` database name. |
| `CompanyName` | yes | Display name (e.g. `Heartland Logistics LLC`). |
| `BillingEmail` | yes | Billing contact on the tenant record. |
| `Region` | yes | `Us` or `Eu`. Drives currency, units, date format, timezone, and the region's seed route points and company address. |
| `SeedDataKey` | no | Top-level section in `SeedData/*.json` this tenant draws its users and customers from. Defaults to the region name. **Two tenants in the same region need distinct keys** - see below. |
| `Presets` | no | Tenant presets, e.g. `["GeneralFreight", "SoloOperator"]`. Empty means `GeneralFreight`. When the list changes, the next run re-applies the preset features. |
| `DataScale` | no | Multiplier on the fake-data volumes. `1.0` (default) is the fleet-sized demo of 100 loads; `0.12` gives a one-truck tenant 12. Scaled counts never floor below 1. |

`SeedDataKey` is load-bearing because a seed user belongs to exactly one tenant (`User.TenantId`).
Two tenants sharing a key means the second run re-homes the first tenant's logins onto the second
tenant. `us` and `eu` leave it unset and inherit their region names; `solo` sets `Solo`.

A demo tenant's database name comes from `TenantDatabaseDefaults:NameTemplate`, and it is reached
through `TenantDatabaseDefaults` like every other tenant. To point the migrator at another server,
override `TenantDatabaseDefaults__Host` (and port or credentials) - `deploy/docker-compose.dev.yml`
sets it to `postgres`.

### Commercial License

```bash
License__Key=""
License__HeartbeatEnabled=true
#License__HeartbeatUrl="https://api.logisticsx.app/license/heartbeat"
```

| Variable | Description |
| --- | --- |
| `License__Key` | Signed commercial license key. When set it wins over the key installed on the admin portal License page. Leave empty for noncommercial use |
| `License__HeartbeatEnabled` | Daily instance report to the author (instance id, hostname, version, key id, licensee, tenant count). Required under a commercial license |
| `License__HeartbeatUrl` | Receiver for the heartbeat. Only change it for a test receiver |

See [COMMERCIAL-LICENSE.md](../../COMMERCIAL-LICENSE.md) for the license types and what the heartbeat sends.

### ASP.NET Core

```bash
ASPNETCORE_ENVIRONMENT="Production"
```

## Complete .env Example

[deploy/.env.example](../../deploy/.env.example) is the full, current list. Copy it to `deploy/.env`. The QuickBooks variables are explained in [QuickBooks setup](../accounting-quickbooks-setup.md).

## API Configuration (appsettings.json)

For local development, configure `src/Presentation/Logistics.API/appsettings.json` (keep secrets
in the git-ignored `appsettings.Development.json` or user-secrets). In production the same
sections are supplied as `Section__Key` variables from `.env` - there is no
`appsettings.Production.json`:

### Database Connections

```json
{
  "ConnectionStrings": {
    "MasterDatabase": "Host=localhost;Port=5432;Database=master_logisticsx;Username=postgres;Password=password",
    "USTenantDatabase": "Host=localhost;Port=5432;Database=us_logisticsx;Username=postgres;Password=password"
  },
  "TenantDatabaseDefaults": {
    "NameTemplate": "{tenant}_logisticsx",
    "Host": "localhost",
    "UserId": "postgres",
    "Password": "password"
  }
}
```

### JWT bearer validation

The API validates tokens from the `Jwt` section. This is separate from the `IdentityServer`
section it also binds (`IdentityServerOptions.ExternalAuthority`, the public issuer URL used in
user-facing links) - putting `Audience` or `ValidIssuers` under `IdentityServer` is silently ignored.

```json
{
  "Jwt": {
    "Authority": "http://localhost:7001",
    "Audience": "logisticsx.api",
    "ValidIssuers": [
      "http://localhost:7001",
      "https://localhost:7001",
      "https://id.yourdomain.com",
      "http://identity-server:7001"
    ]
  }
}
```

### Stripe

```json
{
  "Stripe": {
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  }
}
```

## Security Notes

1. Never commit secrets to version control
2. Use different credentials for dev/staging/production
3. Rotate secrets regularly
4. Use strong passwords (16+ characters)
5. The `.env` file should have restricted permissions (`chmod 600`)
