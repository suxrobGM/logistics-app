using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class UpdateNotificationCommand : IAppRequest
{
    public Guid Id { get; set; }
    public bool? IsRead { get; set; }
}
