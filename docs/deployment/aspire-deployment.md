# Docker Compose Deployment

Deploy Logistics TMS using the pre-generated Docker Compose configuration from .NET Aspire.

## Overview

The `aspire-output` directory contains production-ready configurations:

- `docker-compose.yaml` - Service definitions
- `.env.example` - Environment template
- `logistics.conf` - Nginx configuration

## Prerequisites

- VPS with Docker installed ([VPS Setup](vps-setup.md))
- Domain with DNS configured:
  ```text
  api.yourdomain.com    -> VPS_IP
  id.yourdomain.com     -> VPS_IP
  admin.yourdomain.com  -> VPS_IP
  office.yourdomain.com -> VPS_IP
  ```

## Deployment Steps

### Step 1: Clone Repository

```bash
git clone https://github.com/suxrobgm/logistics-app.git
cd logistics-app/src/Aspire/Logistics.Aspire.AppHost/aspire-output
```

### Step 2: Configure Environment

```bash
cp .env.example .env
nano .env
```

Edit the `.env` file with your production values. See [Environment Variables](../configuration/environment-variables.md) for details.

Key settings to configure:

| Variable | Description |
|----------|-------------|
| `POSTGRES_PASSWORD` | Database password |
| `Stripe__SecretKey` | Stripe API secret key |
| `Stripe__PublishableKey` | Stripe publishable key |
| `Stripe__WebhookSecret` | Stripe webhook secret |
| `SuperAdmin__Email` | Super admin email |
| `SuperAdmin__Password` | Super admin password |

### Step 3: Deploy Services

```bash
docker compose up -d
```

### Step 4: Configure Nginx

```bash
sudo cp logistics.conf /etc/nginx/sites-available/logistics
sudo ln -s /etc/nginx/sites-available/logistics /etc/nginx/sites-enabled/
sudo rm /etc/nginx/sites-enabled/default
sudo nginx -t && sudo systemctl reload nginx
```

### Step 5: Obtain SSL Certificates

```bash
sudo certbot --nginx -d api.yourdomain.com -d id.yourdomain.com -d admin.yourdomain.com -d office.yourdomain.com
```

### Step 6: Verify Deployment

```bash
curl https://api.yourdomain.com/health
curl https://id.yourdomain.com/.well-known/openid-configuration
```

## Service Management

```bash
# View logs
docker compose logs -f

# View specific service
docker compose logs -f api

# Restart services
docker compose restart

# Stop services
docker compose down

# Update images and restart
docker compose pull && docker compose up -d
```

## Services

| Service | Port | Description |
|---------|------|-------------|
| postgres | 5432 | PostgreSQL database |
| migrator | - | Database migrations (runs once) |
| identity-server | 7001 | OAuth2/OIDC authentication |
| api | 7000 | REST API |
| admin-app | 7002 | Super admin Blazor app |
| office-app | 7003 | Dispatcher Angular app |

## Volumes

| Volume | Purpose |
|--------|---------|
| `logistics-pg-data` | PostgreSQL data |
| `identity-keys` | Identity Server signing keys |

## Updating

```bash
cd logistics-app
git pull
cd src/Aspire/Logistics.Aspire.AppHost/aspire-output
docker compose pull
docker compose up -d
```

## Troubleshooting

### Check service status

```bash
docker compose ps
docker compose logs api --tail 100
```

### Database connection issues

```bash
docker compose logs postgres
docker exec -it postgres psql -U postgres -c "\l"
```

### Identity Server issues

```bash
docker compose logs identity-server
curl http://localhost:7001/.well-known/openid-configuration
```

### Nginx issues

```bash
sudo nginx -t
sudo tail -f /var/log/nginx/error.log
```

## Next Steps

- [VPS Setup](vps-setup.md) - Initial server configuration
- [Environment Variables](../configuration/environment-variables.md) - Full configuration reference
