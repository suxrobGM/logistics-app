---
paths:
  - "src/Core/Logistics.Mappings/**/*.cs"
  - "src/Core/Logistics.Application/**/*.cs"
---

# Entity-to-DTO Mapping Rules

## Use Mapperly Only

- Do NOT manually map entity properties to DTOs in handlers
- All mapping must use Riok.Mapperly mappers in `Logistics.Mappings`
- Create extension methods for fluent API: `entity.ToDto()`

## Mapper Location

All mappers go in `src/Core/Logistics.Mappings/`

## Required Attributes

```csharp
[Mapper]
public static partial class EntityMapper
{
    [MapperIgnoreSource(nameof(Entity.NavigationProperty))]  // Ignore nav props
    [MapperIgnoreTarget(nameof(Dto.ComputedField))]          // Ignore computed
    public static partial EntityDto ToDto(this Entity entity);
}
```

## When to Ignore

- `[MapperIgnoreSource]` - Navigation properties, internal fields
- `[MapperIgnoreTarget]` - Computed fields set manually after mapping

## Computed Fields Pattern

```csharp
public static EntityDto ToDto(this Entity entity, int computedValue)
{
    var dto = entity.ToDto();  // Call generated mapper
    return dto with { ComputedField = computedValue };
}
```

## Collections

```csharp
var dtos = entities.Select(e => e.ToDto()).ToList();
```
