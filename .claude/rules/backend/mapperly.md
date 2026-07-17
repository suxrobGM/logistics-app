---
paths:
  - "src/Core/Logistics.Mappings/**/*.cs"
  - "src/Core/Logistics.Application/**/*.cs"
---

# Entity-to-DTO Mapping (Mapperly)

- All mapping via Riok.Mapperly mappers in `src/Core/Logistics.Mappings/` — no manual mapping in handlers
- Extension method pattern: `entity.ToDto()`
- `[MapperIgnoreSource]` for navigation properties, internal fields
- `[MapperIgnoreTarget]` for computed fields set manually after mapping

```csharp
[Mapper]
public static partial class EntityMapper
{
    [MapperIgnoreSource(nameof(Entity.NavProp))]
    public static partial EntityDto ToDto(this Entity entity);
}

// Computed fields: map then override with `with` expression
public static EntityDto ToDto(this Entity entity, int computed)
{
    var dto = entity.ToDto();
    return dto with { ComputedField = computed };
}
```

## Never flatten a navigation property

Lazy loading is on, so letting Mapperly flatten `Truck.Number` into `TruckNumber` costs one SELECT
**per row** — an N+1 that is invisible in the mapper. `[MapperIgnoreTarget]` it, batch the lookup
in the handler, pass the value in:

```csharp
[MapperIgnoreSource(nameof(FuelCardTransaction.Truck))]
[MapperIgnoreTarget(nameof(FuelCardTransactionDto.TruckNumber))]
public static partial FuelCardTransactionDto ToDto(this FuelCardTransaction entity);

public static FuelCardTransactionDto ToDto(this FuelCardTransaction entity, string? truckNumber)
{
    var dto = entity.ToDto();
    return dto with { TruckNumber = truckNumber };
}
```

Worked example: `FuelCardMapper` + `GetFuelCardTransactionsHandler`.
