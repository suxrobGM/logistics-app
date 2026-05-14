using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class UpdateNotificationCommand : ICommand
{
    public Guid Id { get; set; }
    public bool? IsRead { get; set; }
}
