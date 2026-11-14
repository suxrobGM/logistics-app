---
paths:
  - "src/Presentation/Logistics.API/**/*.cs"
---

# API Design

## Routes and controllers

- Plural lowercase nouns, no hyphens: `/loads`, `/loads/{id}/trips`. Custom actions are sub-resources: `POST /loads/import`.
- `[Route("resources")]`, `[Produces("application/json")]`, primary constructor, `[ProducesResponseType]` on every response, `[Authorize(Policy = Permission.X.View|Manage)]` on every action.
- `return result.ToActionResult();` gives 200 (204 for a plain `Result`), 404 for `ErrorCodes.NotFound`, otherwise 400. Use `ErrorResponse.FromResult` by hand only for other statuses. No internal details in messages.

## Binding

- Bind the command or query directly as the body: `Create([FromBody] CreateLoadCommand request)`. No `*Dto` wrapper on writes.
- Route ids are assigned: `request.Id = id;`.
- Add a `*Request` record only when the wire shape must differ from the command.

## Query strings

Query parameters and form fields are camelCase on the wire (`ParameterNameTransformer`); clients regenerate from OpenAPI. One name per concept, enforced by `QueryParameterNameTests`:

- Lists extend `PagedQuery` (`page`, `pageSize`, `orderBy`) or `SearchableQuery` (+ `search`). Free text goes in `search`, never `customerName`-style filters.
- Date ranges are `fromDate` / `toDate` (`DateRangeQuery`, `PagedDateRangeQuery`).
- Flags: `xOnly` narrows (`activeOnly`, `overdueOnly`), `includeX` widens (`includeLoads`).
- Multi-value filters are plural arrays (`statuses`); single-value filters are singular (`status`). Name ids after the role filtered (`driverId`, `employeeId`).

## Error codes

When a client must branch on a failure, return a code, never rely on message text:

```csharp
return Result<T>.Fail($"Credit score {score} is below your minimum of {min}.", ErrorCodes.BrokerCreditBelowThreshold);
```

Add the constant to `src/Shared/Logistics.Shared.Models/ErrorCodes.cs` and mirror it in the Angular `projects/shared/src/lib/errors/upgrade-handler.ts`.

## Security

- **Tenant isolation:** a handler that calls `Repository<T>()` without a resolved tenant reads the wrong database and nothing fails.
- **Uploads:** extension allowlist plus a matching MIME type, `[RequestSizeLimit]`, stored in blob storage.
- **Raw SQL:** `FromSqlInterpolated`, never `FromSqlRaw` with concatenation.
- **Inbound webhooks:** use the `add-webhook-handler` skill.
