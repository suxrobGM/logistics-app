using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Notifications;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

internal sealed class DeleteTripHandler : IAppRequestHandler<DeleteTripCommand, Result>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ITenantUnitOfWork _tenantUow;

    public DeleteTripHandler(
        ITenantUnitOfWork tenantUow,
        IPushNotificationService pushNotificationService)
    {
        _tenantUow = tenantUow;
        _pushNotificationService = pushNotificationService;
    }

    public async Task<Result> Handle(
        DeleteTripCommand req, CancellationToken ct)
    {
        var trip = await _tenantUow.Repository<Trip>().GetByIdAsync(req.Id);

        _tenantUow.Repository<Trip>().Delete(trip);
        await _tenantUow.SaveChangesAsync();
        return Result.Ok();
    }
}
