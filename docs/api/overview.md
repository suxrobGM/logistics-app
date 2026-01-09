# API Overview

The Logistics API is a RESTful service built with ASP.NET Core.

## Base URL

| Environment | URL |
|-------------|-----|
| Development | <https://localhost:7000> |
| Production | <https://api.yourdomain.com> |

## Swagger Documentation

Interactive API documentation is available at:

- Development: <https://localhost:7000/swagger>
- Production: <https://api.yourdomain.com/swagger> (if enabled)

## Authentication

All API endpoints (except health checks) require authentication via JWT Bearer tokens.

```http
GET /api/loads HTTP/1.1
Host: api.yourdomain.com
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

See [Authentication](authentication.md) for details on obtaining tokens.

## Response Format

### Success Response

```json
{
  "isSuccess": true,
  "data": { ... },
  "error": null
}
```

### Error Response

```json
{
  "isSuccess": false,
  "data": null,
  "error": "Error message describing what went wrong"
}
```

### Paginated Response

```json
{
  "isSuccess": true,
  "data": {
    "items": [ ... ],
    "totalItems": 100,
    "totalPages": 10,
    "currentPage": 1,
    "pageSize": 10
  }
}
```

## API Endpoints

### Core Resources

| Resource | Endpoint | Description |
|----------|----------|-------------|
| Loads | `/api/loads` | Shipment management |
| Trips | `/api/trips` | Trip tracking |
| Customers | `/api/customers` | Customer management |
| Employees | `/api/employees` | Employee records |
| Drivers | `/api/drivers` | Driver-specific data |
| Trucks | `/api/trucks` | Fleet management |
| Invoices | `/api/invoices` | Billing |
| Payments | `/api/payments` | Payment processing |

### Administrative

| Resource | Endpoint | Description |
|----------|----------|-------------|
| Users | `/api/users` | User management |
| Roles | `/api/roles` | Role management |
| Tenants | `/api/tenants` | Tenant management |
| Subscriptions | `/api/subscriptions` | Billing plans |

### Utilities

| Resource | Endpoint | Description |
|----------|----------|-------------|
| Documents | `/api/documents` | File management |
| Notifications | `/api/notifications` | Push notifications |
| Reports | `/api/reports` | Report generation |
| Stats | `/api/stats` | Analytics data |

## Common Query Parameters

### Pagination

```
GET /api/loads?page=1&pageSize=20
```

### Sorting

```
GET /api/loads?orderBy=createdDate&descending=true
```

### Filtering

```
GET /api/loads?status=active&customerId=123
```

### Search

```
GET /api/loads?search=container
```

## HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request (validation error) |
| 401 | Unauthorized (no/invalid token) |
| 403 | Forbidden (insufficient permissions) |
| 404 | Not Found |
| 500 | Internal Server Error |

## Rate Limiting

Production APIs implement rate limiting:

- **Standard endpoints**: 100 requests/minute
- **Authentication**: 10 requests/minute
- **Webhooks**: No limit

## Webhooks

Stripe webhooks are received at:

```
POST /webhooks/stripe
```

See [Webhooks](../configuration/stripe-integration.md) for configuration.

## Real-Time (SignalR)

For real-time features, connect to SignalR hubs:

- **GPS Tracking**: `/hubs/live-tracking`
- **Notifications**: `/hubs/notification`

See [SignalR Hubs](signalr-hubs.md) for details.

## API Client Generation

The Angular Office App generates its API client from OpenAPI:

```bash
cd src/Client/Logistics.OfficeApp
bun run gen:api
```

This creates TypeScript types and services matching the API.

## Next Steps

- [Authentication](authentication.md) - OAuth2 flow
- [Endpoints](endpoints.md) - Detailed endpoint reference
- [SignalR Hubs](signalr-hubs.md) - Real-time features
