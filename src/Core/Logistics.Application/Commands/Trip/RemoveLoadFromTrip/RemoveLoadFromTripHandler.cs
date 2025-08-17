using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class RemoveLoadFromTripHandler : IAppRequestHandler<RemoveLoadFromTripCommand, Result>
{
    private readonly ILoadService _loadService;
    private readonly ILogger<RemoveLoadFromTripHandler> _logger;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ITenantUnitOfWork _tenantUow;

    public RemoveLoadFromTripHandler(
        ITenantUnitOfWork tenantUow,
        ILoadService loadService,
        IPushNotificationService pushNotificationService,
        ILogger<RemoveLoadFromTripHandler> logger)
    {
        _tenantUow = tenantUow;
        _loadService = loadService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveLoadFromTripCommand req, CancellationToken ct)
    {
        var trip = await _tenantUow.Repository<Trip>().GetByIdAsync(req.TripId, ct);

        if (trip is null)
        {
            return Result.Fail($"Could not find the trip with ID '{req.TripId}'");
        }

        await _loadService.DeleteLoadAsync(req.LoadId);

        _logger.LogInformation("Removed load {LoadId} from trip '{TripName}' with ID '{TripId}'", req.LoadId, trip.Name,
            trip.Id);
        return Result.Ok();
    }
}
