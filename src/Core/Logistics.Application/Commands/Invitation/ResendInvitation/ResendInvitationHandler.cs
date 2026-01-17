using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Logistics.Application.Commands;

internal sealed class ResendInvitationHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    IConfiguration configuration)
    : IAppRequestHandler<ResendInvitationCommand, Result>
{
    public async Task<Result> Handle(ResendInvitationCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        var invitation = await masterUow.Repository<Invitation>()
            .Query()
            .FirstOrDefaultAsync(i => i.Id == req.Id && i.TenantId == tenant.Id, ct);

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

        // Update send count and last sent time
        invitation.SendCount++;
        invitation.LastSentAt = DateTime.UtcNow;

        masterUow.Repository<Invitation>().Update(invitation);
        await masterUow.SaveChangesAsync(ct);

        // Resend the email
        await SendInvitationEmailAsync(invitation, tenant);

        return Result.Ok();
    }

    private async Task SendInvitationEmailAsync(Invitation invitation, Tenant tenant)
    {
        var identityServerUrl = configuration["IdentityServer:Authority"];
        var acceptUrl = $"{identityServerUrl}/Account/AcceptInvitation?token={invitation.Token}";
        var companyName = tenant.CompanyName ?? tenant.Name;

        var model = new InvitationEmailModel
        {
            InvitedByName = invitation.InvitedByUser?.GetFullName() ?? "A team member",
            CompanyName = companyName,
            TypeLabel = invitation.Type == InvitationType.Employee ? "team member" : "customer portal user",
            RoleDisplayName = InvitationMapper.GetRoleDisplayName(invitation.TenantRole),
            AcceptUrl = acceptUrl,
            ExpiresAt = invitation.ExpiresAt.ToString("MMMM dd, yyyy")
        };

        var subject = $"Reminder: You're invited to join {companyName}";
        var body = await emailTemplateService.RenderAsync("InvitationReminder", model);

        await emailSender.SendEmailAsync(invitation.Email, subject, body);
    }
}
