using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteTripHandler : RequestHandler<DeleteTripCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IPushNotificationService _pushNotificationService;

    public DeleteTripHandler(
        ITenantUnityOfWork tenantUow,
        IPushNotificationService pushNotificationService)
    {
        _tenantUow = tenantUow;
        _pushNotificationService = pushNotificationService;
    }

    protected override async Task<Result> HandleValidated(
        DeleteTripCommand req, CancellationToken cancellationToken)
    {
        var trip = await _tenantUow.Repository<Trip>().GetByIdAsync(req.Id);

        _tenantUow.Repository<Trip>().Delete(trip);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
