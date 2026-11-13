---
paths:
  - "src/Core/Logistics.Application/**/*.cs"
---

# Entity-to-DTO Mapping (Mapperly)

- All mapping via Riok.Mapperly mappers next to their feature
  (`Modules/{Module}/{Feature}/{Entity}Mapper.cs`, namespace = the feature's) - no manual mapping in
  handlers. Shared `MappingConversions` and the assembly-wide `MapperDefaults` sit in `Common/Mapping/`
- Extension method pattern: `entity.ToDto()`
- `[MapperIgnoreSource]` for navigation properties only. `MapperDefaults.cs` sets
  `RequiredMappingStrategy.Target` assembly-wide, so an unmapped entity column is fine and only an
  unfilled DTO field warns (RMG012)
- `[MapperIgnoreTarget]` for computed fields set manually after mapping
- Display strings and null defaults go through `MappingConversions` (`Describe`, `DefaultToEmpty`,
  `HasText`) via `Use = nameof(@MappingConversions.X)`. A `Use` method must accept the member's exact
  type, nullability included - a non-nullable parameter on a nullable member makes Mapperly throw on null
- When the public `ToDto` takes extra arguments or reads a navigation, make the partial a private
  `MapX(Entity entity)` and fill the rest in the public method

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
**per row** - an N+1 that is invisible in the mapper. `[MapperIgnoreTarget]` it, batch the lookup
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
