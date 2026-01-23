# Logistics Angular Workspace

This is an Angular 21 workspace containing multiple applications and a shared library.

## Projects

| Project           | Type        | Port | Description                                                     |
| ----------------- | ----------- | ---- | --------------------------------------------------------------- |
| `shared`          | library     | N/A  | Shared API client, interceptors, guards, services               |
| `tms-portal`      | application | 7003 | Internal TMS for dispatchers/managers (migrated from OfficeApp) |
| `customer-portal` | application | 7004 | Customer self-service portal                                    |
| `website`         | application | 7005 | Marketing website (SSR)                                         |

## Commands

```bash
# Install dependencies
bun install

# Development servers
bun run start:tms          # TMS Portal on https://localhost:7003
bun run start:customer     # Customer Portal on https://localhost:7004
bun run start:website      # Marketing Website on https://localhost:7005

# Build
bun run build:shared       # Build shared library first
bun run build:tms          # Build TMS Portal
bun run build:customer     # Build Customer Portal
bun run build:website      # Build Marketing Website
bun run build:all          # Build all projects

# API Generation
bun run gen:api            # Regenerate API client from OpenAPI spec

# Linting & Formatting
bun run lint
bun run format
```

## Architecture

### Shared Library (`@logistics/shared`)

Contains code shared between all Angular apps:

- `api/` - Generated API client (ng-openapi-gen)
- `interceptors/` - HTTP interceptors
- `guards/` - Route guards
- `services/` - Base services
- `models/` - Shared TypeScript types

### TMS Portal (`tms-portal`)

Internal application for dispatchers and managers:

- Component prefix: `app-`
- Uses all features from the original OfficeApp
- Full access to all API endpoints

### Customer Portal (`customer-portal`)

External customer self-service application:

- Component prefix: `cp-`
- Limited to customer-scoped API endpoints
- Features: shipment tracking, invoices, documents, account management

### Marketing Website (`website`)

Public-facing marketing website with SSR:

- Component prefix: `web-`
- Uses Angular SSR for server-side rendering
- Features: marketing pages, blog, contact forms, demo requests

## Import Conventions

```typescript
// Import from shared library
import { ApiConfiguration, provideApi } from "@logistics/shared";
import type { CustomerDto } from "@logistics/shared/api/models";
// Import within app
import { SomeService } from "@/core/services/some.service";
import { environment } from "@/env";
```

## Component Patterns

- Use standalone components (default)
- Use separate HTML template files (templateUrl, not template)
- Use signals + `@ngrx/signals` for state management
- Use `input()` and `output()` functions instead of decorators
- Use native control flow (`@if`, `@for`) instead of `*ngIf`, `*ngFor`
