# Production Docker Compose Configuration

This guide provides a production-ready Docker Compose configuration for manual deployment.

## Complete docker-compose.yml

Create `/opt/logistics/docker-compose.yml`:

```yaml
version: "3.9"

services:
  postgres:
    image: postgres:17-alpine
    container_name: logistics-postgres
    restart: unless-stopped
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: master_logistics
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./init-db.sql:/docker-entrypoint-initdb.d/init-db.sql:ro
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - logistics-network

  api:
    image: ghcr.io/suxrobgm/logistics-api:latest
    container_name: logistics-api
    restart: unless-stopped
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:7000
      - ConnectionStrings__MasterDatabase=${MASTER_DB_CONNECTION}
      - ConnectionStrings__DefaultTenantDatabase=${TENANT_DB_CONNECTION}
      - IdentityServer__Authority=${IDENTITY_AUTHORITY}
      - IdentityServer__Audience=logistics.api
      - StripeConfig__PublishableKey=${STRIPE_PUBLISHABLE_KEY}
      - StripeConfig__SecretKey=${STRIPE_SECRET_KEY}
      - StripeConfig__WebhookSecret=${STRIPE_WEBHOOK_SECRET}
      - SmtpConfig__Host=${SMTP_HOST}
      - SmtpConfig__Port=${SMTP_PORT}
      - SmtpConfig__UserName=${SMTP_USERNAME}
      - SmtpConfig__Password=${SMTP_PASSWORD}
      - Mapbox__AccessToken=${MAPBOX_ACCESS_TOKEN}
    volumes:
      - uploads_data:/app/wwwroot/uploads
      - api_logs:/app/Logs
    ports:
      - "127.0.0.1:7000:7000"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:7000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
    networks:
      - logistics-network

  identity-server:
    image: ghcr.io/suxrobgm/logistics-identity:latest
    container_name: logistics-identity
    restart: unless-stopped
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:7001
      - ConnectionStrings__MasterDatabase=${MASTER_DB_CONNECTION}
    volumes:
      - identity_logs:/app/Logs
    ports:
      - "127.0.0.1:7001:7001"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:7001/health"]
      interval: 30s
      timeout: 10s
      retries: 3
    networks:
      - logistics-network

  admin-app:
    image: ghcr.io/suxrobgm/logistics-admin:latest
    container_name: logistics-admin
    restart: unless-stopped
    depends_on:
      - api
      - identity-server
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:7002
    ports:
      - "127.0.0.1:7002:7002"
    networks:
      - logistics-network

  office-app:
    image: ghcr.io/suxrobgm/logistics-office:latest
    container_name: logistics-office
    restart: unless-stopped
    depends_on:
      - api
      - identity-server
    ports:
      - "127.0.0.1:7003:80"
    networks:
      - logistics-network

volumes:
  postgres_data:
    driver: local
  uploads_data:
    driver: local
  api_logs:
    driver: local
  identity_logs:
    driver: local

networks:
  logistics-network:
    driver: bridge
```

## Environment File

Create `/opt/logistics/.env`:

```bash
# Database
POSTGRES_PASSWORD=your-strong-password-here
MASTER_DB_CONNECTION=Host=postgres;Port=5432;Database=master_logistics;Username=postgres;Password=your-strong-password-here;Include Error Detail=false
TENANT_DB_CONNECTION=Host=postgres;Port=5432;Database=default_logistics;Username=postgres;Password=your-strong-password-here;Include Error Detail=false

# Identity Server
IDENTITY_AUTHORITY=https://id.yourdomain.com

# Stripe
STRIPE_PUBLISHABLE_KEY=pk_live_your_key
STRIPE_SECRET_KEY=sk_live_your_key
STRIPE_WEBHOOK_SECRET=whsec_your_secret

# SMTP
SMTP_HOST=smtp.yourdomain.com
SMTP_PORT=587
SMTP_USERNAME=noreply@yourdomain.com
SMTP_PASSWORD=your-smtp-password

# Mapbox
MAPBOX_ACCESS_TOKEN=pk.your_mapbox_token
```

## Database Initialization Script

Create `/opt/logistics/init-db.sql`:

```sql
-- Create tenant database
CREATE DATABASE default_logistics;

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE master_logistics TO postgres;
GRANT ALL PRIVILEGES ON DATABASE default_logistics TO postgres;
```

## Deployment Commands

```bash
# Start all services
docker compose up -d

# View logs
docker compose logs -f

# View specific service logs
docker compose logs -f api

# Restart a service
docker compose restart api

# Stop all services
docker compose down

# Stop and remove volumes (WARNING: deletes data)
docker compose down -v
```

## Building Images Locally

If you're not using pre-built images:

```bash
# Clone the repository
git clone https://github.com/suxrobgm/logistics-app.git
cd logistics-app

# Build all images
docker compose -f docker-compose.yml build

# Or build individual images
docker build -t logistics-api:latest -f src/Presentation/Logistics.API/Dockerfile .
docker build -t logistics-identity:latest -f src/Presentation/Logistics.IdentityServer/Dockerfile .
```

## Data Persistence

Data is stored in Docker volumes:

| Volume | Purpose | Backup Priority |
|--------|---------|-----------------|
| `postgres_data` | Database files | Critical |
| `uploads_data` | User uploads | High |
| `api_logs` | API logs | Low |
| `identity_logs` | Auth logs | Low |

## Health Checks

All services include health checks. Monitor with:

```bash
# Check all service health
docker compose ps

# Detailed health info
docker inspect logistics-api --format='{{.State.Health.Status}}'
```

## Resource Limits (Optional)

Add resource limits for production stability:

```yaml
services:
  api:
    # ... other config
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 512M
```

## Next Steps

- [Nginx Reverse Proxy](nginx-reverse-proxy.md) - Configure domain routing and SSL
- [Backup & Restore](../operations/backup-restore.md) - Set up automated backups
