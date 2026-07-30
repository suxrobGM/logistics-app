# Environment Variables

Configuration reference for LogisticsX Docker deployment.

## Docker Compose Environment (.env)

The `.env` file alongside `deploy/docker-compose.yml` (copied from `deploy/.env.example`)
configures all services. In CI it is created from the `DOCKER_ENV` GitHub secret.

### Images and Ports

Images are selected by `GITHUB_REPOSITORY` and `IMAGE_TAG`, which the deploy workflow
appends to `.env` automatically. The compose file defaults to `suxrobgm/logistics-app`
and `latest` if they are unset.

```bash
API_PORT=7000
IDENTITY_SERVER_PORT=7001
```

| Variable               | Description                                                       |
| ---------------------- | ----------------------------------------------------------------- |
| `GITHUB_REPOSITORY`    | GHCR image owner/repo (set by CI; default suxrobgm/logistics-app) |
| `IMAGE_TAG`            | Image tag to pull (set by CI; default latest)                     |
| `API_PORT`             | Port for the API service (default: 7000)                          |
| `IDENTITY_SERVER_PORT` | Port for the Identity Server (default: 7001)                      |

### Database (External PostgreSQL)

Production uses an external (installed) PostgreSQL instance instead of a containerized one.

```bash
ConnectionStrings__MasterDatabase="Host=localhost;Port=5432;Database=master_logisticsx;Username=postgres;Password=your-secure-password"
ConnectionStrings__UsTenantDatabase="Host=localhost;Port=5432;Database=us_logisticsx;Username=postgres;Password=your-secure-password"
ConnectionStrings__EuTenantDatabase="Host=localhost;Port=5432;Database=eu_logisticsx;Username=postgres;Password=your-secure-password"
ConnectionStrings__SoloTenantDatabase="Host=localhost;Port=5432;Database=solo_logisticsx;Username=postgres;Password=your-secure-password"
```

| Variable                                | Description                                                       |
| --------------------------------------- | ----------------------------------------------------------------- |
| `ConnectionStrings__MasterDatabase`     | Full connection string for the master database                    |
| `ConnectionStrings__UsTenantDatabase`   | Fallback tenant database the API registers `TenantDbContext` with |
| `ConnectionStrings__EuTenantDatabase`   | EU demo tenant database - read by the migrator only               |
| `ConnectionStrings__SoloTenantDatabase` | Owner-operator demo tenant database - read by the migrator only   |

### Stripe Integration

```bash
Stripe__SecretKey="sk_live_xxx"
Stripe__WebhookSecret="whsec_xxx"
```

| Variable                | Description                           |
| ----------------------- | ------------------------------------- |
| `Stripe__SecretKey`     | Stripe API secret key                 |
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

| Variable              | Description                                           |
| --------------------- | ----------------------------------------------------- |
| `Resend__ApiKey`      | Resend API key from resend.com dashboard              |
| `Resend__SenderEmail` | Sender email address (must be from a verified domain) |
| `Resend__SenderName`  | Display name for the sender                           |

### Mapbox (Optional)

```bash
Mapbox__AccessToken="pk.xxx"
```

### LLM API (Optional - AI Dispatch)

```bash
Llm__Providers__Anthropic__ApiKey="sk-ant-xxx"
```

| Variable                            | Description                                                                        |
| ----------------------------------- | ---------------------------------------------------------------------------------- |
| `Llm__Providers__Anthropic__ApiKey` | Anthropic API key for AI dispatch agent                                            |
| `Llm__Providers__OpenAi__ApiKey`    | OpenAI API key (alternative provider)                                              |
| `Llm__Providers__DeepSeek__ApiKey`  | DeepSeek API key (alternative provider)                                            |
| `Llm__DefaultProvider`              | Default LLM provider: `Anthropic`, `OpenAI`, `DeepSeek`, `Glm` (default: `OpenAI`) |

### TMS Portal (Runtime)

The TMS portal Docker image uses runtime environment variable substitution for secrets. These are injected at container startup via the entrypoint script.

```bash
# Mapped from Mapbox__AccessToken in docker-compose.yml
MAPBOX_TOKEN="pk.xxx"
```

| Variable       | Description                         |
| -------------- | ----------------------------------- |
| `MAPBOX_TOKEN` | Mapbox public access token for maps |

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

| Variable                           | Description                                                  |
| ---------------------------------- | ------------------------------------------------------------ |
| `SuperAdmin__*`                    | Initial super admin account credentials (synced on each run) |
| `TenantDatabaseDefaults__Password` | Password used when provisioning new tenant databases         |

#### Demo tenants (`Tenants[]`)

The migrator seeds one demo tenant per entry in the `Tenants` array of its
`appsettings.json`. `DemoTenantsSeeder` reads the whole array, so adding an entry is all it takes to
get a new demo company; there is no other list to keep in sync.

