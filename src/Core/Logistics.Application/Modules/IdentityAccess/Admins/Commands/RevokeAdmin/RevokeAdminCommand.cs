using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Modules.IdentityAccess.Admins.Commands;

/// <summary>
/// Revokes the app-level Admin role from a user.
/// </summary>
public class RevokeAdminCommand : ICommand
{
    public Guid UserId { get; set; }
}
