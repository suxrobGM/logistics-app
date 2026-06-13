using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.IdentityAccess.Admins.Commands;

internal sealed class ResendAdminInvitationHandler(
    IMasterUnitOfWork masterUow,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IOptions<IdentityServerOptions> identityServerOptions)
    : IAppRequestHandler<ResendAdminInvitationCommand, Result>
{
    public async Task<Result> Handle(ResendAdminInvitationCommand req, CancellationToken ct)
    {
        var invitation = await masterUow.Repository<Invitation>()
            .Query()
            .FirstOrDefaultAsync(i => i.Id == req.Id && i.Type == InvitationType.AppUser, ct);

        if (invitation is null)
        {
            return Result.Fail("Invitation not found.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return Result.Fail("Only pending invitations can be resent.");
        }

        if (invitation.IsExpired)
        {
            return Result.Fail("Cannot resend an expired invitation. Please create a new invitation.");
        }

        invitation.SendCount++;
        invitation.LastSentAt = DateTime.UtcNow;

        masterUow.Repository<Invitation>().Update(invitation);
        await masterUow.SaveChangesAsync(ct);

        var acceptUrl = $"{identityServerOptions.Value.UserFacingAuthority}/Account/AcceptInvitation?token={invitation.Token}";

        var model = new InvitationEmailModel
        {
            InvitedByName = invitation.InvitedByUser?.GetFullName() ?? "An administrator",
            CompanyName = PlatformConstants.PlatformName,
            TypeLabel = "administrator",
            RoleDisplayName = InvitationMapper.GetAppRoleDisplayName(invitation.AppRole),
            AcceptUrl = acceptUrl,
            ExpiresAt = invitation.ExpiresAt.ToString("MMMM dd, yyyy")
        };

        var body = await emailTemplateService.RenderAsync("InvitationReminder", model);
        await emailSender.SendEmailAsync(
            invitation.Email,
            $"Reminder: You're invited to join {PlatformConstants.PlatformName}",
            body);

        return Result.Ok();
    }
}
