using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

internal sealed class PostTruckToLoadBoardHandler(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderGateway gateway,
    ILogger<PostTruckToLoadBoardHandler> logger)
    : IRequestHandler<PostTruckToLoadBoardCommand, Result<PostTruckResultDto>>
{
    public async Task<Result<PostTruckResultDto>> Handle(
        PostTruckToLoadBoardCommand req,
        CancellationToken ct)
    {
        var providerConfig = await tenantUow.Repository<LoadBoardConfiguration>()
            .GetAsync(c => c.ProviderType == req.ProviderType && c.IsActive, ct);

        if (providerConfig is null)
        {
            return Result<PostTruckResultDto>.Fail(
                $"No active provider configuration found for {req.ProviderType}. Please configure the provider first.");
        }

        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
        if (truck is null)
        {
            return Result<PostTruckResultDto>.Fail("Truck not found");
        }

        var equipmentType = req.EquipmentType ?? DefaultEquipmentFor(truck.Type);
        if (equipmentType is null)
        {
            return Result<PostTruckResultDto>.Fail(
                $"Pick the equipment type to post: a {truck.Type.GetDescription()} has no load board equivalent.");
        }

        // One post per truck and board (unique index); an expired or booked one is replaced.
        var existingPost = await tenantUow.Repository<PostedTruck>()
            .GetAsync(p => p.TruckId == req.TruckId && p.ProviderType == req.ProviderType, ct);

        if (existingPost is { Status: PostedTruckStatus.Available })
        {
            return Result<PostTruckResultDto>.Fail(
                $"Truck is already posted to {req.ProviderType.GetDescription()}. Remove the existing post first or update it.");
        }

        var request = new PostTruckRequest
        {
            TruckId = req.TruckId,
            Origin = req.Origin,
            Destination = req.Destination,
            DestinationRadius = req.DestinationRadius,
            AvailableFrom = req.AvailableFrom,
            AvailableTo = req.AvailableTo,
            EquipmentType = equipmentType.Value,
            MaxWeight = req.MaxWeight,
            MaxLength = req.MaxLength
        };

        var postResult = await gateway.ExecuteAsync(providerConfig, p => p.PostTruckAsync(request, ct), ct);
        if (!postResult.IsSuccess || postResult.Value is null)
        {
            return Result<PostTruckResultDto>.Fail(postResult.Error ?? "Failed to post truck to load board");
        }

        var posted = postResult.Value;
        var postedTruck = existingPost ?? new PostedTruck
        {
            Truck = truck,
            ProviderType = req.ProviderType,
            // Boards locate a post by city/state or ZIP; there are no coordinates to store.
            AvailableAtLocation = new GeoPoint(0, 0),
            AvailableAtAddress = req.Origin.ToAddress()
        };

        postedTruck.TruckId = req.TruckId;
        postedTruck.ExternalPostId = posted.ExternalPostId;
        postedTruck.AvailableAtAddress = req.Origin.ToAddress();
        postedTruck.DestinationPreference = req.Destination?.ToAddress();
        postedTruck.DestinationRadius = req.DestinationRadius;
        postedTruck.AvailableFrom = req.AvailableFrom;
        postedTruck.AvailableTo = req.AvailableTo;
        postedTruck.EquipmentType = equipmentType;
        postedTruck.MaxWeight = req.MaxWeight;
        postedTruck.MaxLength = req.MaxLength;
        postedTruck.Status = PostedTruckStatus.Available;
        postedTruck.ExpiresAt = posted.ExpiresAt;
        postedTruck.LastRefreshedAt = DateTime.UtcNow;

        if (existingPost is null)
        {
            await tenantUow.Repository<PostedTruck>().AddAsync(postedTruck, ct);
        }

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Posted truck {TruckId} to {Provider}, external post ID: {ExternalPostId}",
            truck.Id, req.ProviderType, posted.ExternalPostId);

        return Result<PostTruckResultDto>.Ok(posted with { PostedTruckId = postedTruck.Id });
    }

    /// <summary>
    /// The board equipment a truck of this type is usually posted as; null when there is none
    /// (a tow truck) and the dispatcher has to choose.
    /// </summary>
    private static LoadBoardEquipmentType? DefaultEquipmentFor(TruckType truckType) => truckType switch
    {
        TruckType.Flatbed => LoadBoardEquipmentType.Flatbed,
        TruckType.FreightTruck or TruckType.SwapBody => LoadBoardEquipmentType.Van,
        TruckType.Reefer => LoadBoardEquipmentType.Reefer,
        TruckType.Tanker => LoadBoardEquipmentType.Tanker,
        TruckType.BoxTruck => LoadBoardEquipmentType.BoxTruck,
        TruckType.DumpTruck => LoadBoardEquipmentType.DumpTrailer,
        TruckType.CarHauler or TruckType.CarTransporter => LoadBoardEquipmentType.AutoCarrier,
        TruckType.ContainerTruck => LoadBoardEquipmentType.Container,
        TruckType.Tautliner or TruckType.Curtainsider => LoadBoardEquipmentType.Conestoga,
        TruckType.LowLoader => LoadBoardEquipmentType.Lowboy,
        _ => null
    };
}
