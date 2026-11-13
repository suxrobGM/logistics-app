using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Modules.Identity.Admins.Commands;

/// <summary>
/// Resends a pending app-level (Admin) invitation email.
/// </summary>
public class ResendAdminInvitationCommand : ICommand
{
    public Guid Id { get; set; }
}
