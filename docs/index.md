# LogisticsX Documentation

Welcome to the LogisticsX docs.

LogisticsX is a fleet management platform for trucking companies. The same system runs whether you're dry van, reefer, flatbed, tanker, box truck, dump truck, tow truck, car hauler, container/intermodal, low loader, tautliner, swap body, or curtainsider. It works in the US and Europe, with address validation, currency (USD or EUR), and map defaults that match the region a tenant is provisioned in.

## Quick Links

| Section                                                 | Description                            |
| ------------------------------------------------------- | -------------------------------------- |
| [Features](features.md)                                 | Complete feature list                  |
| [Screenshots](screenshots.md)                           | Visual tour of all applications        |
| [Getting Started](getting-started/prerequisites.md)     | Prerequisites, local setup, Docker     |
| [Architecture](architecture/overview.md)                | System design, domain model, patterns  |
| [API Reference](api/overview.md)                        | Authentication, endpoints, webhooks    |
| [Deployment](deployment/overview.md)                    | VPS setup, one-command deploy with SSL |
| [Development](development/backend-guide.md)             | Backend, Angular, mobile guides        |
| [Configuration](configuration/environment-variables.md) | Environment variables, services        |

## Platform Overview

| Component           | Technology            | Purpose                      |
| ------------------- | --------------------- | ---------------------------- |
| **API**             | .NET 10, ASP.NET Core | RESTful backend              |
| **Identity Server** | Duende IdentityServer | OAuth2/OIDC auth             |
| **Admin App**       | Angular 21            | Super admin management       |
| **TMS Portal**      | Angular 21            | Dispatcher/manager interface |
| **Customer Portal** | Angular 21            | Customer self-service        |
| **Driver App**      | Kotlin Multiplatform  | Mobile app for drivers       |
| **Database**        | PostgreSQL            | Multi-tenant storage         |

## Features

- **AI Dispatch Agent** - Picks a truck for a load, runs HOS feasibility checks, supports multiple LLMs, and shows the reasoning behind every decision.
- **Load Management & Dispatching** - Create, assign, and track shipments.
- **Intermodal Containers** - ISO 6346 tracking with a lifecycle state machine (Empty → Loaded → At Port → In Transit → Delivered → Returned).
- **Terminals & Depots** - UN/LOCODE directory for sea ports, rail terminals, inland depots, air cargo facilities, and border crossings.
- **Trip Planning & Route Optimization** - Multi-stop routing through Mapbox.
- **GPS Tracking** - Live fleet map over SignalR WebSockets.
- **Fleet & Maintenance Tracking** - Trucks, trailers, schedules, VIN decoding.
- **Load Board Integration** - Search DAT, Truckstop, and 123Loadboard. Book loads. Post trucks.
- **Invoicing & Payments** - Stripe and Stripe Connect, with payment links and partial payments.
- **Payroll & Timesheets** - Driver pay calculation and payroll invoices.
- **Expense Tracking** - Categorized fleet expenses with reports.
- **ELD / HOS Compliance** - Samsara and Motive.
- **Safety & DVIR** - Digital inspections, incident tracking, compliance records.
- **Reports & Analytics** - 7 report types: driver, truck, revenue, customer, payroll, expense, operations.
- **Messaging** - In-app chat with read receipts and typing indicators.
- **Customer Portal** - Shipment tracking, invoices, documents, online payments.
- **Driver Mobile App** - Kotlin Multiplatform, Android and iOS.
- **Multi-Region** - US and European operations, with address validation, currency (USD or EUR), and map defaults that match the region.
- **Multi-Tenant** - One PostgreSQL database per company. Demo seeders for both US and EU regions.

[Complete feature list](features.md)

## Service Ports

| Service          | Development              | Production              |
| ---------------- | ------------------------ | ----------------------- |
| API              | <https://localhost:7000> | api.yourdomain.com      |
| Identity Server  | <https://localhost:7001> | id.yourdomain.com       |
| Admin App        | <https://localhost:7002> | admin.yourdomain.com    |
| TMS Portal       | <https://localhost:7003> | tms.yourdomain.com      |
| Customer Portal  | <https://localhost:7004> | customer.yourdomain.com |
| Website          | <http://localhost:7005>  | yourdomain.com          |
| Aspire Dashboard | <http://localhost:7100>  | (dev only)              |
