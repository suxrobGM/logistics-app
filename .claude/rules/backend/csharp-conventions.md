---
paths:
  - "**/*.cs"
---

# C# Conventions

`.editorconfig` and the analyzers enforce style, casing and nullability. This file is what they don't check.

- Primary constructors for DI; one type per file.
- Extension members go in C# 14 `extension(T receiver) { }` blocks, except Mapperly's `partial` mapping methods, which stay classic `this` methods.
- `Async` suffix on async methods except request handlers. Always take a `CancellationToken`; never `.Result` / `.Wait()`.
- **Never `(decimal)x` on a `double`/`float`.** Since .NET 11 it keeps the binary noise (`(decimal)0.3f` is 0.30000001192…). Use `x.ToDecimal()` (`FloatingPointExtensions`).
- **No static helpers taking a unit of work.** Make an injected instance service. Extension methods that read as a UoW operation (`tenantUow.ReserveSequencesAsync`) are the exception.
- Raise domain events in entity methods; event handlers live in `Modules/{Module}/{Feature}/Events/`.
- Build a `TaxJurisdiction` with `TaxJurisdiction.Create(country, region)` on every write path; it upper-cases and turns a blank region into null, which the equality checks rely on.
- Filter receivables on `InvoiceStatuses.Open` / `Receivable`; never list invoice statuses inline.

## Naming

- `{Action}{Entity}Command` → `{Action}{Entity}Handler` (`internal sealed`). `Get{Entity}ByIdQuery`, `Get{Entities}Query`.
- `{Entity}Dto`, `{Entity}Mapper`. Private fields `_camelCase`; constants and static readonly `PascalCase`.

## Validation

No validator whose only rule is `Id NotEmpty`; the not-found path covers it. A Create/Update pair sharing three or more rules uses a shared base validator (see Ifta `TaxRates`).

## Enums

`GetDescription()` humanizes names (`PickedUp` → "Picked Up"). Add `[Description]` only for acronyms or special wording (`Eld` → "ELD / HOS").

## Sorting

Sort parameters are `-field` (descending) or `field`, camelCase like every query value; the parser ignores case. It rejects `"field desc"`. Every client uses this form.
