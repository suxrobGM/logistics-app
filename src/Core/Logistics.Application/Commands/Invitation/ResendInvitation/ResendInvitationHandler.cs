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

        var subject = $"Reminder: You're invited to join {tenant.CompanyName ?? tenant.Name}";
        var body = BuildInvitationEmailBody(invitation, tenant, acceptUrl, invitation.InvitedByUser?.GetFullName() ?? "A team member");

        await emailSender.SendEmailAsync(invitation.Email, subject, body);
    }

    private static string BuildInvitationEmailBody(Invitation invitation, Tenant tenant, string acceptUrl, string invitedByName)
    {
        var roleDisplayName = InvitationMapper.GetRoleDisplayName(invitation.TenantRole);
        var typeLabel = invitation.Type == InvitationType.Employee ? "team member" : "customer portal user";
        var companyName = tenant.CompanyName ?? tenant.Name;

        return $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
            </head>
            <body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; margin: 0; padding: 0; background-color: #f5f5f5;">
                <div style="max-width: 600px; margin: 0 auto; padding: 40px 20px;">
                    <div style="background-color: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); padding: 40px;">
                        <h1 style="color: #1a1a1a; font-size: 24px; margin: 0 0 24px 0;">Reminder: You're Invited!</h1>

                        <p style="color: #4a4a4a; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;">
                            This is a reminder that <strong>{invitedByName}</strong> has invited you to join <strong>{companyName}</strong> as a {typeLabel}.
                        </p>

                        <p style="color: #4a4a4a; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;">
                            <strong>Role:</strong> {roleDisplayName}
                        </p>

                        <div style="text-align: center; margin: 32px 0;">
                            <a href="{acceptUrl}" style="display: inline-block; background-color: #007bff; color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-size: 16px; font-weight: 500;">
                                Accept Invitation
                            </a>
                        </div>

                        <p style="color: #6c757d; font-size: 14px; line-height: 1.6; margin: 16px 0 0 0;">
                            This invitation expires on {invitation.ExpiresAt:MMMM dd, yyyy}.
                        </p>

                        <hr style="border: none; border-top: 1px solid #e9ecef; margin: 24px 0;">

                        <p style="color: #6c757d; font-size: 12px; line-height: 1.6; margin: 0;">
                            If you did not expect this invitation, you can safely ignore this email.
                        </p>
                    </div>
                </div>
            </body>
            </html>
            """;
    }
}
