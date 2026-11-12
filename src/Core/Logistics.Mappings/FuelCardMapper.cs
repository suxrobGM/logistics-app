using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

/// <summary>
/// TruckNumber is deliberately not flattened from the Truck navigation property - that would
/// lazy-load one query per row. Callers batch-load the numbers and pass them in.
/// </summary>
[Mapper]
public static partial class FuelCardMapper
{
    [MapperIgnoreSource(nameof(FuelCardTransaction.Truck))]
    [MapperIgnoreTarget(nameof(FuelCardTransactionDto.TruckNumber))]
    public static partial FuelCardTransactionDto ToDto(this FuelCardTransaction entity);

    extension(FuelCardTransaction entity)
    {
        public FuelCardTransactionDto ToDto(string? truckNumber)
        {
            var dto = entity.ToDto();
            return dto with { TruckNumber = truckNumber };
        }
    }
}
