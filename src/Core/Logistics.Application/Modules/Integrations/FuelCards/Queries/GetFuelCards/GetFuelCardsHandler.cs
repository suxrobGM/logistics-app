using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Queries;

internal sealed class GetFuelCardsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetFuelCardsQuery, Result<List<FuelCardDto>>>
{
    public async Task<Result<List<FuelCardDto>>> Handle(GetFuelCardsQuery req, CancellationToken ct)
    {
        var cards = await tenantUow.Repository<FuelCard>().GetListAsync(ct: ct);
        return Result<List<FuelCardDto>>.Ok(cards.Select(c => c.ToDto()).ToList());
    }
}
