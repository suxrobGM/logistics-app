using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Modules.Identity.Admins.Commands;

/// <summary>
/// Cancels a pending app-level (Admin) invitation.
/// </summary>
public class CancelAdminInvitationCommand : ICommand
{
    public Guid Id { get; set; }
}
