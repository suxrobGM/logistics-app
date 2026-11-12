using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Domain.Entities;
using Logistics.Application.Abstractions.Options;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.IdentityAccess.Invitations.Services;

internal sealed class InvitationEmailSender(
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IOptions<IdentityServerOptions> identityServerOptions) : IInvitationEmailSender
{
    public async Task SendAsync(
        Invitation invitation, string token, string invitedByName, Tenant? tenant, bool isReminder)
    {
        var isAdmin = invitation.Type == InvitationType.AppUser;
        var companyName = tenant?.DisplayName ?? PlatformConstants.PlatformName;

        var model = new InvitationEmailModel
        {
            InvitedByName = invitedByName,
            CompanyName = companyName,
            TypeLabel = TypeLabel(invitation.Type),
            RoleDisplayName = isAdmin
                ? InvitationMapper.GetAppRoleDisplayName(invitation.AppRole)
                : InvitationMapper.GetRoleDisplayName(invitation.TenantRole),
            PersonalMessage = isReminder ? null : invitation.PersonalMessage,
            AcceptUrl = $"{identityServerOptions.Value.UserFacingAuthority}/Account/AcceptInvitation?token={token}",
            ExpiresAt = invitation.ExpiresAt.ToString("MMMM dd, yyyy")
        };

        var subject = $"You're invited to join {companyName}";
        if (isReminder)
        {
            subject = "Reminder: " + subject;
        }
        else if (isAdmin)
        {
            subject += " as an administrator";
        }

        var body = await emailTemplateService.RenderAsync(isReminder ? "InvitationReminder" : "Invitation", model);
        await emailSender.SendEmailAsync(invitation.Email, subject, body);
    }

    private static string TypeLabel(InvitationType type) => type switch
    {
        InvitationType.AppUser => "administrator",
        InvitationType.Employee => "team member",
        _ => "customer portal user"
    };
}
