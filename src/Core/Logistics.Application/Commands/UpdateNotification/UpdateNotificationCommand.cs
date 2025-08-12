using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class UpdateNotificationCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public bool? IsRead { get; set; }
}
