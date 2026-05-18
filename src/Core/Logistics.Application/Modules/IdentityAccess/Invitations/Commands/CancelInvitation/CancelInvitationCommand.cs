using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Invitations.Commands;

public class CancelInvitationCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
