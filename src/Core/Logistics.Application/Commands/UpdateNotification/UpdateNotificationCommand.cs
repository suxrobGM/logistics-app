using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateNotificationCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
    public bool? IsRead { get; set; }
}
