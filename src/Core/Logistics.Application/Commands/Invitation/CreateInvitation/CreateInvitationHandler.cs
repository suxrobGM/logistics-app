using Logistics.Application.Abstractions;
using Logistics.Application.Contracts.Models.Email;
using Logistics.Application.Contracts.Services.Email;
using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Commands;

internal sealed class CreateInvitationHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    UserManager<User> userManager,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    ICurrentUserService currentUserService,
    IOptions<IdentityServerOptions> identityServerOptions)
    : IAppRequestHandler<CreateInvitationCommand, Result<InvitationDto>>
{
    public async Task<Result<InvitationDto>> Handle(CreateInvitationCommand req, CancellationToken ct)
    {
        var currentUserId = currentUserService.GetUserId();
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
            var canAssignOwner = currentUserRoles.Contains(AppRoles.SuperAdmin) ||
                currentUserRoles.Contains(AppRoles.Admin) ||
                currentUserRoles.Contains(AppRoles.Manager);

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
            Token = TokenGenerator.GenerateSecureToken(64),
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

    private async Task SendInvitationEmailAsync(Invitation invitation, Tenant tenant, string invitedByName)
    {
        var acceptUrl = $"{identityServerOptions.Value.Authority}/Account/AcceptInvitation?token={invitation.Token}";
        var companyName = tenant.CompanyName ?? tenant.Name;

        var model = new InvitationEmailModel
        {
            InvitedByName = invitedByName,
            CompanyName = companyName,
            TypeLabel = invitation.Type == InvitationType.Employee ? "team member" : "customer portal user",
            RoleDisplayName = InvitationMapper.GetRoleDisplayName(invitation.TenantRole),
            PersonalMessage = invitation.PersonalMessage,
            AcceptUrl = acceptUrl,
            ExpiresAt = invitation.ExpiresAt.ToString("MMMM dd, yyyy")
        };

        var subject = $"You're invited to join {companyName}";
        var body = await emailTemplateService.RenderAsync("Invitation", model);

        await emailSender.SendEmailAsync(invitation.Email, subject, body);
    }
}
