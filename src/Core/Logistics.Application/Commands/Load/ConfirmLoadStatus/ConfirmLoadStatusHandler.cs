using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class ConfirmLoadStatusHandler : RequestHandler<ConfirmLoadStatusCommand, Result>
{
    private readonly INotificationService _notificationService;
    private readonly ITenantUnityOfWork _tenantUow;

    public ConfirmLoadStatusHandler(
        ITenantUnityOfWork tenantUow,
        INotificationService notificationService)
    {
        _tenantUow = tenantUow;
        _notificationService = notificationService;
    }

    protected override async Task<Result> HandleValidated(
        ConfirmLoadStatusCommand req, CancellationToken ct)
    {
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.LoadId);

        if (load is null) return Result.Fail($"Could not find load with ID '{req.LoadId}'");

        var loadStatus = req.LoadStatus!.Value;
        load.SetStatus(loadStatus);

        _tenantUow.Repository<Load>().Update(load);
        var changes = await _tenantUow.SaveChangesAsync();

        if (changes > 0) await SendNotificationAsync(load);

        return Result.Succeed();
    }

    private async Task SendNotificationAsync(Load load)
    {
        const string title = "Load updates";
        var driverName = load.AssignedTruck?.MainDriver?.GetFullName();
        var message = $"Driver {driverName} confirmed the load #{load.Number} status to '{load.Status}'";
        await _notificationService.SendNotificationAsync(title, message);
    }
}
