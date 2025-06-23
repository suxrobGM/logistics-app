using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateTripHandler : RequestHandler<CreateTripCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IPushNotificationService _pushNotificationService;

    public CreateTripHandler(
        ITenantUnityOfWork tenantUow,
        IPushNotificationService pushNotificationService)
    {
        _tenantUow = tenantUow;
        _pushNotificationService = pushNotificationService;
    }

    protected override async Task<Result> HandleValidated(
        CreateTripCommand req, CancellationToken cancellationToken)
    {
        List<Load> loads = [];
        var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId);

        if (truck is null)
        {
            return Result.Fail($"Could not find the truck with ID '{req.TruckId}'");
        }

        if (req.Loads is not null)
        {
            loads = await _tenantUow.Repository<Load>().GetListAsync(i => req.Loads.Contains(i.Id));
        }
        
        var trip = Trip.Create(req.Name, req.PlannedStart, truck, loads);

        await _tenantUow.Repository<Trip>().AddAsync(trip);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
