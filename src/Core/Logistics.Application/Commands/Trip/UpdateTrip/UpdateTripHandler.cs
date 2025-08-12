using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateTripHandler : RequestHandler<UpdateTripCommand, Result>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ITenantUnityOfWork _tenantUow;

    public UpdateTripHandler(
        ITenantUnityOfWork tenantUow,
        IPushNotificationService pushNotificationService)
    {
        _tenantUow = tenantUow;
        _pushNotificationService = pushNotificationService;
    }

    protected override async Task<Result> HandleValidated(
        UpdateTripCommand req, CancellationToken ct)
    {
        List<Load> loads = [];

        if (req.Loads is not null)
            loads = await _tenantUow.Repository<Load>().GetListAsync(i => req.Loads.Contains(i.Id));

        var trip = await _tenantUow.Repository<Trip>().GetByIdAsync(req.TripId);

        if (trip is null) return Result.Fail($"Trip not found with ID {req.TripId}");

        trip.Name = PropertyUpdater.UpdateIfChanged(req.Name, trip.Name);
        trip.PlannedStart = PropertyUpdater.UpdateIfChanged(req.PlannedStart, trip.PlannedStart);

        // Update trip loads
        if (loads.Count > 0) trip.UpdateTripLoads(loads);

        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
