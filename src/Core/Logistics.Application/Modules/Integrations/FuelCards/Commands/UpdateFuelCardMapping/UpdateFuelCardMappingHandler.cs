using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

internal sealed class UpdateFuelCardMappingHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateFuelCardMappingCommand, Result>
{
    public async Task<Result> Handle(UpdateFuelCardMappingCommand req, CancellationToken ct)
    {
        var card = await tenantUow.Repository<FuelCard>().GetByIdAsync(req.FuelCardId, ct);
        if (card is null)
        {
            return Result.Fail("Fuel card not found");
        }

        if (req.TruckId.HasValue)
        {
            var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId.Value, ct);
            if (truck is null)
            {
                return Result.Fail("Truck not found");
            }
        }

        card.TruckId = req.TruckId;
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
