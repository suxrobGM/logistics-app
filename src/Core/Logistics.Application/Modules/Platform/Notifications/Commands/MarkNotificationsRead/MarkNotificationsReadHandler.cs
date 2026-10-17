using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Notifications.Commands;

internal sealed class MarkNotificationsReadHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<MarkNotificationsReadCommand, Result>
{
    public async Task<Result> Handle(MarkNotificationsReadCommand req, CancellationToken ct)
    {
        var notifications = await tenantUow.Repository<Notification>()
            .GetListAsync(n => req.Ids.Contains(n.Id) && !n.IsRead, ct);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