| Field              | Required | Description                                                                                                                                                                                   |
| ------------------ | -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Name`             | yes      | Tenant slug, lowercase. This is the value clients send in the `X-Tenant` header, and the `{tenant}` token in the `{tenant}_logisticsx` database name.                                         |
| `CompanyName`      | yes      | Display name (e.g. `Heartland Logistics LLC`).                                                                                                                                                |
| `BillingEmail`     | yes      | Billing contact on the tenant record.                                                                                                                                                         |
| `Region`           | yes      | `Us` or `Eu`. Drives currency, units, date format, timezone, and the region's seed route points and company address.                                                                          |
| `SeedDataKey`      | no       | Top-level section in `SeedData/*.json` this tenant draws its users and customers from. Defaults to the region name. **Two tenants in the same region need distinct keys** - see below.        |
| `OperatingMode`    | no       | `Fleet` (default) or `SoloOperator`.                                                                                                                                                          |
| `DataScale`        | no       | Multiplier on the fake-data volumes. `1.0` (default) is the fleet-sized demo of 100 loads; `0.12` gives a one-truck tenant 12. Scaled counts never floor below 1.                             |
| `ConnectionString` | no       | Explicit connection string. When omitted, the seeder falls back to `ConnectionStrings:{Name}TenantDatabase` (`us` -> `UsTenantDatabase`), which is the slot Aspire's `WithReference()` fills. |

`SeedDataKey` is load-bearing because a seed user belongs to exactly one tenant (`User.TenantId`).
Two tenants sharing a key means the second run re-homes the first tenant's logins onto the second
tenant. `us` and `eu` leave it unset and inherit their region names; `solo` sets `Solo`.

In Docker the array is set through the double-underscore index form, and the index must match the
tenant's position in `appsettings.json`:

```yaml
Tenants__0__ConnectionString: "Host=postgres;Port=5432;Database=us_logisticsx;Username=postgres;Password=Test12345#"
Tenants__1__ConnectionString: "Host=postgres;Port=5432;Database=eu_logisticsx;Username=postgres;Password=Test12345#"
Tenants__2__ConnectionString: "Host=postgres;Port=5432;Database=solo_logisticsx;Username=postgres;Password=Test12345#"
```

`Tenants__N__ConnectionString` wins over the `ConnectionStrings:{Name}TenantDatabase` fallback, which
is why `deploy/docker-compose.dev.yml` uses it: the containerized run needs `Host=postgres`, not the
`Host=localhost` baked into `appsettings.json`.

### ASP.NET Core

```bash
ASPNETCORE_ENVIRONMENT="Production"
```

## Complete .env Example

```bash
# Ports (images are selected by GITHUB_REPOSITORY + IMAGE_TAG, appended by CI)
API_PORT=7000
IDENTITY_SERVER_PORT=7001

# Database (external PostgreSQL)
ConnectionStrings__MasterDatabase="Host=localhost;Port=5432;Database=master_logisticsx;Username=postgres;Password=your-secure-password"
ConnectionStrings__UsTenantDatabase="Host=localhost;Port=5432;Database=us_logisticsx;Username=postgres;Password=your-secure-password"
ConnectionStrings__EuTenantDatabase="Host=localhost;Port=5432;Database=eu_logisticsx;Username=postgres;Password=your-secure-password"
ConnectionStrings__SoloTenantDatabase="Host=localhost;Port=5432;Database=solo_logisticsx;Username=postgres;Password=your-secure-password"

# Stripe
Stripe__SecretKey="sk_live_xxx"
Stripe__WebhookSecret="whsec_xxx"

# Super Admin and Tenant Database
SuperAdmin__Email="admin@yourdomain.com"
SuperAdmin__Password="YourSecurePassword123#"
SuperAdmin__FirstName="Admin"
SuperAdmin__LastName="Admin"
TenantDatabaseDefaults__Password="your-secure-tenant-db-password"

# Resend (Email)
Resend__ApiKey="re_your_api_key_here"
Resend__SenderEmail="noreply@logisticsx.app"
Resend__SenderName="LogisticsX"

# Optional: Mapbox
Mapbox__AccessToken="pk.xxx"

# Optional: LLM API (AI Dispatch)
Llm__Providers__Anthropic__ApiKey="sk-ant-xxx"
```

## API Configuration (appsettings.json)

For local development, configure `src/Presentation/Logistics.API/appsettings.json`:

### Database Connections

```json
{
  "ConnectionStrings": {
    "MasterDatabase": "Host=localhost;Port=5432;Database=master_logisticsx;Username=postgres;Password=password",
    "UsTenantDatabase": "Host=localhost;Port=5432;Database=us_logisticsx;Username=postgres;Password=password"
  },
  "TenantDatabaseDefaults": {
    "NameTemplate": "{tenant}_logisticsx",
    "Host": "localhost",
    "UserId": "postgres",
    "Password": "password"
  }
}
```

### Identity Server

```json
{
  "IdentityServer": {
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
