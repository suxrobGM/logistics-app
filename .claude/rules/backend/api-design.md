---
paths:
  - "src/Presentation/Logistics.API/**/*.cs"
---

# API Design Standards

## REST Endpoints

- Plural lowercase nouns: `/loads`, `/customers`, `/trucks`
- No hyphens, no camelCase/PascalCase in URLs
- Path params for IDs: `/loads/{id}`, nested: `/loads/{id}/trips`
- Custom actions as sub-resources: `POST /loads/import`

## Controller Structure

- Route: `[Route("resources")]`, `[Produces("application/json")]`
- Primary constructor for DI, `[ProducesResponseType]` for all responses
- Auth: `[Authorize(Policy = Permission.{Entity}.View|Manage)]`
- Errors: `ErrorResponse.FromResult(result)`, appropriate status codes, no internal details

## Request binding

- Bind the command/query **directly** as the request body — `Create([FromBody] CreateLoadCommand request)`. The MediatR request _is_ the wire contract; there is no `*Dto` wrapper on the write side.
- For `PUT`/`DELETE`/`GET {id}`, take the id from the route and assign it: `request.Id = id;` (or `new GetLoadByIdQuery { Id = id }`).
- Add a dedicated `*Request` record **only** when the wire shape must differ from the command (e.g. the client sends a subset, or a field is server-derived). Map it to the command in the controller. Don't introduce one by default.

## Machine-readable errors

When the client must _branch_ on a failure, carry a code — never a prefix or substring of the
message. The message is prose and gets copy-edited; a code is a contract.

```csharp
return Result<T>.Fail($"Credit score {score} is below your minimum of {min}.",
    ErrorCodes.BrokerCreditBelowThreshold);
```

`ErrorResponse.FromResult` serializes it as `errorCode`; the frontend reads `err.error.errorCode`.
Add the constant to `src/Shared/Logistics.Shared.Models/ErrorCodes.cs` **and** mirror it in
`projects/shared/src/lib/errors/upgrade-handler.ts`.

## HTTP Methods

| Action | Method | Route             | Returns              |
| ------ | ------ | ----------------- | -------------------- |
| List   | GET    | `/resources`      | `PagedResponse<Dto>` |
| Get    | GET    | `/resources/{id}` | `Dto` or 404         |
| Create | POST   | `/resources`      | `Dto` or 204         |
| Update | PUT    | `/resources/{id}` | 204 or 400           |
| Delete | DELETE | `/resources/{id}` | 204 or 404           |
