using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.Invitations.Commands;

public class ResendInvitationCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
