using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

internal sealed class UpdatePostedTruckHandler(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderGateway gateway)
    : IRequestHandler<UpdatePostedTruckCommand, Result>
{
    public async Task<Result> Handle(UpdatePostedTruckCommand req, CancellationToken ct)
    {
        var postedTruck = await tenantUow.Repository<PostedTruck>().GetByIdAsync(req.PostedTruckId, ct);
        if (postedTruck is null)
        {
            return Result.Fail("Posted truck not found");
        }

        if (postedTruck.Status != PostedTruckStatus.Available || string.IsNullOrEmpty(postedTruck.ExternalPostId))
        {
            return Result.Fail("Only an active post can be updated. Post the truck again instead.");
        }

        var providerConfig = await tenantUow.Repository<LoadBoardConfiguration>()
            .GetAsync(c => c.ProviderType == postedTruck.ProviderType && c.IsActive, ct);
        if (providerConfig is null)
        {
            return Result.Fail($"No active provider configuration found for {postedTruck.ProviderType.GetDescription()}.");
        }

        var request = new PostTruckRequest
        {
            TruckId = postedTruck.TruckId,
            Origin = req.Origin,
            Destination = req.Destination,
            DestinationRadius = req.DestinationRadius,
            AvailableFrom = req.AvailableFrom,
            AvailableTo = req.AvailableTo,
            EquipmentType = req.EquipmentType,
            MaxWeight = req.MaxWeight,
            MaxLength = req.MaxLength
        };

        var externalPostId = postedTruck.ExternalPostId;
        var result = await gateway.ExecuteAsync(
            providerConfig, p => p.UpdateTruckPostAsync(externalPostId, request, ct), ct);
        if (!result.IsSuccess)
        {
            return result;
        }

        postedTruck.AvailableAtAddress = req.Origin.ToAddress();
        postedTruck.DestinationPreference = req.Destination?.ToAddress();
        postedTruck.DestinationRadius = req.DestinationRadius;
        postedTruck.AvailableFrom = req.AvailableFrom;
        postedTruck.AvailableTo = req.AvailableTo;
        postedTruck.EquipmentType = req.EquipmentType;
        postedTruck.MaxWeight = req.MaxWeight;
        postedTruck.MaxLength = req.MaxLength;
        postedTruck.LastRefreshedAt = DateTime.UtcNow;

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
