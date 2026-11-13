using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.IdentityAccess.Invitations.Services;

public interface IInvitationEmailSender : IApplicationService
{
    /// <summary><paramref name="tenant" /> is null for a platform admin; a reminder omits the personal message.</summary>
    Task SendAsync(Invitation invitation, string token, string? invitedByName, Tenant? tenant, bool isReminder);
}
