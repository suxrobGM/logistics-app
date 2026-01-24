---
name: angular-workspace
description: Angular workspace commands and directory reference
---

# Logistics Angular Workspace

Location: `src/Client/Logistics.Angular`

## Projects

| Project           | Type        | Port | Description                       |
| ----------------- | ----------- | ---- | --------------------------------- |
| `shared`          | library     | N/A  | Shared API client and services    |
| `tms-portal`      | application | 7003 | Internal TMS for dispatchers      |
| `customer-portal` | application | 7004 | Customer self-service portal      |
| `website`         | application | 7005 | Marketing website (SSR)           |

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

## Shared Library Structure

`@logistics/shared` contains code shared between all Angular apps:

```text
projects/shared/src/lib/
├── api/              # Generated API client (ng-openapi-gen)
├── components/       # Shared UI components
├── interceptors/     # HTTP interceptors (auth, error handling)
├── guards/           # Route guards
├── services/         # Base services
└── models/           # Shared TypeScript types
```

## Import Paths

```typescript
// Shared library
import { ApiConfiguration, provideApi } from "@logistics/shared";
import type { CustomerDto } from "@logistics/shared/api/models";

// Within app (path aliases)
import { SomeService } from "@/core/services/some.service";
import { environment } from "@/env";
```

## PrimeNG UI Components

The workspace uses PrimeNG for UI components. Common imports:

```typescript
imports: [
  ButtonModule,
  TableModule,
  DialogModule,
  InputTextModule,
  DropdownModule,
  CalendarModule,
  ToastModule,
]
```
