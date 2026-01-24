---
paths:
  - "src/Presentation/Logistics.API/**/*.cs"
---

# API Design Standards

## REST Endpoint Naming

- Use plural nouns for resources: `/loads`, `/customers`, `/trucks`
- Use lowercase only - no camelCase or PascalCase in URLs
- Avoid hyphens in endpoint names - prefer single words or concatenation
- Use path parameters for resource identifiers: `/loads/{id}`

### Good Examples

```text
GET    /loads              # List loads
GET    /loads/{id}         # Get single load
POST   /loads              # Create load
PUT    /loads/{id}         # Update load
DELETE /loads/{id}         # Delete load
POST   /loads/import       # Custom action (verb as sub-resource)
GET    /loads/{id}/trips   # Nested resource
```

### Avoid

```text
GET /get-loads            # Don't use verbs in resource names
GET /load-list            # Don't use hyphens
GET /LoadsByCustomer      # Don't use camelCase/PascalCase
POST /loads/create-new    # Don't use hyphens or redundant verbs
```

## Controller Structure

- Route attribute: `[Route("resources")]` (plural, lowercase)
- Use primary constructor for dependency injection
- Include `[Produces("application/json")]` attribute
- Use `[ProducesResponseType]` for all response types

## HTTP Methods

| Action | Method | Route | Returns |
|--------|--------|-------|---------|
| List | GET | `/resources` | `PagedResponse<Dto>` |
| Get | GET | `/resources/{id}` | `Dto` or 404 |
| Create | POST | `/resources` | `Dto` or 204 |
| Update | PUT | `/resources/{id}` | 204 or 400 |
| Delete | DELETE | `/resources/{id}` | 204 or 404 |
| Custom | POST | `/resources/{action}` | Varies |

## Authorization

- Always include `[Authorize(Policy = Permission.X.Y)]`
- Use `Permission.{Entity}.View` for read operations
- Use `Permission.{Entity}.Manage` for write operations

## Error Responses

- Return `ErrorResponse.FromResult(result)` for failures
- Use appropriate status codes: 400 (bad request), 404 (not found)
- Never expose internal exception details
