# Docker Compose Deployment

LogisticsX deploys as a set of containers defined by a hand-maintained Docker Compose file under [`deploy/`](https://github.com/suxrobgm/logistics-app/tree/main/deploy). The host runs plain `docker compose`; nginx (on the host) terminates TLS and reverse-proxies each subdomain to a loopback-bound container port.

## Layout

```text
deploy/
  docker-compose.yml            # main production stack
  docker-compose.dev.yml        # local dev infra (Postgres + migrator)
  docker-compose.portainer.yml  # standalone Portainer (separate project)
  .env.example                  # template for the DOCKER_ENV secret / .env
  nginx/logisticsx.conf         # host nginx reverse proxy (subdomains -> 127.0.0.1:port)
```

The main stack contains `identity-server`, `api`, `admin-portal`, `tms-portal`, `customer-portal`, and `website`. PostgreSQL is **external** (installed on the host or a managed instance) — it is not part of the compose file.

## Automated deployment (recommended)

Deployment is handled by the [`deploy.yml`](https://github.com/suxrobgm/logistics-app/blob/main/.github/workflows/deploy.yml) GitHub Actions workflow. Pushing to the `prod` branch (or running it manually) will:

1. Build and push all six images to GHCR.
2. Copy `deploy/docker-compose.yml` to `~/deploy/logistics/` on the VPS.
3. Write `.env` from the `DOCKER_ENV` secret and append `GITHUB_REPOSITORY` + `IMAGE_TAG`.
4. `docker compose pull && docker compose up -d --force-recreate --remove-orphans`.

Required GitHub secrets: `SSH_HOST`, `SSH_USER`, `SSH_KEY`, `GHCR_PAT`, `DOCKER_ENV` (the full `.env` contents), `FIREBASE_CREDENTIALS_JSON`.

## Manual deployment

Use this on first setup or when deploying outside CI. See [VPS Setup](vps-setup.md) first.

### 1. Copy the stack to the VPS

```bash
mkdir -p ~/deploy/logistics && cd ~/deploy/logistics
# Copy deploy/docker-compose.yml here, then:
cp /path/to/repo/deploy/.env.example .env
nano .env   # fill in production values (see Environment Variables)
```

`GITHUB_REPOSITORY` and `IMAGE_TAG` are appended automatically by CI; for a manual run, either export them or rely on the compose defaults (`suxrobgm/logistics-app` + `latest`).

### 2. Log in to GHCR and start

```bash
echo "$GHCR_PAT" | docker login ghcr.io -u <github-user> --password-stdin
docker compose pull
docker compose up -d
```

### 3. Configure nginx + SSL

See [VPS Setup](vps-setup.md) for the nginx copy + certbot steps.

### 4. Verify

```bash
curl -sf http://127.0.0.1:7000/health
curl http://127.0.0.1:7001/.well-known/openid-configuration
docker compose ps
```

## Subdomains

| Subdomain                  | Container port (loopback) |
| -------------------------- | ------------------------- |
| `api.logisticsx.app`       | 7000                      |
| `id.logisticsx.app`        | 7001                      |
| `admin.logisticsx.app`     | 7002                      |
| `tms.logisticsx.app`       | 7003                      |
| `customer.logisticsx.app`  | 7004                      |
| `logisticsx.app` (website) | 7005                      |
| `portainer.logisticsx.app` | 9000 (separate stack)     |

All app ports bind to `127.0.0.1`, so the containers are reachable only through nginx.

## Database migrations

Migrations are not run automatically in production. Apply them with the `Logistics.DbMigrator` project (or its image) pointed at the production connection strings before/after a release.

## Service management

```bash
docker compose logs -f            # all services
docker compose logs -f api        # one service
docker compose restart
docker compose pull && docker compose up -d   # update images
```

## Portainer

Portainer runs as its own compose project so a main-stack redeploy (`--remove-orphans`) never removes it. It is deployed once, manually — see [VPS Setup](vps-setup.md). Access is via `https://portainer.logisticsx.app` (nginx → `127.0.0.1:9000`); the port is never exposed publicly.

## Troubleshooting

```bash
docker compose ps
docker compose logs api --tail 100
docker compose logs identity-server
sudo nginx -t && sudo tail -f /var/log/nginx/error.log
```

## Next Steps

- [VPS Setup](vps-setup.md) — initial server configuration
- [Environment Variables](../configuration/environment-variables.md) — full configuration reference
