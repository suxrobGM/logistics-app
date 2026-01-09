# Deployment Overview

Deploy Logistics TMS to a production VPS with automatic SSL.

## Quick Start

```bash
# On your VPS
git clone https://github.com/suxrobgm/logistics-app.git
cd logistics-app/src/Aspire/Logistics.Aspire.AppHost

# Configure production settings (see aspire-deployment.md)
dotnet run --environment Production
```

## Architecture

```text
Internet → nginx-proxy (80/443) with automatic SSL
              ├── api.domain.com    → API (:7000)
              ├── id.domain.com     → Identity Server (:7001)
              ├── admin.domain.com  → Admin App (:7002)
              └── office.domain.com → Office App (:7003)
                        │
                        ▼
                   PostgreSQL
```

## System Requirements

| Resource | Minimum | Recommended |
|----------|---------|-------------|
| CPU | 2 vCPU | 4 vCPU |
| RAM | 4 GB | 8 GB |
| Storage | 40 GB SSD | 80 GB SSD |
| OS | Ubuntu 22.04 LTS | Ubuntu 24.04 LTS |

## Deployment Steps

1. **[VPS Setup](vps-setup.md)** - Install Docker and .NET SDK
2. **[Aspire Deployment](aspire-deployment.md)** - Deploy with automatic SSL

## DNS Configuration

Create DNS A records pointing to your VPS IP:

```text
api.yourdomain.com      → YOUR_VPS_IP
id.yourdomain.com       → YOUR_VPS_IP
admin.yourdomain.com    → YOUR_VPS_IP
office.yourdomain.com   → YOUR_VPS_IP
```

## Checklist

Before deploying:

- [ ] VPS with SSH access
- [ ] Domain with DNS configured
- [ ] Stripe API keys (for payments)
- [ ] SMTP credentials (for emails, optional)
- [ ] Mapbox access token (for maps)

## Guides

| Guide | Description |
|-------|-------------|
| [VPS Setup](vps-setup.md) | Initial server configuration |
| [Aspire Deployment](aspire-deployment.md) | One-command deployment with SSL |
| [Docker Compose](docker-compose-prod.md) | Manual Docker Compose setup |
| [CI/CD](ci-cd.md) | GitHub Actions automation |
