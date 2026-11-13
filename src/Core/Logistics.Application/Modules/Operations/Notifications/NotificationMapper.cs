using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Operations.Notifications;

[Mapper]
public static partial class NotificationMapper
{
    [MapperIgnoreTarget(nameof(NotificationDto.IsRead))]
    private static partial NotificationDto MapNotification(Notification entity);

    extension(Notification entity)
    {
        public NotificationDto ToDto(bool isRead = false)
        {
            var dto = MapNotification(entity);
            return dto with { IsRead = isRead };
        }
    }
}
