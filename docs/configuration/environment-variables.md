# Environment Variables

Configuration reference for LogisticsX Docker deployment.

## Docker Compose Environment (.env)

The `.env` file in `src/Aspire/Logistics.Aspire.AppHost/aspire-output/` configures all services.

### Container Images and Ports

```bash
API_IMAGE="ghcr.io/suxrobgm/logistics-app/api:latest"
ADMIN_APP_IMAGE="ghcr.io/suxrobgm/logistics-app/admin:latest"
IDENTITY_SERVER_IMAGE="ghcr.io/suxrobgm/logistics-app/identity:latest"
MIGRATOR_IMAGE="ghcr.io/suxrobgm/logistics-app/migrator:latest"

API_PORT=7000
IDENTITY_SERVER_PORT=7001
ADMIN_APP_PORT=7002
```

| Variable | Description |
|----------|-------------|
| `*_IMAGE` | Docker image references for each service |
| `API_PORT` | Port for the API service (default: 7000) |
| `IDENTITY_SERVER_PORT` | Port for the Identity Server (default: 7001) |
| `ADMIN_APP_PORT` | Port for the Admin App (default: 7002) |

### Database (External PostgreSQL)

Production uses an external (installed) PostgreSQL instance instead of a containerized one.

```bash
ConnectionStrings__MasterDatabase="Host=localhost;Port=5432;Database=master_logisticsx;Username=postgres;Password=your-secure-password"
ConnectionStrings__DefaultTenantDatabase="Host=localhost;Port=5432;Database=default_logisticsx;Username=postgres;Password=your-secure-password"
```

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__MasterDatabase` | Full connection string for the master database |
| `ConnectionStrings__DefaultTenantDatabase` | Full connection string for the default tenant database |

### Stripe Integration

```bash
Stripe__SecretKey="sk_live_xxx"
Stripe__WebhookSecret="whsec_xxx"
Stripe__PublishableKey="pk_live_xxx"
STRIPE_API_KEY="sk_live_xxx"
```

| Variable | Description |
|----------|-------------|
| `Stripe__SecretKey` | Stripe API secret key |
| `Stripe__PublishableKey` | Stripe publishable key (client-side) |
| `Stripe__WebhookSecret` | Webhook signature verification secret |
| `STRIPE_API_KEY` | Used by Stripe CLI for webhook forwarding |

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
|----------|-------------|
| `Resend__ApiKey` | Resend API key from resend.com dashboard |
| `Resend__SenderEmail` | Sender email address (must be from a verified domain) |
| `Resend__SenderName` | Display name for the sender |

### Mapbox (Optional)

```bash
Mapbox__AccessToken="pk.xxx"
```

### Claude API (Optional — AI Dispatch)

```bash
Claude__ApiKey="sk-ant-xxx"
```

| Variable | Description |
|----------|-------------|
| `Claude__ApiKey` | Anthropic Claude API key for the AI dispatch agent (Enterprise plan feature) |

### TMS Portal (Runtime)

The TMS portal Docker image uses runtime environment variable substitution for secrets. These are injected at container startup via the entrypoint script.

```bash
# Mapped from Mapbox__AccessToken and Stripe__PublishableKey in docker-compose.yaml
MAPBOX_TOKEN="pk.xxx"
STRIPE_PUBLISHABLE_KEY="pk_live_xxx"
```

| Variable | Description |
|----------|-------------|
| `MAPBOX_TOKEN` | Mapbox public access token for maps |
| `STRIPE_PUBLISHABLE_KEY` | Stripe publishable key (client-side) |

### Database Migrator

```bash
SuperAdmin__Email="admin@example.com"
SuperAdmin__Password="YourSecurePassword123#"
SuperAdmin__FirstName="Admin"
SuperAdmin__LastName="Admin"
TenantsDatabaseConfig__DatabasePassword="your-secure-tenant-db-password"
```

| Variable | Description |
|----------|-------------|
| `SuperAdmin__*` | Initial super admin account credentials (synced on each run) |
| `TenantsDatabaseConfig__DatabasePassword` | Password used when creating new tenant databases |

### ASP.NET Core

```bash
ASPNETCORE_ENVIRONMENT="Production"
```

## Complete .env Example

```bash
# Container Images and Ports
API_IMAGE="ghcr.io/suxrobgm/logistics-app/api:latest"
ADMIN_APP_IMAGE="ghcr.io/suxrobgm/logistics-app/admin:latest"
IDENTITY_SERVER_IMAGE="ghcr.io/suxrobgm/logistics-app/identity:latest"
MIGRATOR_IMAGE="ghcr.io/suxrobgm/logistics-app/migrator:latest"

API_PORT=7000
IDENTITY_SERVER_PORT=7001
ADMIN_APP_PORT=7002

# Database (external PostgreSQL)
ConnectionStrings__MasterDatabase="Host=localhost;Port=5432;Database=master_logisticsx;Username=postgres;Password=your-secure-password"
ConnectionStrings__DefaultTenantDatabase="Host=localhost;Port=5432;Database=default_logisticsx;Username=postgres;Password=your-secure-password"

# Stripe
Stripe__SecretKey="sk_live_xxx"
Stripe__WebhookSecret="whsec_xxx"
Stripe__PublishableKey="pk_live_xxx"

# Super Admin and Tenant Database
SuperAdmin__Email="admin@yourdomain.com"
SuperAdmin__Password="YourSecurePassword123#"
SuperAdmin__FirstName="Admin"
SuperAdmin__LastName="Admin"
TenantsDatabaseConfig__DatabasePassword="your-secure-tenant-db-password"

# Resend (Email)
Resend__ApiKey="re_your_api_key_here"
Resend__SenderEmail="noreply@logisticsx.app"
Resend__SenderName="LogisticsX"

# Optional: Mapbox
Mapbox__AccessToken="pk.xxx"

# Optional: Claude API (AI Dispatch)
Claude__ApiKey="sk-ant-xxx"
```

## API Configuration (appsettings.json)

For local development, configure `src/Presentation/Logistics.API/appsettings.json`:

### Database Connections

```json
{
  "ConnectionStrings": {
    "MasterDatabase": "Host=localhost;Port=5432;Database=master_logisticsx;Username=postgres;Password=password",
    "DefaultTenantDatabase": "Host=localhost;Port=5432;Database=default_logisticsx;Username=postgres;Password=password"
  },
  "TenantsDatabaseConfig": {
    "DatabaseNameTemplate": "{tenant}_logisticsx",
    "DatabaseHost": "localhost",
    "DatabaseUserId": "postgres",
    "DatabasePassword": "password"
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
    "PublishableKey": "pk_test_...",
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
