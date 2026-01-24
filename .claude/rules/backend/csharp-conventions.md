---
paths:
  - "**/*.cs"
---

# C# Code Conventions

## Namespace & File Structure

- Use file-scoped namespaces (`namespace Foo;` not `namespace Foo { }`)
- One type per file, filename matches type name
- Use primary constructors for dependency injection

## Code Style

- Use `var` only when type is apparent from the right-hand side
- Prefer pattern matching over type checks and casts
- Use expression-bodied members for single-line methods/properties
- Use collection expressions (`[1, 2, 3]`) over `new List<int> { 1, 2, 3 }`

## Async/Await

- All async methods must end with `Async` suffix (except handlers)
- Use `CancellationToken` parameter in async methods
- Never use `.Result` or `.Wait()` - always await

## Null Handling

- Use nullable reference types (`string?` for nullable)
- Prefer `is null` / `is not null` over `== null`
- Use null-coalescing (`??`) and null-conditional (`?.`) operators

## EF Core

- Lazy loading is enabled - do NOT use `.Include()` for navigation properties
- Navigation properties load automatically on access
- Use specifications for reusable query filters

## Domain Events

- Use domain events for notifications and cross-cutting concerns
- Raise events in entity methods, not in command handlers
- Event handlers in `Logistics.Application/Events/`

## Naming

- Commands: `{Action}{Entity}Command` (e.g., `CreateLoadCommand`)
- Queries: `Get{Entity}ByIdQuery`, `Get{Entities}Query`
- Handlers: `{Command}Handler` (internal sealed class)
- DTOs: `{Entity}Dto`
- Mappers: `{Entity}Mapper`
