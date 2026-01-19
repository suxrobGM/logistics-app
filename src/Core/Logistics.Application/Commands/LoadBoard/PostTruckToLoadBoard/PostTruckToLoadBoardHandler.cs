using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class PostTruckToLoadBoardHandler(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderFactory providerFactory,
    ILogger<PostTruckToLoadBoardHandler> logger)
    : IAppRequestHandler<PostTruckToLoadBoardCommand, Result<PostTruckResultDto>>
{
    public async Task<Result<PostTruckResultDto>> Handle(
        PostTruckToLoadBoardCommand req,
        CancellationToken ct)
    {
        // Check if provider is supported
        if (!providerFactory.IsProviderSupported(req.ProviderType))
        {
            return Result<PostTruckResultDto>.Fail($"Load board provider '{req.ProviderType}' is not supported");
        }

        // Get provider configuration
        var providerConfig = await tenantUow.Repository<LoadBoardConfiguration>()
            .GetAsync(c => c.ProviderType == req.ProviderType && c.IsActive, ct);

        if (providerConfig is null)
        {
            return Result<PostTruckResultDto>.Fail(
                $"No active provider configuration found for {req.ProviderType}. Please configure the provider first.");
        }

        // Get the truck
        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
        if (truck is null)
        {
            return Result<PostTruckResultDto>.Fail("Truck not found");
        }

        // Check if truck is already posted to this provider
        var existingPost = await tenantUow.Repository<PostedTruck>()
            .GetAsync(p => p.TruckId == req.TruckId
                && p.ProviderType == req.ProviderType
                && p.Status == PostedTruckStatus.Available, ct);

        if (existingPost is not null)
        {
            return Result<PostTruckResultDto>.Fail(
                $"Truck is already posted to {req.ProviderType}. Remove the existing post first or update it.");
        }

        // Post to the load board provider
        var provider = providerFactory.GetProvider(providerConfig);
        var postResult = await provider.PostTruckAsync(new PostTruckRequest
        {
            TruckId = req.TruckId,
            ProviderType = req.ProviderType,
            AvailableAtAddress = req.AvailableAtAddress,
            AvailableAtLocation = req.AvailableAtLocation,
            DestinationPreference = req.DestinationPreference,
            DestinationRadius = req.DestinationRadius,
            AvailableFrom = req.AvailableFrom,
            AvailableTo = req.AvailableTo,
            EquipmentType = req.EquipmentType ?? truck.Type.ToString(),
            MaxWeight = req.MaxWeight,
            MaxLength = req.MaxLength
        });

        if (!postResult.Success)
        {
            return Result<PostTruckResultDto>.Fail(
                postResult.ErrorMessage ?? "Failed to post truck to load board");
        }

        // Create the posted truck record
        var postedTruck = new PostedTruck
        {
            TruckId = req.TruckId,
            Truck = truck,
            ProviderType = req.ProviderType,
            ExternalPostId = postResult.ExternalPostId,
            AvailableAtAddress = req.AvailableAtAddress,
            AvailableAtLocation = req.AvailableAtLocation,
            DestinationPreference = req.DestinationPreference,
            DestinationRadius = req.DestinationRadius,
            AvailableFrom = req.AvailableFrom,
            AvailableTo = req.AvailableTo,
            EquipmentType = req.EquipmentType ?? truck.Type.ToString(),
            MaxWeight = req.MaxWeight,
            MaxLength = req.MaxLength,
            Status = PostedTruckStatus.Available,
            ExpiresAt = postResult.ExpiresAt,
            LastRefreshedAt = DateTime.UtcNow
        };

        await tenantUow.Repository<PostedTruck>().AddAsync(postedTruck, ct);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Posted truck {TruckId} to {Provider}, external post ID: {ExternalPostId}",
            truck.Id, req.ProviderType, postResult.ExternalPostId);

        return Result<PostTruckResultDto>.Ok(new PostTruckResultDto
        {
            Success = true,
            ExternalPostId = postResult.ExternalPostId,
            PostedTruckId = postedTruck.Id,
            ExpiresAt = postResult.ExpiresAt
        });
    }
}
