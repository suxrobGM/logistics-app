# LogisticsX

[![Build Status](https://github.com/suxrobgm/logistics-app/actions/workflows/build.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/build.yml)
[![Deploy](https://github.com/suxrobGM/logistics-app/actions/workflows/deploy.yml/badge.svg)](https://github.com/suxrobGM/logistics-app/actions/workflows/deploy.yml)
[![FSL-1.1-Apache-2.0][fsl-shield]][fsl]

[fsl]: https://fsl.software/
[fsl-shield]: https://img.shields.io/badge/License-FSL--1.1--Apache--2.0-blue.svg

> Fleet management platform for trucking companies. An AI driven dispatch agent matches loads to trucks, checks HOS compliance, plans routes, and pulls leads from load boards - every decision logged so a human can review or override it.

![AI Dispatch - Sessions & Decisions](docs/images/tms-portal/ai-dispatch-sessions.png)

<!-- markdownlint-disable MD033 -->
<p align="center">
  <a href="https://logisticsx.app"><img src="https://img.shields.io/badge/Website-logisticsx.app-7c3aed?style=for-the-badge&logo=google-chrome&logoColor=white" alt="Website"></a>
  <a href="https://tms.logisticsx.app"><img src="https://img.shields.io/badge/Live_Demo-Try_It_Now-10b981?style=for-the-badge&logo=rocket&logoColor=white" alt="Live Demo"></a>
</p>

## Try it

| Portal          | URL                                                        |
| --------------- | ---------------------------------------------------------- |
| TMS Portal      | [tms.logisticsx.app](https://tms.logisticsx.app)           |
| Customer Portal | [customer.logisticsx.app](https://customer.logisticsx.app) |

| Role       | Email                  | Password   |
| ---------- | ---------------------- | ---------- |
| Owner      | `owner@test.com`       | Test12345# |
| Manager    | `manager1@test.com`    | Test12345# |
| Dispatcher | `dispatcher1@test.com` | Test12345# |
| Driver     | `driver1@test.com`     | Test12345# |
| Customer   | `customer1@test.com`   | Test12345# |

[All test credentials](docs/getting-started/test-credentials.md)

## Who it's for

LogisticsX is built first-class for **freight, vehicle transport, and intermodal drayage** operations. Other fleet types (refrigerated, flatbed, tanker, etc.) are supported experimentally and may need workflow tweaks. The platform replaces the patchwork of spreadsheets, group chats, and standalone TMS/accounting tools - dispatch, driver mobile app, customer tracking, invoicing, and payroll all run from the same system, in real time. Works across both **US and EU** operations, with multi-tenant isolation so each company gets its own database.

Roles:

- **Dispatchers** create loads, assign drivers, search load boards, and watch deliveries - or hand it off to the agent.
- **Drivers** get assignments, navigate routes, capture proof of delivery, and message dispatch from the mobile app.
- **Customers** track shipments, download documents, and pay invoices through the customer portal.
- **Owners** see financials, driver metrics, payroll, and operational reports.

## AI dispatch agent

The agent looks at fleet state, picks a truck for a load, checks HOS compliance, plans a trip, and watches load boards for revenue. Two modes:

- **Human-in-the-loop** - the agent suggests assignments; a dispatcher approves them.
- **Autonomous** - the agent acts on its own.

Every decision is logged with the reasoning that produced it, so dispatchers can approve, reject, or re-plan instead of staring at a black box. See [AI Dispatch](docs/ai-dispatch.md) and [MCP Server](docs/mcp-server.md) for connecting Claude Desktop, Cursor, and other AI tools to your fleet.

## Features

| Operations                               | Financial                          | Compliance                      | Communication                |
| :--------------------------------------- | :--------------------------------- | :------------------------------ | :--------------------------- |
| AI-powered dispatching                   | Invoicing & Stripe payments        | ELD / HOS (Samsara, Motive)     | Real-time messaging          |
| MCP server for AI tools                  | Stripe Connect direct payouts      | Safety & DVIR inspections       | Push notifications           |
| Trip planning & route optimization       | Multi-currency billing (USD / EUR) | Document management (POD, BOL)  | Customer self-service portal |
| Intermodal container tracking (ISO 6346) | Payroll & timesheets               | Region-aware address validation | Driver mobile app            |
| Terminals & depots (UN/LOCODE)           | Expense tracking                   | Role-based access control       |                              |
| Fleet & maintenance tracking             | Reports & analytics                |                                 |                              |
| Load board integration (DAT, Truckstop)  |                                    |                                 |                              |

[Complete feature list](docs/features.md)

## Quick start

```bash
dotnet run --project src/Aspire/Logistics.Aspire.AppHost
```

Dashboard: http://localhost:7100. For manual setup, see the [Local Development Guide](docs/getting-started/local-development.md).

## Tech stack

| Layer              | Technologies                                                                |
| ------------------ | --------------------------------------------------------------------------- |
| **Backend**        | .NET 10, ASP.NET Core, EF Core, MediatR, SignalR, Duende IdentityServer     |
| **Frontend**       | Angular 21, PrimeNG, Tailwind CSS                                           |
| **Mobile**         | Kotlin Multiplatform, Compose Multiplatform                                 |
| **Database**       | PostgreSQL 18                                                               |
| **Payments**       | Stripe, Stripe Connect                                                      |
| **Infrastructure** | Docker, .NET Aspire, Nginx, GitHub Actions                                  |
| **Integrations**   | Mapbox, Firebase, Samsara, Motive, DAT, Truckstop, 123Loadboard, Claude API |

Architecture: DDD + CQRS with MediatR, multi-tenant with one database per company. See [architecture overview](docs/architecture/overview.md).

| App             | Port |
| --------------- | ---- |
| API             | 7000 |
| Identity Server | 7001 |
| Admin Portal    | 7002 |
| TMS Portal      | 7003 |
| Customer Portal | 7004 |
| Website         | 7005 |

## Screenshots

|                     Sessions & Decisions                     |                           Agent Timeline                           |
| :----------------------------------------------------------: | :----------------------------------------------------------------: |
| ![Sessions](docs/images/tms-portal/ai-dispatch-sessions.png) | ![Timeline](docs/images/tms-portal/ai-dispatch-agent-timeline.png) |

<details>
<summary>More screenshots</summary>

### TMS Portal

|                     Dashboard                     |                     Loads                      |                     Trips                      |
| :-----------------------------------------------: | :--------------------------------------------: | :--------------------------------------------: |
| ![Dashboard](docs/images/tms-portal/tms-home.png) | ![Loads](docs/images/tms-portal/tms-loads.png) | ![Trips](docs/images/tms-portal/tms-trips.png) |

|                     Fleet                      |                      Reports                       |                           Invoicing                           |
| :--------------------------------------------: | :------------------------------------------------: | :-----------------------------------------------------------: |
| ![Fleet](docs/images/tms-portal/tms-fleet.png) | ![Reports](docs/images/tms-portal/tms-reports.png) | ![Invoices](docs/images/tms-portal/tms-invoice-dashboard.png) |

### Customer Portal

|                            Dashboard                             |                        Shipment Details                        |
| :--------------------------------------------------------------: | :------------------------------------------------------------: |
| ![Dashboard](docs/images/customer-portal/customer-dashboard.png) | ![Shipment](docs/images/customer-portal/customer-shipment.png) |

</details>

[All screenshots](docs/screenshots.md)

## Documentation

| Guide                                                    | Description                                                   |
| -------------------------------------------------------- | ------------------------------------------------------------- |
| [Features](docs/features.md)                             | Complete feature list                                         |
| [Getting Started](docs/getting-started/prerequisites.md) | Prerequisites, local setup, Docker                            |
| [Architecture](docs/architecture/overview.md)            | System design, patterns, domain model                         |
| [API Reference](docs/api/overview.md)                    | Authentication, endpoints, webhooks                           |
| [Deployment](docs/deployment/overview.md)                | VPS setup, Docker Compose, Nginx, SSL                         |
| [AI Dispatch](docs/ai-dispatch.md)                       | Agentic dispatcher architecture & API                         |
| [MCP Server](docs/mcp-server.md)                         | Connect Claude Desktop, Cursor & other AI tools to your fleet |
| [Development](docs/development/backend-guide.md)         | Backend, Angular, mobile guides                               |

## Contributing

Pull requests welcome. Fork, branch off `main`, open a PR. See the [development guides](docs/development/backend-guide.md) for coding conventions.

## License

[Functional Source License, Version 1.1, Apache 2.0 Future License][fsl] (FSL-1.1-Apache-2.0).

Free for internal use, non-commercial use, and professional services. Commercial use that competes with LogisticsX (e.g. offering it as a hosted TMS) is not permitted during the 2-year change period. The license auto-converts to **Apache 2.0** on the second anniversary of each release. For earlier commercial licensing, contact the author.

## Contact

Created by **Sukhrob Ilyosbekov**

- Email: [suxrobgm@gmail.com](mailto:suxrobgm@gmail.com)
- LinkedIn: [linkedin.com/in/suxrobgm](https://www.linkedin.com/in/suxrobgm)
- Telegram: [@suxrobgm](https://t.me/suxrobgm)
