# Test Credentials

Default accounts available after seeding the database.

## Live Demo

A live demo is available at [https://tms.suxrobgm.net](https://tms.suxrobgm.net)

> **Warning**: The demo uses test Stripe keys. Do not enter real payment information.

## Account Credentials

| Role | Email | Password | Application |
|------|-------|----------|-------------|
| **Super Admin** | admin@test.com | Test12345# | Admin App |
| **Owner** | owner@test.com | Test12345# | Office App |
| **Manager** | manager1@test.com | Test12345# | Office App |
| **Dispatcher** | dispatcher1@test.com | Test12345# | Office App |
| **Driver** | driver1@test.com | Test12345# | Driver Mobile App |

## Role Permissions

### Super Admin (Admin App Only)

- Manage all tenants (companies)
- View system-wide statistics
- Manage subscriptions and billing
- Create and manage super admin users

### Owner (Office App)

- Full access to company data
- Manage employees and drivers
- View financial reports
- Manage company settings
- All Manager and Dispatcher permissions

### Manager (Office App)

- View analytics dashboard
- Manage loads and trips
- Manage customers
- View invoices and payments
- All Dispatcher permissions

### Dispatcher (Office App)

- Create and manage loads
- Assign drivers to loads
- Track driver locations
- Manage trips
- View load documents

### Driver (Mobile App Only)

- View assigned loads
- Update load status
- View trip details
- Upload documents (POD, BOL)
- Share GPS location

## Application Access Matrix

| Feature | Super Admin | Owner | Manager | Dispatcher | Driver |
|---------|:-----------:|:-----:|:-------:|:----------:|:------:|
| Tenant Management | X | | | | |
| User Management | X | X | | | |
| Load Management | | X | X | X | View |
| Customer Management | | X | X | | |
| Invoice Management | | X | X | View | |
| Driver Management | | X | X | | |
| GPS Tracking | | X | X | X | Send |
| Analytics Dashboard | X | X | X | | |
| Payroll | | X | View | | |
| Documents | | X | X | X | Upload |

## Test Tenant

The default tenant (company) for testing:

- **Tenant ID**: default
- **Company Name**: Default Logistics
- **Database**: default_logistics

## Creating Additional Test Users

Via Admin App (Super Admin) or Office App (Owner):

1. Navigate to Users section
2. Click "Add User"
3. Fill in details and assign role
4. User receives email with login instructions

## Password Requirements

- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character

## Resetting Test Data

To reset to initial test data:

```bash
# Drop and recreate databases
psql -U postgres -c "DROP DATABASE IF EXISTS master_logistics;"
psql -U postgres -c "DROP DATABASE IF EXISTS default_logistics;"
psql -U postgres -c "CREATE DATABASE master_logistics;"
psql -U postgres -c "CREATE DATABASE default_logistics;"

# Re-run migrations and seeding
dotnet run --project src/Presentation/Logistics.DbMigrator
```

Or with Docker:

```bash
docker compose down -v
dotnet run --project src/Aspire/Logistics.Aspire.AppHost
```
