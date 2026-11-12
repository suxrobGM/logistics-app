using Logistics.Mediator;
using Microsoft.EntityFrameworkCore;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Queries;

internal sealed class GetPostedTrucksHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetPostedTrucksQuery, Result<List<PostedTruckDto>>>
{
    public async Task<Result<List<PostedTruckDto>>> Handle(GetPostedTrucksQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<PostedTruck>().Query();

        if (req.ProviderType.HasValue)
        {
            query = query.Where(p => p.ProviderType == req.ProviderType.Value);
        }

        if (req.Status.HasValue)
        {
            query = query.Where(p => p.Status == req.Status.Value);
        }

        if (req.TruckId.HasValue)
        {
            query = query.Where(p => p.TruckId == req.TruckId.Value);
        }

        var postedTrucks = await query.ToListAsync(ct);

        // Order by CreatedAt descending in memory
        var dtos = postedTrucks
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostedTruckDto
            {
                Id = p.Id,
                TruckId = p.TruckId,
                TruckNumber = p.Truck.Number,
                ProviderType = p.ProviderType,
                ProviderName = p.ProviderType.ToString(),
                ExternalPostId = p.ExternalPostId,
                AvailableAtAddress = p.AvailableAtAddress,
                AvailableAtLocation = p.AvailableAtLocation,
                DestinationPreference = p.DestinationPreference,
                DestinationRadius = p.DestinationRadius,
                AvailableFrom = p.AvailableFrom,
                AvailableTo = p.AvailableTo,
                EquipmentType = p.EquipmentType,
                MaxWeight = p.MaxWeight,
                MaxLength = p.MaxLength,
                Status = p.Status,
                ExpiresAt = p.ExpiresAt,
                LastRefreshedAt = p.LastRefreshedAt,
                CreatedAt = p.CreatedAt
            }).ToList();

        return Result<List<PostedTruckDto>>.Ok(dtos);
    }

}
