# Environment Variables

Complete configuration reference for Logistics TMS.

## API Configuration

Located in `src/Presentation/Logistics.API/appsettings.json`:

### Database

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

| Variable | Description |
|----------|-------------|
| `MasterDatabase` | Connection to master database (tenants, subscriptions) |
| `DefaultTenantDatabase` | Default tenant database connection |
| `DatabaseNameTemplate` | Pattern for tenant database names |

### Identity Server

```json
{
  "IdentityServer": {
    "Authority": "https://localhost:7001",
    "Audience": "logistics.api",
    "ValidIssuers": [
      "https://localhost:7001",
      "http://localhost:7001"
    ]
  }
}
```

| Variable | Description |
|----------|-------------|
| `Authority` | Identity Server URL |
| `Audience` | API resource identifier |
| `ValidIssuers` | Accepted token issuers |

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

| Variable | Description |
|----------|-------------|
| `PublishableKey` | Stripe publishable key (client-side) |
| `SecretKey` | Stripe secret key (server-side) |
| `WebhookSecret` | Webhook signature verification |

### Email (SMTP)

```json
{
  "SmtpConfig": {
    "SenderEmail": "noreply@yourdomain.com",
    "SenderName": "Logistics TMS",
    "UserName": "smtp_username",
    "Password": "smtp_password",
    "Host": "smtp.yourdomain.com",
    "Port": 587
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

Options for `BlobStorage.Type`:

- `File` - Local file system
- `Azure` - Azure Blob Storage

### Mapbox

```json
{
  "Mapbox": {
    "AccessToken": "pk.eyJ1..."
  }
}
```

### Logging

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "Logs/webapi-.log",
          "rollingInterval": "Month"
        }
      }
    ]
  }
}
```

## Environment-Specific Configuration

### Development

`appsettings.Development.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

### Production

Use environment variables to override:

```bash
# Docker environment
ConnectionStrings__MasterDatabase="Host=postgres;..."
StripeConfig__SecretKey="sk_live_..."
```

Or `appsettings.Production.json`:

```json
{
  "IdentityServer": {
    "Authority": "https://id.yourdomain.com"
  }
}
```

## Docker Compose Environment

In `.env` file:

```bash
# Database
POSTGRES_PASSWORD=secure_password
MASTER_DB_CONNECTION=Host=postgres;Port=5432;Database=master_logistics;Username=postgres;Password=secure_password

# Identity
IDENTITY_AUTHORITY=https://id.yourdomain.com

# Stripe
STRIPE_PUBLISHABLE_KEY=pk_live_...
STRIPE_SECRET_KEY=sk_live_...
STRIPE_WEBHOOK_SECRET=whsec_...

# SMTP
SMTP_HOST=smtp.mailgun.org
SMTP_PORT=587
SMTP_USERNAME=postmaster@yourdomain.com
SMTP_PASSWORD=smtp_password

# External
MAPBOX_ACCESS_TOKEN=pk.eyJ1...
```

## Identity Server Configuration

Located in `src/Presentation/Logistics.IdentityServer/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MasterDatabase": "..."
  },
  "Clients": [
    {
      "ClientId": "logistics.office",
      "AllowedGrantTypes": ["authorization_code"],
      "RedirectUris": ["https://office.yourdomain.com/callback"],
      "PostLogoutRedirectUris": ["https://office.yourdomain.com"],
      "AllowedScopes": ["openid", "profile", "logistics.api"]
    }
  ]
}
```

## Angular Configuration

Located in `src/Client/Logistics.OfficeApp/src/environments/`:

### Development

`environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7000',
  identityUrl: 'https://localhost:7001',
  mapboxToken: 'pk.eyJ1...'
};
```

### Production

`environment.prod.ts`:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.yourdomain.com',
  identityUrl: 'https://id.yourdomain.com',
  mapboxToken: 'pk.eyJ1...'
};
```

## Aspire Configuration

Located in `src/Aspire/Logistics.Aspire.AppHost/appsettings.json`:

```json
{
  "Stripe": {
    "SecretKey": "sk_test_..."
  }
}
```

## Security Notes

1. **Never commit secrets** to version control
2. Use `.gitignore` for local settings files
3. Use environment variables in production
4. Rotate secrets regularly
5. Use different keys for dev/staging/production
