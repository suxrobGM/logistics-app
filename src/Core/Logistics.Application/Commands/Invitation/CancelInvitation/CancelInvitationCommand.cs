using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class CancelInvitationCommand : IAppRequest<Result>
{
    public Guid Id { get; set; }
}
