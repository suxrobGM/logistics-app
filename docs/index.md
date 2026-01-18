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
| **Admin App** | Angular 21 | Super admin management |
| **TMS Portal** | Angular 21 | Dispatcher/manager interface |
| **Customer Portal** | Angular 21 | Customer self-service |
| **Driver App** | Kotlin Multiplatform | Mobile app for drivers |
| **Database** | PostgreSQL | Multi-tenant storage |

## Features

- **Multi-Tenant Architecture**: Isolated databases per company
- **Role-Based Access Control**: Super admin, owner, manager, dispatcher, driver, customer
- **Load Management**: Create, track, and manage shipments
- **GPS Tracking**: Real-time driver tracking via SignalR
- **Real-Time Messaging**: In-app chat between dispatchers and drivers with read receipts and typing indicators
- **Proof of Delivery (POD)**: Capture photos, signatures, recipient info, and GPS location for delivery confirmation
- **Vehicle Condition Reports**: Pre-trip/post-trip inspections (DVIR) with visual damage marking and VIN decoding
- **VIN Decoding**: Automatic vehicle information lookup via NHTSA API
- **Invoicing & Payments**: Stripe integration
- **Payroll Management**: Employee payroll tracking
- **Document Management**: Load-related documents with Azure Blob Storage
- **Analytics Dashboard**: Operations insights
- **Notifications**: Real-time push notifications via Firebase

## Service Ports

| Service | Development | Production |
|---------|-------------|------------|
| API | <https://localhost:7000> | api.yourdomain.com |
| Identity Server | <https://localhost:7001> | id.yourdomain.com |
| Admin App | <https://localhost:7002> | admin.yourdomain.com |
| TMS Portal | <https://localhost:7003> | tms.yourdomain.com |
| Customer Portal | <https://localhost:7004> | customer.yourdomain.com |
| Aspire Dashboard | <http://localhost:8100> | (dev only) |
