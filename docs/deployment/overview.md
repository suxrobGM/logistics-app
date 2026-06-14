# Deployment Overview

Deploy LogisticsX to a production VPS using a hand-maintained Docker Compose stack under `deploy/`, fronted by host nginx. Deployment is automated via GitHub Actions (push to `prod`); manual steps are documented for first setup.

## Architecture

```text
Internet -> Nginx (80/443 with SSL, on the host)
               |-- api.domain.com      -> API (:7000)
               |-- id.domain.com       -> Identity Server (:7001)
               |-- admin.domain.com    -> Admin Portal (:7002)
               |-- tms.domain.com      -> TMS Portal (:7003)
               |-- customer.domain.com -> Customer Portal (:7004)
               |-- domain.com          -> Website (:7005)
               +-- portainer.domain.com-> Portainer (:9000, separate stack)
                         |
                         v
                External PostgreSQL
```

All app container ports bind to `127.0.0.1`; nginx is the only public entry point.

## System Requirements

| Resource | Minimum          | Recommended      |
| -------- | ---------------- | ---------------- |
| CPU      | 2 vCPU           | 4 vCPU           |
| RAM      | 4 GB             | 8 GB             |
| Storage  | 40 GB SSD        | 80 GB SSD        |
| OS       | Ubuntu 22.04 LTS | Ubuntu 24.04 LTS |

## Deployment Steps

1. **[VPS Setup](vps-setup.md)** — install Docker + nginx, configure the server and Portainer
2. **[Docker Compose Deployment](docker-deployment.md)** — configure and run the stack (automated or manual)

## DNS Configuration

Create DNS A records pointing to your VPS IP:

```text
api.yourdomain.com       -> YOUR_VPS_IP
id.yourdomain.com        -> YOUR_VPS_IP
admin.yourdomain.com     -> YOUR_VPS_IP
tms.yourdomain.com       -> YOUR_VPS_IP
customer.yourdomain.com  -> YOUR_VPS_IP
portainer.yourdomain.com -> YOUR_VPS_IP
yourdomain.com           -> YOUR_VPS_IP
```

## Pre-deployment Checklist

- [ ] VPS with SSH access
- [ ] External PostgreSQL reachable from the VPS
- [ ] Domain with DNS configured
- [ ] Stripe API keys (for payments)
- [ ] Resend API key (for emails)
- [ ] Mapbox access token (for maps)

## Guides

| Guide                                                              | Description                  |
| ------------------------------------------------------------------ | ---------------------------- |
| [VPS Setup](vps-setup.md)                                          | Initial server configuration |
| [Docker Compose Deployment](docker-deployment.md)                  | Configure and run the stack  |
| [Environment Variables](../configuration/environment-variables.md) | Configuration reference      |
