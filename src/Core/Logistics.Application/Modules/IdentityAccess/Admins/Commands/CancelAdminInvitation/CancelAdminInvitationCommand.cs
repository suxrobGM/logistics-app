using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Modules.IdentityAccess.Admins.Commands;

/// <summary>
/// Cancels a pending app-level (Admin) invitation.
/// </summary>
public class CancelAdminInvitationCommand : ICommand
{
    public Guid Id { get; set; }
}
