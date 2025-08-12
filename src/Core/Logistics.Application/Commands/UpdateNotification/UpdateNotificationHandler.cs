using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateNotificationHandler : RequestHandler<UpdateNotificationCommand, Result>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public UpdateNotificationHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        UpdateNotificationCommand req, CancellationToken ct)
    {
        var notification = await _tenantUow.Repository<Notification>().GetByIdAsync(req.Id);

        if (notification is null)
        {
            return Result.Fail($"Could not find a notification with ID '{req.Id}'");
        }

        if (req.IsRead.HasValue && notification.IsRead != req.IsRead)
        {
            notification.IsRead = req.IsRead.Value;
        }

        _tenantUow.Repository<Notification>().Update(notification);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
