using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.IdentityAccess.Invitations.Services;

public interface IInvitationEmailSender : IApplicationService
{
    /// <summary>
    ///     Emails the accept link for <paramref name="token" />. <paramref name="tenant" /> is the organisation joined,
    ///     null for a platform admin. A reminder uses the reminder template and leaves out the personal message,
    ///     which was in the first email.
    /// </summary>
    Task SendAsync(Invitation invitation, string token, string invitedByName, Tenant? tenant, bool isReminder);
}
