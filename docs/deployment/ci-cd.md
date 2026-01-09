# CI/CD Pipeline

GitHub Actions workflows for building and deploying Logistics TMS.

## Workflows

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| `build.yml` | Push/PR to main | Build and test |
| `deploy-ssh.yml` | Release published | Deploy to VPS |

## Build Workflow

Located in `.github/workflows/build.yml`:

### Triggers

```yaml
on:
  push:
    branches: [main]
  pull_request:
    branches: [main]
```

### Jobs

1. **Build .NET projects**
2. **Run tests**
3. **Build Angular app**
4. **Build Docker images** (optional)

### Key Steps

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build --verbosity normal
```

## Deploy Workflow

Located in `.github/workflows/deploy-ssh.yml`:

### Triggers

```yaml
on:
  release:
    types: [published]
```

### Deployment Steps

1. Build and publish .NET apps
2. Build Angular app
3. SSH to VPS
4. Stop services
5. Copy new files
6. Run migrations
7. Start services

### Required Secrets

Configure in GitHub repository settings:

| Secret | Description |
|--------|-------------|
| `SSH_HOST` | VPS IP address |
| `SSH_USERNAME` | SSH user (e.g., `logistics`) |
| `SSH_PRIVATE_KEY` | SSH private key |
| `SSH_PORT` | SSH port (default: 22) |

### Example Deploy Script

```yaml
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Publish API
        run: dotnet publish src/Presentation/Logistics.API -c Release -o publish/api

      - name: Publish Identity Server
        run: dotnet publish src/Presentation/Logistics.IdentityServer -c Release -o publish/identity

      - name: Setup Bun
        uses: oven-sh/setup-bun@v1

      - name: Build Office App
        run: |
          cd src/Client/Logistics.OfficeApp
          bun install
          bun run build

      - name: Deploy via SSH
        uses: appleboy/ssh-action@v1
        with:
          host: ${{ secrets.SSH_HOST }}
          username: ${{ secrets.SSH_USERNAME }}
          key: ${{ secrets.SSH_PRIVATE_KEY }}
          script: |
            cd /opt/logistics
            sudo systemctl stop logistics-api
            sudo systemctl stop logistics-identity
            # Copy files, run migrations, restart services
```

## SSH Key Setup

### Generate Deploy Key

```bash
ssh-keygen -t ed25519 -C "github-deploy" -f deploy_key
```

### Add to VPS

```bash
# On VPS
cat deploy_key.pub >> ~/.ssh/authorized_keys
```

### Add to GitHub

1. Go to repository Settings → Secrets → Actions
2. Add `SSH_PRIVATE_KEY` with contents of `deploy_key`
3. Add other secrets (`SSH_HOST`, `SSH_USERNAME`, etc.)

## Docker-Based Deployment

Alternative workflow using Docker:

```yaml
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Login to Container Registry
        uses: docker/login-action@v3
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Build and push API
        uses: docker/build-push-action@v5
        with:
          context: .
          file: src/Presentation/Logistics.API/Dockerfile
          push: true
          tags: ghcr.io/${{ github.repository }}/api:latest

      - name: Deploy to VPS
        uses: appleboy/ssh-action@v1
        with:
          host: ${{ secrets.SSH_HOST }}
          username: ${{ secrets.SSH_USERNAME }}
          key: ${{ secrets.SSH_PRIVATE_KEY }}
          script: |
            cd /opt/logistics
            docker compose pull
            docker compose up -d
```

## Rollback

### Manual Rollback

```bash
# On VPS
cd /opt/logistics

# Stop services
docker compose down

# Restore previous version
git checkout v1.0.0  # or specific commit

# Rebuild and start
docker compose build
docker compose up -d
```

### Database Rollback

```bash
# Restore from backup
./restore-database.sh /var/backups/logistics/backup_20240115.tar.gz
```

## Monitoring Deployments

### GitHub Actions Status

Check workflow runs at:
```
https://github.com/suxrobgm/logistics-app/actions
```

### Post-Deploy Verification

Add health check step:

```yaml
- name: Verify deployment
  run: |
    sleep 30
    curl -f https://api.yourdomain.com/health || exit 1
```

## Branch Protection

Recommended settings for `main` branch:

1. Require pull request reviews
2. Require status checks to pass
3. Require branches to be up to date
4. Include administrators

## Next Steps

- [VPS Setup](vps-setup.md) - Server preparation
- [Docker Compose](docker-compose-prod.md) - Container configuration
