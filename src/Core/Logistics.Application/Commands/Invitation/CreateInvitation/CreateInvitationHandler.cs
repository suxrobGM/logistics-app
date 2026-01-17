using System.Security.Claims;
using System.Security.Cryptography;
using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Logistics.Application.Commands;

internal sealed class CreateInvitationHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    UserManager<User> userManager,
    IEmailSender emailSender,
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration)
    : IAppRequestHandler<CreateInvitationCommand, Result<InvitationDto>>
{
    public async Task<Result<InvitationDto>> Handle(CreateInvitationCommand req, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Result<InvitationDto>.Fail("User not authenticated.");
        }

        var currentUser = await userManager.FindByIdAsync(currentUserId.Value.ToString());
        if (currentUser is null)
        {
            return Result<InvitationDto>.Fail("Current user not found.");
        }

        var tenant = tenantUow.GetCurrentTenant();

        // Validate role assignment permissions - Owner can only be assigned by SuperAdmin/AppManager
        if (req.TenantRole == TenantRoles.Owner)
        {
            var currentUserRoles = await userManager.GetRolesAsync(currentUser);
            var canAssignOwner = currentUserRoles.Any(r => r == AppRoles.SuperAdmin || r == AppRoles.Manager);
            if (!canAssignOwner)
            {
                return Result<InvitationDto>.Fail("Only Super Admins and App Managers can invite Owner users.");
            }
        }

        // Check for existing pending invitation
        var existingInvitation = await masterUow.Repository<Invitation>()
            .GetAsync(i => i.Email == req.Email
                        && i.TenantId == tenant.Id
                        && i.Status == InvitationStatus.Pending, ct);

        if (existingInvitation is not null)
        {
            return Result<InvitationDto>.Fail("An active invitation already exists for this email.");
        }

        // Check if user already exists and is already a tenant member
        var existingUser = await masterUow.Repository<User>()
            .GetAsync(u => u.Email == req.Email, ct);

        if (existingUser is not null)
        {
            var isAlreadyMember = await CheckExistingMembershipAsync(existingUser.Id, req.Type, ct);
            if (isAlreadyMember)
            {
                return Result<InvitationDto>.Fail("This user is already a member of your organization.");
            }
        }

        // For CustomerUser, validate customer exists
        string? customerName = null;
        if (req.Type == InvitationType.CustomerUser)
        {
            if (!req.CustomerId.HasValue)
            {
                return Result<InvitationDto>.Fail("CustomerId is required for customer user invitations.");
            }

            var customer = await tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId.Value, ct);
            if (customer is null)
            {
                return Result<InvitationDto>.Fail("Customer not found.");
            }
            customerName = customer.Name;
        }

        var invitation = new Invitation
        {
            Email = req.Email,
            Token = GenerateSecureToken(),
            TenantId = tenant.Id,
            Type = req.Type,
            TenantRole = req.TenantRole,
            CustomerId = req.CustomerId,
            ExpiresAt = DateTime.UtcNow.AddDays(req.ExpirationDays),
            InvitedByUserId = currentUserId.Value,
            PersonalMessage = req.PersonalMessage,
            LastSentAt = DateTime.UtcNow
        };

        await masterUow.Repository<Invitation>().AddAsync(invitation, ct);
        await masterUow.SaveChangesAsync(ct);

        // Send invitation email
        await SendInvitationEmailAsync(invitation, tenant, currentUser.GetFullName());

        var dto = invitation.ToDto(
            tenant.CompanyName ?? tenant.Name,
            currentUser.GetFullName(),
            customerName);

        return Result<InvitationDto>.Ok(dto);
    }

    private async Task<bool> CheckExistingMembershipAsync(Guid userId, InvitationType type, CancellationToken ct)
    {
        if (type == InvitationType.Employee)
        {
            var employee = await tenantUow.Repository<Employee>().GetByIdAsync(userId, ct);
            return employee is not null;
        }
        else
        {
            var customerUser = await tenantUow.Repository<CustomerUser>()
                .GetAsync(cu => cu.UserId == userId, ct);
            return customerUser is not null;
        }
    }

    private static string GenerateSecureToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private async Task SendInvitationEmailAsync(Invitation invitation, Tenant tenant, string invitedByName)
    {
        var identityServerUrl = configuration["IdentityServer:Authority"];
        var acceptUrl = $"{identityServerUrl}/Account/AcceptInvitation?token={invitation.Token}";

        var subject = $"You're invited to join {tenant.CompanyName ?? tenant.Name}";
        var body = BuildInvitationEmailBody(invitation, tenant, acceptUrl, invitedByName);

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
                        <h1 style="color: #1a1a1a; font-size: 24px; margin: 0 0 24px 0;">You're Invited!</h1>

                        <p style="color: #4a4a4a; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;">
                            <strong>{invitedByName}</strong> has invited you to join <strong>{companyName}</strong> as a {typeLabel}.
                        </p>

                        <p style="color: #4a4a4a; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;">
                            <strong>Role:</strong> {roleDisplayName}
                        </p>

                        {(string.IsNullOrEmpty(invitation.PersonalMessage) ? "" : $"""
                        <div style="background-color: #f8f9fa; border-left: 4px solid #007bff; padding: 16px; margin: 16px 0; border-radius: 0 4px 4px 0;">
                            <p style="color: #4a4a4a; font-size: 14px; font-style: italic; margin: 0;">"{invitation.PersonalMessage}"</p>
                        </div>
                        """)}

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

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
