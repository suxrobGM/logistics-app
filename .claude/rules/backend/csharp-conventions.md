---
paths:
  - "**/*.cs"
---

# C# Code Conventions

## Style

- File-scoped namespaces, one type per file matching filename
- Primary constructors for DI
- `var` only when type is apparent from RHS
- Prefer pattern matching, expression-bodied members, collection expressions (`[1, 2, 3]`)

## Async

- `Async` suffix on async methods (except handlers)
- Always accept `CancellationToken`, never use `.Result` or `.Wait()`

## Null Handling

- Use nullable reference types (`string?`), `is null` / `is not null`, `??`, `?.`

## EF Core

- Lazy loading enabled — do NOT use `.Include()` for navigation properties

## Domain Events

- Raise events in entity methods, not handlers. Handlers in `Logistics.Application/Events/`

## Naming

- Commands: `{Action}{Entity}Command` → Handlers: `{Command}Handler` (internal sealed)
- Queries: `Get{Entity}ByIdQuery`, `Get{Entities}Query`
- DTOs: `{Entity}Dto`, Mappers: `{Entity}Mapper`
- Private fields: `camelCase` (no `_` prefix) — enforced by `.editorconfig`
- Constants and static readonly: `PascalCase`

## Enum Display Names

Use `GetDescription()` from `Logistics.Domain.Primitives.Enums.EnumExtensions` for display strings — it auto-humanizes enum names (`PickedUp` → "Picked Up"). Only use `[Description]` when humanization isn't enough: acronyms (`Eld` → "ELD / HOS"), special formatting (`OnDutyNotDriving` → "On Duty (Not Driving)"), domain-specific (`Driving11Hour` → "11-Hour Driving Limit").

## API Query Sort Syntax

Sort parameters use `-FieldName` for descending, plain `FieldName` for ascending. Do NOT use `"FieldName desc"` / `"FieldName asc"` — the API parser doesn't accept that form. Same syntax applies on every client (Angular, Kotlin driver app, MCP).
