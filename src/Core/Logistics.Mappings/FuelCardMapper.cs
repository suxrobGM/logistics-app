using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

/// <summary>
/// TruckNumber is deliberately not flattened from the Truck navigation property — that would
/// lazy-load one query per row. Callers batch-load the numbers and pass them in.
/// </summary>
[Mapper]
public static partial class FuelCardMapper
{
    [MapperIgnoreSource(nameof(FuelCardTransaction.Truck))]
    [MapperIgnoreSource(nameof(FuelCardTransaction.ExternalCardId))]
    [MapperIgnoreSource(nameof(FuelCardTransaction.RawPayloadJson))]
    [MapperIgnoreSource(nameof(FuelCardTransaction.DomainEvents))]
    [MapperIgnoreSource(nameof(FuelCardTransaction.CreatedAt))]
    [MapperIgnoreSource(nameof(FuelCardTransaction.CreatedBy))]
    [MapperIgnoreSource(nameof(FuelCardTransaction.UpdatedAt))]
    [MapperIgnoreSource(nameof(FuelCardTransaction.UpdatedBy))]
    [MapperIgnoreTarget(nameof(FuelCardTransactionDto.TruckNumber))]
    public static partial FuelCardTransactionDto ToDto(this FuelCardTransaction entity);

    public static FuelCardTransactionDto ToDto(this FuelCardTransaction entity, string? truckNumber)
    {
        var dto = entity.ToDto();
        return dto with { TruckNumber = truckNumber };
    }
}
