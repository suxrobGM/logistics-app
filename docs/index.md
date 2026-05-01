# LogisticsX Documentation

Welcome to the LogisticsX documentation.

LogisticsX is a fleet management platform for trucking companies of any specialization — dry van, reefer, flatbed, tanker, box truck, dump truck, tow truck, car hauler, container/intermodal, low loader, tautliner, swap body, and curtainsider. It runs across **US** and **European** operations with region-aware address validation, multi-currency billing (USD / EUR), and locale-aware map defaults.

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

- **AI Dispatch Agent**: Autonomous load-to-truck matching with HOS-aware feasibility checks, multi-LLM support, and full reasoning transparency
- **Load Management & Dispatching**: Create, assign, and track shipments end-to-end
- **Intermodal Containers**: ISO 6346 container tracking with full lifecycle state machine (Empty → Loaded → At Port → In Transit → Delivered → Returned)
- **Terminals & Depots**: UN/LOCODE-keyed directory for sea ports, rail terminals, inland depots, air cargo facilities, and border crossings
- **Trip Planning & Route Optimization**: Multi-stop routing with Mapbox integration
- **Real-Time GPS Tracking**: Live fleet map via SignalR WebSockets
- **Fleet & Maintenance Tracking**: Trucks, trailers, schedules, and VIN decoding
- **Load Board Integration**: Search DAT, Truckstop, 123Loadboard; book loads and post trucks
- **Invoicing & Payments**: Stripe + Stripe Connect with payment links and partial payments
- **Payroll & Timesheets**: Driver pay calculation and payroll invoice generation
- **Expense Tracking**: Categorized fleet expenses with reporting
- **ELD / HOS Compliance**: Samsara and Motive integrations
- **Safety & DVIR**: Digital inspections, incident tracking, compliance records
- **Reports & Analytics**: 7 report types — driver, truck, revenue, customer, payroll, expense, operations
- **Real-Time Messaging**: In-app chat with read receipts and typing indicators
- **Customer Self-Service Portal**: Shipment tracking, invoices, documents, online payments
- **Driver Mobile App**: Kotlin Multiplatform for Android & iOS
- **Multi-Region Support**: First-class US and European operations with region-aware address validation, multi-currency billing (USD / EUR), and locale-aware map defaults
- **Multi-Tenant Architecture**: Isolated PostgreSQL databases per company, region-aware demo seeders (US + EU)

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
