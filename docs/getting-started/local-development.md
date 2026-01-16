# Local Development Setup

Step-by-step guide to run Logistics TMS without Docker.

## Step 1: Clone Repository

```bash
git clone https://github.com/suxrobgm/logistics-app.git
cd logistics-app
```

## Step 2: Install Angular Dependencies

```bash
cd src/Client/Logistics.OfficeApp
bun install
cd ../../..
```

## Step 3: Configure Database

### Create Databases

Connect to PostgreSQL and create the required databases:

```sql
CREATE DATABASE master_logistics;
CREATE DATABASE default_logistics;
```

Or via command line:

```bash
psql -U postgres -c "CREATE DATABASE master_logistics;"
psql -U postgres -c "CREATE DATABASE default_logistics;"
```

### Update Connection Strings

Edit `src/Presentation/Logistics.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MasterDatabase": "Host=localhost;Port=5432;Database=master_logistics;Username=postgres;Password=YOUR_PASSWORD",
    "DefaultTenantDatabase": "Host=localhost;Port=5432;Database=default_logistics;Username=postgres;Password=YOUR_PASSWORD"
  },
  "TenantsDatabaseConfig": {
    "DatabaseNameTemplate": "{tenant}_logistics",
    "DatabaseHost": "localhost",
    "DatabaseUserId": "postgres",
    "DatabasePassword": "YOUR_PASSWORD"
  }
}
```

Also update `src/Presentation/Logistics.IdentityServer/appsettings.json` with the same connection string.

## Step 4: Seed Databases

Run the database migrator to create tables and seed initial data:

```bash
# Using the provided script
scripts/seed-databases.cmd

# Or manually
dotnet run --project src/Presentation/Logistics.DbMigrator
```

## Step 5: Run Applications

Open separate terminals for each service:

**Terminal 1 - API**:

```bash
dotnet run --project src/Presentation/Logistics.API
# Runs on https://localhost:7000
```

**Terminal 2 - Identity Server**:

```bash
dotnet run --project src/Presentation/Logistics.IdentityServer
# Runs on https://localhost:7001
```

**Terminal 3 - Admin App**:

```bash
dotnet run --project src/Presentation/Logistics.AdminApp
# Runs on https://localhost:7002
```

**Terminal 4 - Office App**:

```bash
cd src/Client/Logistics.Angular
bun run start:tms
# Runs on https://localhost:7003
```

**Terminal 5 - Customer Portal (Optional)**:

```bash
cd src/Client/Logistics.Angular
bun run start:customer
# Runs on https://localhost:7004
```

Or use the provided scripts:

```bash
scripts/run-api.cmd
scripts/run-identity.cmd
scripts/run-adminapp.cmd
scripts/run-tms.cmd
scripts/run-customer.cmd
```

## Step 6: Configure Stripe (Optional)

For payment testing:

1. Get your test keys from [Stripe Dashboard](https://dashboard.stripe.com/test/apikeys)

2. Update `src/Presentation/Logistics.API/appsettings.json`:

   ```json
   {
     "Stripe": {
       "PublishableKey": "pk_test_...",
       "SecretKey": "sk_test_...",
       "WebhookSecret": ""
     }
   }
   ```

3. Run Stripe CLI for webhook forwarding:

   ```bash
   scripts/listen-stripe-webhook.cmd
   ```

4. Copy the webhook secret from Stripe CLI output and update `WebhookSecret`

## Step 7: Access Applications

| Application | URL | Credentials |
|-------------|-----|-------------|
| API (Swagger) | <https://localhost:7000/swagger> | - |
| Identity Server | <https://localhost:7001> | - |
| Admin App | <https://localhost:7002> | <admin@test.com> / Test12345# |
| Office App | <https://localhost:7003> | <owner@test.com> / Test12345# |

## Test Credentials

| Role | Email | Password | App Access |
|------|-------|----------|------------|
| Super Admin | <admin@test.com> | Test12345# | Admin App |
| Owner | <owner@test.com> | Test12345# | Office App |
| Manager | <manager1@test.com> | Test12345# | Office App |
| Dispatcher | <dispatcher1@test.com> | Test12345# | Office App |
| Driver | <driver1@test.com> | Test12345# | Driver Mobile App |

## Common Issues

### HTTPS Certificate Errors

Trust the development certificate:

```bash
dotnet dev-certs https --trust
```

### Port Already in Use

Check what's using the port:

```bash
# Windows
netstat -ano | findstr :7000

# macOS/Linux
lsof -i :7000
```

### Database Connection Failed

1. Verify PostgreSQL is running
2. Check connection string credentials
3. Ensure database exists

### Angular Build Errors

```bash
cd src/Client/Logistics.OfficeApp
bun install --force
```

## Next Steps

- [Docker Development](docker-development.md) - Simpler setup with Aspire
- [Test Credentials](test-credentials.md) - Full credentials reference
