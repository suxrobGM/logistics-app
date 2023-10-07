using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Mappers;

public static class NotificationMapper
{
    public static NotificationDto ToDto(this Notification entity)
    {
        return new NotificationDto()
        {
            Id = entity.Id,
            Title = entity.Title,
            Message = entity.Message,
            IsRead = entity.IsRead
        };
    }
}
