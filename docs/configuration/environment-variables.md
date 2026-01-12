# Environment Variables

Configuration reference for Logistics TMS Docker deployment.

## Docker Compose Environment (.env)

The `.env` file in `src/Aspire/Logistics.Aspire.AppHost/aspire-output/` configures all services.

### Container Images

```bash
API_IMAGE="ghcr.io/suxrobgm/logistics-app/api:latest"
ADMIN_APP_IMAGE="ghcr.io/suxrobgm/logistics-app/admin:latest"
IDENTITY_SERVER_IMAGE="ghcr.io/suxrobgm/logistics-app/identity:latest"
MIGRATOR_IMAGE="ghcr.io/suxrobgm/logistics-app/migrator:latest"
```

### Database

```bash
POSTGRES_PASSWORD="your-secure-postgres-password"
```

The database connection strings are configured automatically using this password.

### Identity Server

```bash
IdentityServer__Authority="http://identity-server:7001"
IdentityServer__RequireHttpsMetadata="false"
```

| Variable | Description |
|----------|-------------|
| `IdentityServer__Authority` | Internal Docker network URL for API-to-IdentityServer communication |
| `IdentityServer__RequireHttpsMetadata` | Set to `false` since SSL terminates at nginx |

### Stripe Integration

```bash
StripeConfig__SecretKey="sk_live_xxx"
StripeConfig__WebhookSecret="whsec_xxx"
StripeConfig__PublishableKey="pk_live_xxx"
STRIPE_API_KEY="sk_live_xxx"
```

| Variable | Description |
|----------|-------------|
| `StripeConfig__SecretKey` | Stripe API secret key |
| `StripeConfig__PublishableKey` | Stripe publishable key (client-side) |
| `StripeConfig__WebhookSecret` | Webhook signature verification secret |
| `STRIPE_API_KEY` | Used by Stripe CLI for webhook forwarding |

### Google reCAPTCHA (Optional)

```bash
GoogleRecaptchaConfig__SiteKey="your-site-key"
GoogleRecaptchaConfig__SecretKey="your-secret-key"
```

### SMTP Email (Optional)

```bash
SmtpConfig__SenderEmail="noreply@example.com"
SmtpConfig__SenderName="Logistics NoReply"
SmtpConfig__UserName="smtp-username"
SmtpConfig__Password="smtp-password"
SmtpConfig__Host="smtp.example.com"
SmtpConfig__Port="587"
```

### Mapbox (Optional)

```bash
Mapbox__AccessToken="pk.xxx"
```

### Azure Blob Storage (Optional)

```bash
BlobStorage__Type="file"
ConnectionStrings__AzureBlobStorage="DefaultEndpointsProtocol=https;AccountName=xxx;AccountKey=xxx"
```

Set `BlobStorage__Type` to `azure` to use Azure Blob Storage, or `file` for local storage.

### Database Migrator

```bash
PopulateFakeData="false"
SuperAdmin__Email="admin@example.com"
SuperAdmin__Password="YourSecurePassword123#"
SuperAdmin__FirstName="Admin"
SuperAdmin__LastName="Admin"
```

| Variable | Description |
|----------|-------------|
| `PopulateFakeData` | Set to `true` to seed demo data |
| `SuperAdmin__*` | Initial super admin account credentials |

### ASP.NET Core

```bash
ASPNETCORE_ENVIRONMENT="Production"
```

## Complete .env Example

```bash
# Container Images
API_IMAGE="ghcr.io/suxrobgm/logistics-app/api:latest"
ADMIN_APP_IMAGE="ghcr.io/suxrobgm/logistics-app/admin:latest"
IDENTITY_SERVER_IMAGE="ghcr.io/suxrobgm/logistics-app/identity:latest"
MIGRATOR_IMAGE="ghcr.io/suxrobgm/logistics-app/migrator:latest"

# Database
POSTGRES_PASSWORD="your-secure-postgres-password"

# Identity Server
IdentityServer__Authority="http://identity-server:7001"
IdentityServer__RequireHttpsMetadata="false"

# Stripe
StripeConfig__SecretKey="sk_live_xxx"
StripeConfig__WebhookSecret="whsec_xxx"
StripeConfig__PublishableKey="pk_live_xxx"

# Super Admin
SuperAdmin__Email="admin@yourdomain.com"
SuperAdmin__Password="YourSecurePassword123#"
SuperAdmin__FirstName="Admin"
SuperAdmin__LastName="Admin"

# Optional: SMTP
SmtpConfig__SenderEmail="noreply@yourdomain.com"
SmtpConfig__SenderName="Logistics NoReply"
SmtpConfig__UserName="smtp-username"
SmtpConfig__Password="smtp-password"
SmtpConfig__Host="smtp.yourdomain.com"
SmtpConfig__Port="587"

# Optional: Mapbox
Mapbox__AccessToken="pk.xxx"

# ASP.NET Core
ASPNETCORE_ENVIRONMENT="Production"
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
  "StripeConfig": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  }
}
```

### File Storage

```json
{
  "BlobStorage": {
    "Type": "File"
  },
  "FileBlobStorage": {
    "RootPath": "wwwroot/uploads",
    "RequestPath": "/uploads",
    "CacheSeconds": 3600
  }
}
```

## Security Notes

1. Never commit secrets to version control
2. Use different credentials for dev/staging/production
3. Rotate secrets regularly
4. Use strong passwords (16+ characters)
5. The `.env` file should have restricted permissions (`chmod 600`)
