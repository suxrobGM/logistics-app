---
paths:
  - "src/Core/Logistics.Application/**/*.cs"
---

# Mapperly

- Mappers sit next to their feature (`Modules/{Module}/{Feature}/{Entity}Mapper.cs`). No manual mapping in handlers. Shared `MappingConversions` and `MapperDefaults` are in `Common/Mapping/`.
- `MapperDefaults` sets `RequiredMappingStrategy.Target`, so only an unfilled DTO field warns (RMG012).
- `[MapperIgnoreSource]` for navigations; `[MapperIgnoreTarget]` for fields filled after mapping.
- Display strings and null defaults use `MappingConversions` via `Use = nameof(...)`. The `Use` method must accept the member's exact type, nullability included, or Mapperly throws on null.

## Never flatten a navigation

Letting Mapperly flatten `Truck.Number` into `TruckNumber` lazy-loads one row per item. Ignore it, batch the lookup in the handler and pass it in:

```csharp
[MapperIgnoreSource(nameof(FuelCardTransaction.Truck))]
[MapperIgnoreTarget(nameof(FuelCardTransactionDto.TruckNumber))]
public static partial FuelCardTransactionDto ToDto(this FuelCardTransaction entity);

public static FuelCardTransactionDto ToDto(this FuelCardTransaction entity, string? truckNumber) =>
    entity.ToDto() with { TruckNumber = truckNumber };
```

When the public `ToDto` needs extra arguments, make the generated partial a private `MapX` and wrap it. Example: `FuelCardMapper` + `GetFuelCardTransactionsHandler`.
