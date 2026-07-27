# Test Credentials

Accounts created by the database migrator (`src/Presentation/Logistics.DbMigrator`). Every password is `Test12345#`.

## Live Demo

A live demo is available at [https://tms.logisticsx.app](https://tms.logisticsx.app)

> **Warning**: The demo uses test Stripe keys. Do not enter real payment information.

## Super Admin

Runs the Admin Portal and is not attached to any tenant. Its credentials come from the `SuperAdmin` section of the migrator's `appsettings.json` (override with `SuperAdmin__Email` / `SuperAdmin__Password`), so a deployment can seed a different one.

| Email            | Password   | Application  |
| ---------------- | ---------- | ------------ |
| `admin@test.com` | Test12345# | Admin Portal |

## Demo Tenants

Three tenants are seeded. The tenant slug is what the client sends in the `X-Tenant` header, and it is also the `{tenant}` in the `{tenant}_logisticsx` database name.

| Slug   | Company                 | Region | Database          | Operating mode | Shape                                          |
| ------ | ----------------------- | ------ | ----------------- | -------------- | ---------------------------------------------- |
| `us`   | Heartland Logistics LLC | US     | `us_logisticsx`   | Fleet          | 10 employees, 5 trucks, 5 customers, 100 loads |
| `eu`   | EuroFreight GmbH        | EU     | `eu_logisticsx`   | Fleet          | 10 employees, 5 trucks, 5 customers, 100 loads |
| `solo` | Rodriguez Trucking LLC  | US     | `solo_logisticsx` | Owner-operator | 1 employee, 1 truck, 2 customers, 12 loads     |

The EU tenant bills in EUR, uses metric units and the Europe/Berlin timezone. The `solo` tenant runs in owner-operator mode, so the TMS portal hides employees, payroll, timesheets and messaging for it.

### US tenant (`us`)

| Role       | Email                  | Application       |
| ---------- | ---------------------- | ----------------- |
| Owner      | `owner@test.com`       | TMS Portal        |
| Manager    | `manager1@test.com`    | TMS Portal        |
| Dispatcher | `dispatcher1@test.com` | TMS Portal        |
| Dispatcher | `dispatcher2@test.com` | TMS Portal        |
| Dispatcher | `dispatcher3@test.com` | TMS Portal        |
| Driver     | `driver1@test.com`     | Driver Mobile App |
| Driver     | `driver2@test.com`     | Driver Mobile App |
| Driver     | `driver3@test.com`     | Driver Mobile App |
| Driver     | `driver4@test.com`     | Driver Mobile App |
| Driver     | `driver5@test.com`     | Driver Mobile App |
| Customer   | `customer1@test.com`   | Customer Portal   |
| Customer   | `customer2@test.com`   | Customer Portal   |

### EU tenant (`eu`)

Same shape, `eu_` prefixed. Note the manager has no number - it is `eu_manager@test.com`, not `eu_manager1@test.com`.

| Role       | Email                     | Application       |
| ---------- | ------------------------- | ----------------- |
| Owner      | `eu_owner@test.com`       | TMS Portal        |
| Manager    | `eu_manager@test.com`     | TMS Portal        |
| Dispatcher | `eu_dispatcher1@test.com` | TMS Portal        |
| Dispatcher | `eu_dispatcher2@test.com` | TMS Portal        |
| Dispatcher | `eu_dispatcher3@test.com` | TMS Portal        |
| Driver     | `eu_driver1@test.com`     | Driver Mobile App |
| Driver     | `eu_driver2@test.com`     | Driver Mobile App |
| Driver     | `eu_driver3@test.com`     | Driver Mobile App |
| Driver     | `eu_driver4@test.com`     | Driver Mobile App |
| Driver     | `eu_driver5@test.com`     | Driver Mobile App |
| Customer   | `eu_customer1@test.com`   | Customer Portal   |
| Customer   | `eu_customer2@test.com`   | Customer Portal   |

### Solo tenant (`solo`)

| Role     | Email                     | Application                    |
| -------- | ------------------------- | ------------------------------ |
| Owner    | `solo@test.com`           | TMS Portal + Driver Mobile App |
| Customer | `solo_customer1@test.com` | Customer Portal                |

`solo@test.com` is the tenant's only employee. They hold the Owner role and are the main driver of the tenant's single truck, so the same login works in both the TMS portal and the driver app. Owner carries `Permission.Driver.*`, which is what makes the driver app usable here.

> The Owner-to-driver-app path depends on `TenantRoleClaim` rows in each tenant database, not on the C# permission list. If a driver-app call 403s after a permission change, re-run the migrator so `TenantRoleSeeder` re-syncs them.

## Role Permissions

### Super Admin (Admin Portal only)

- Manage all tenants (companies)
- View system-wide statistics
- Manage subscriptions and billing
- Create and manage app admins

### Owner (TMS Portal, and the driver app when assigned to a truck)

- Full access to company data and company settings
- Manage employees, roles, and invitations
- Financials: invoices, payments, payroll, expenses, tax, accounting sync
- Everything Manager and Dispatcher can do

### Manager (TMS Portal)

- Loads, trips, trucks, customers, containers, terminals
- Invoices, payments, payroll, expenses
- ELD/HOS, DVIR review, safety, maintenance
- Manage employees and send invitations, but not company settings or roles

### Dispatcher (TMS Portal)

- Create and manage loads, assign drivers, manage trips
- Customers, containers, terminals, load board search/book/post
- View invoices, payments, trucks, ELD/HOS
- AI dispatch sessions and decisions

### Driver (Driver Mobile App)

- View assigned loads and trips, update load status
- Upload documents (POD, BOL) and share GPS location
- File and submit DVIRs (review/sign-off stays with Owner and Manager)
- Message dispatch

### Customer (Customer Portal)

- Track their own shipments, download documents, pay invoices

## Application Access Matrix

| Area                              | Super Admin | Owner | Manager | Dispatcher | Driver |
| --------------------------------- | :---------: | :---: | :-----: | :--------: | :----: |
| Tenant management (all companies) |      X      |       |         |            |        |
| Company settings                  |             |   X   |         |            |        |
| Employees & roles                 |             |   X   | Manage  |            |        |
| Loads & trips                     |             |   X   |    X    |     X      |  View  |
| Customers                         |             |   X   |    X    |     X      |        |
| Invoices & payments               |             |   X   |    X    |    View    |        |
| Payroll                           |             |   X   |    X    |            |        |
| Expenses                          |             |   X   |    X    |     X      |        |
| Load board                        |             |   X   |    X    |     X      |        |
| ELD / HOS                         |             |   X   |    X    |    View    |        |
| DVIR                              |             |   X   |    X    |            | Submit |
| Safety & maintenance              |             |   X   |    X    |            |        |
| AI dispatch                       |             |   X   |    X    |     X      |        |
| Analytics dashboard               |      X      |   X   |  View   |    View    |  View  |
| Messaging                         |             |   X   |    X    |     X      |   X    |

Source of truth: `src/Shared/Logistics.Shared.Identity/Policies/TenantRolePermissions.cs`.

## Creating Additional Test Users

Via the Admin Portal (super admin) or the TMS Portal (owner or manager):

1. Navigate to the Employees section
2. Invite a user by email and assign a role
3. The invitee receives an email with a link to set their password

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
psql -U postgres -c "DROP DATABASE IF EXISTS master_logisticsx;"
psql -U postgres -c "DROP DATABASE IF EXISTS us_logisticsx;"
psql -U postgres -c "DROP DATABASE IF EXISTS eu_logisticsx;"
psql -U postgres -c "DROP DATABASE IF EXISTS solo_logisticsx;"
psql -U postgres -c "CREATE DATABASE master_logisticsx;"

# Re-run migrations and seeding (the migrator creates the tenant databases itself)
dotnet run --project src/Presentation/Logistics.DbMigrator
```

Or with Docker:

```bash
docker compose -f deploy/docker-compose.dev.yml down -v
docker compose -f deploy/docker-compose.dev.yml up -d
```
