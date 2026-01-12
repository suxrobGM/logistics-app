# Environment Variables

Configuration reference for Logistics TMS Docker deployment.

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

### Database

```bash
POSTGRES_PASSWORD="your-secure-postgres-password"
```

The database connection strings are configured automatically using this password.

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

### SMTP Email (Optional)

```bash
Smtp__SenderEmail="noreply@example.com"
Smtp__SenderName="Logistics NoReply"
Smtp__UserName="smtp-username"
Smtp__Password="smtp-password"
Smtp__Host="smtp.example.com"
Smtp__Port="587"
```

### Mapbox (Optional)

```bash
Mapbox__AccessToken="pk.xxx"
```

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
| `PopulateFakeData` | Set to `true` to seed demo data |
| `SuperAdmin__*` | Initial super admin account credentials |
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

# Database
POSTGRES_PASSWORD="your-secure-postgres-password"

# Identity Server
IdentityServer__Authority="http://identity-server:7001"
IdentityServer__RequireHttpsMetadata="false"

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

# Optional: SMTP
Smtp__SenderEmail="noreply@yourdomain.com"
Smtp__SenderName="Logistics NoReply"
Smtp__UserName="smtp-username"
Smtp__Password="smtp-password"
Smtp__Host="smtp.yourdomain.com"
Smtp__Port="587"

# Optional: Mapbox
Mapbox__AccessToken="pk.xxx"
```

## API Configuration (appsettings.json)

For local development, configure `src/Presentation/Logistics.API/appsettings.json`:

### Database Connections

```json
{
  "ConnectionStrings": {
    "MasterDatabase": "Host=localhost;Port=5432;Database=master_logistics;Username=postgres;Password=password",
    "DefaultTenantDatabase": "Host=localhost;Port=5432;Database=default_logistics;Username=postgres;Password=password"
  },
  "TenantsDatabaseConfig": {
    "DatabaseNameTemplate": "{tenant}_logistics",
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
    "Audience": "logistics.api",
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
