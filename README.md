# Logistics TMS

[![Build Status](https://github.com/suxrobgm/logistics-app/actions/workflows/build.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/build.yml)
[![Deploy](https://github.com/suxrobGM/logistics-app/actions/workflows/deploy.yml/badge.svg)](https://github.com/suxrobGM/logistics-app/actions/workflows/deploy.yml)
[![CC BY-NC 4.0][cc-by-nc-shield]][cc-by-nc]

[cc-by-nc]: https://creativecommons.org/licenses/by-nc/4.0/
[cc-by-nc-shield]: https://img.shields.io/badge/License-CC%20BY--NC%204.0-lightgrey.svg

> Multi-tenant fleet management platform for trucking companies. Automates dispatching, GPS tracking, invoicing, and payroll with a modern cloud-native architecture.

## Overview

Logistics TMS is purpose-built for trucking fleets specializing in intermodal containers and vehicle transport. It replaces spreadsheet-based workflows with an end-to-end digital system.

**Key Features:**

- **Multi-Tenant**: Each company gets isolated database and data
- **Real-Time Tracking**: GPS tracking of drivers and vehicles via SignalR
- **Load Management**: Create, assign, and track shipments from origin to destination
- **Invoicing & Payments**: Generate invoices and process payments with Stripe
- **Driver Mobile App**: Native Android/iOS app for drivers to manage their tasks
- **Analytics Dashboard**: Insights into operations, driver performance, and financials

## Quick Start

### Option 1: Docker with Aspire (Recommended)

```bash
dotnet run --project src/Aspire/Logistics.Aspire.AppHost
```

Dashboard: <http://localhost:7100>

### Option 2: Manual Setup

See [Local Development Guide](docs/getting-started/local-development.md)

## Live Demo

Try the application: [https://office.suxrobgm.net](https://office.suxrobgm.net)

**Test Credentials:**

| Role | Email | Password |
|------|-------|----------|
| Owner | <Test1@gmail.com> | Test12345# |
| Dispatcher | <Test3@gmail.com> | Test12345# |

[All test credentials](docs/getting-started/test-credentials.md)

## Documentation

| Guide | Description |
|-------|-------------|
| [Getting Started](docs/getting-started/prerequisites.md) | Prerequisites, local setup, Docker |
| [Architecture](docs/architecture/overview.md) | System design, patterns, domain model |
| [API Reference](docs/api/overview.md) | Authentication, endpoints, webhooks |
| [Deployment](docs/deployment/overview.md) | VPS setup, Docker Compose, Nginx, SSL |
| [Development](docs/development/backend-guide.md) | Backend, Angular, mobile guides |

## Tech Stack

**.NET 10** | **Angular 21** | **Kotlin Multiplatform** | **PostgreSQL** | **SignalR** | **Stripe** | **Docker** | **Aspire**

[Full tech stack and architecture](docs/architecture/overview.md)

## Applications

| App | Technology | Purpose |
|-----|------------|---------|
| Admin App | Blazor | Super admin management |
| Office App | Angular | Dispatcher/manager interface |
| Driver App | Kotlin Multiplatform | Mobile app for drivers |
| API | ASP.NET Core | RESTful backend |
| Identity Server | Duende IdentityServer | OAuth2/OIDC auth |

## Contributing

Pull requests welcome! Fork, create a feature branch, and submit a PR.

See [development guides](docs/development/backend-guide.md) for coding conventions.

## License

[Creative Commons Attribution-NonCommercial 4.0][cc-by-nc]

For commercial licensing, contact me.

## Contact

Created by **Sukhrob Ilyosbekov**

- Email: [suxrobgm@gmail.com](mailto:suxrobgm@gmail.com)
- Telegram: [@suxrobgm](https://t.me/suxrobgm)

## Preview

![Office App](docs/images/office-app/office_app_1.jpg)

[View all screenshots](docs/screenshots.md)
