using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.FuelCards.Queries;

internal sealed class GetFuelCardsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetFuelCardsQuery, Result<List<FuelCardDto>>>
{
    public async Task<Result<List<FuelCardDto>>> Handle(GetFuelCardsQuery req, CancellationToken ct)
    {
        var cards = await tenantUow.Repository<FuelCard>().GetListAsync(ct: ct);

        // One query for every mapped truck — reading Card.Truck per row would lazy-load per card.
        var truckIds = cards.Select(c => c.TruckId).OfType<Guid>().Distinct().ToArray();
        var truckNumbers = truckIds.Length == 0
            ? []
            : await tenantUow.Repository<Truck>().Query()
                .Where(t => truckIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Number, ct);

        var dtos = cards
            .Select(c => c.ToDto(c.TruckId is { } id ? truckNumbers.GetValueOrDefault(id) : null))
            .ToList();

        return Result<List<FuelCardDto>>.Ok(dtos);
    }
}
