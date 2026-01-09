# Logistics TMS Documentation

Welcome to the Logistics TMS documentation.

## Quick Links

| Section | Description |
|---------|-------------|
| [Getting Started](getting-started/prerequisites.md) | Prerequisites, local setup, Docker |
| [Architecture](architecture/overview.md) | System design, domain model, patterns |
| [API Reference](api/overview.md) | Authentication, endpoints, webhooks |
| [Deployment](deployment/overview.md) | VPS setup, one-command deploy with SSL |
| [Development](development/backend-guide.md) | Backend, Angular, mobile guides |
| [Configuration](configuration/environment-variables.md) | Environment variables, services |

## Platform Overview

| Component | Technology | Purpose |
|-----------|------------|---------|
| **API** | .NET 10, ASP.NET Core | RESTful backend |
| **Identity Server** | Duende IdentityServer | OAuth2/OIDC auth |
| **Admin App** | Blazor Server | Super admin management |
| **Office App** | Angular 21 | Dispatcher/manager interface |
| **Driver App** | Kotlin Multiplatform | Mobile app for drivers |
| **Database** | PostgreSQL | Multi-tenant storage |

## Features

- **Multi-Tenant Architecture**: Isolated databases per company
- **Role-Based Access Control**: Super admin, owner, manager, dispatcher, driver
- **Load Management**: Create, track, and manage shipments
- **GPS Tracking**: Real-time driver tracking via SignalR
- **Invoicing & Payments**: Stripe integration
- **Payroll Management**: Employee payroll tracking
- **Document Management**: Load-related documents
- **Analytics Dashboard**: Operations insights
- **Notifications**: Real-time push notifications via Firebase

## Service Ports

| Service | Development | Production |
|---------|-------------|------------|
| API | <https://localhost:7000> | api.yourdomain.com |
| Identity Server | <https://localhost:7001> | id.yourdomain.com |
| Admin App | <https://localhost:7002> | admin.yourdomain.com |
| Office App | <https://localhost:7003> | office.yourdomain.com |
| Aspire Dashboard | <http://localhost:8100> | (dev only) |
