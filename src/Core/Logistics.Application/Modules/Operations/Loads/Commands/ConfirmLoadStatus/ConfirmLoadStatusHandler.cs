using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Notifications;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class ConfirmLoadStatusHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser,
    INotificationService notificationService)
    : IAppRequestHandler<ConfirmLoadStatusCommand, Result>
{
    public async Task<Result> Handle(ConfirmLoadStatusCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);

        if (load is null)
        {
            return Result.Fail($"Could not find load with ID '{req.LoadId}'");
        }

        if (!currentUser.CanDrive(load.AssignedTruck))
        {
            return Result.Fail("This load isn't assigned to your truck.");
        }

        var loadStatus = req.LoadStatus!.Value;

        if (loadStatus != load.NextDriverStatus)
        {
            return Result.Fail(
                $"This load is {load.Status.GetDescription()}, so it can't be marked {loadStatus.GetDescription()}.");
        }

        load.UpdateStatus(loadStatus);

        var changes = await tenantUow.SaveChangesAsync(ct);

        if (changes > 0)
        {
            await SendNotificationAsync(load);
        }

        return Result.Ok();
    }

    private async Task SendNotificationAsync(Load load)
    {
        const string title = "Load updates";
        var driverName = load.AssignedTruck?.MainDriver?.GetFullName();
        var message = $"Driver {driverName} confirmed the load #{load.Number} status to '{load.Status}'";
        await notificationService.SendNotificationAsync(title, message);
    }
}
