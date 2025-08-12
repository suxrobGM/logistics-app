using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteTripHandler : RequestHandler<DeleteTripCommand, Result>
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

    protected override async Task<Result> HandleValidated(
        DeleteTripCommand req, CancellationToken ct)
    {
        var trip = await _tenantUow.Repository<Trip>().GetByIdAsync(req.Id);

        _tenantUow.Repository<Trip>().Delete(trip);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
