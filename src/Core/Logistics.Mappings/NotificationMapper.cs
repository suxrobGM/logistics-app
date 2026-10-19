using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class NotificationMapper
{
    public static NotificationDto ToDto(this Notification entity, bool isRead = false)
    {
        return new NotificationDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Message = entity.Message,
            IsRead = isRead,
            CreatedDate = entity.CreatedDate
        };
    }
}
