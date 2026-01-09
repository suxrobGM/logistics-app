# Deploying with .NET Aspire

Deploy Logistics TMS to your VPS with a single command. Aspire handles everything including automatic SSL certificates.

## Overview

Aspire includes:

- **nginx-proxy**: Automatic reverse proxy for all services
- **acme-companion**: Automatic Let's Encrypt SSL certificates
- **All services**: API, Identity Server, Admin App, Office App, PostgreSQL

## Prerequisites

- Ubuntu VPS with Docker installed ([VPS Setup](vps-setup.md))
- .NET 10 SDK installed on VPS
- Domain with DNS configured:

  ```text
  api.yourdomain.com    → VPS_IP
  id.yourdomain.com     → VPS_IP
  admin.yourdomain.com  → VPS_IP
  office.yourdomain.com → VPS_IP
  ```

## Deployment Steps

### Step 1: Clone Repository

```bash
ssh your-user@your-vps
git clone https://github.com/suxrobgm/logistics-app.git
cd logistics-app
```

### Step 2: Configure Production Settings

Create `src/Aspire/Logistics.Aspire.AppHost/appsettings.Production.json`:

```json
{
  "Stripe": {
    "SecretKey": "sk_live_your_stripe_key"
  },
  "Domain": "yourdomain.com",
  "EnableNginx": true,
  "LetsEncryptEmail": "admin@yourdomain.com"
}
```

### Step 3: Update API Configuration

Create `src/Presentation/Logistics.API/appsettings.Production.json`:

```json
{
  "IdentityServer": {
    "Authority": "https://id.yourdomain.com"
  },
  "StripeConfig": {
    "PublishableKey": "pk_live_...",
    "SecretKey": "sk_live_...",
    "WebhookSecret": "whsec_..."
  }
}
```

### Step 4: Deploy

```bash
cd src/Aspire/Logistics.Aspire.AppHost
dotnet run --environment Production
```

Aspire automatically:

1. Starts PostgreSQL with persistent storage
2. Runs database migrations
3. Starts all application services
4. Starts nginx-proxy on ports 80/443
5. Obtains SSL certificates from Let's Encrypt

### Step 5: Verify

```bash
curl https://api.yourdomain.com/health
curl https://id.yourdomain.com/.well-known/openid-configuration
```

## Running as a Service

Create `/etc/systemd/system/logistics.service`:

```ini
[Unit]
Description=Logistics TMS
After=docker.service
Requires=docker.service

[Service]
Type=simple
User=logistics
WorkingDirectory=/home/logistics/logistics-app/src/Aspire/Logistics.Aspire.AppHost
ExecStart=/usr/bin/dotnet run --environment Production
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

Enable and start:

```bash
sudo systemctl enable logistics
sudo systemctl start logistics
```

View logs:

```bash
sudo journalctl -u logistics -f
```

## Configuration Reference

| Setting | Description | Example |
|---------|-------------|---------|
| `Domain` | Your domain (without subdomain) | `yourdomain.com` |
| `EnableNginx` | Enable nginx-proxy + SSL | `true` |
| `LetsEncryptEmail` | Email for SSL certificates | `admin@yourdomain.com` |
| `Stripe:SecretKey` | Stripe API secret key | `sk_live_...` |

## How It Works

When `EnableNginx=true`:

```
Internet → nginx-proxy (80/443)
              ├── api.domain.com    → API (:7000)
              ├── id.domain.com     → Identity Server (:7001)
              ├── admin.domain.com  → Admin App (:7002)
              └── office.domain.com → Office App (:7003)

acme-companion → Automatic SSL certificates
```

1. nginx-proxy listens on ports 80/443
2. Routes traffic based on hostname using `VIRTUAL_HOST` env vars
3. acme-companion obtains/renews SSL certificates automatically

## Updating

```bash
cd ~/logistics-app
git pull
sudo systemctl restart logistics
```

## Troubleshooting

### SSL Certificate Issues

```bash
docker logs acme-companion
dig api.yourdomain.com  # Verify DNS
```

### Service Not Accessible

```bash
docker logs nginx-proxy
docker ps  # Verify services are running
```

### Database Issues

```bash
docker logs logistics-postgres
```

## Next Steps

- [VPS Setup](vps-setup.md) - Initial server configuration
- [CI/CD](ci-cd.md) - Automated deployments
