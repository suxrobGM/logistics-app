using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Common.Security;
using Logistics.Application.Modules.IdentityAccess.Invitations.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Modules.IdentityAccess.Invitations.Commands;

internal sealed class CreateInvitationHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    UserManager<User> userManager,
    ICurrentUserService currentUserService,
    IInvitationEmailSender invitationEmailSender)
    : IRequestHandler<CreateInvitationCommand, Result<InvitationDto>>
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
            return Result<InvitationDto>.Fail("Current user not found.", ErrorCodes.NotFound);
        }

        var tenant = tenantUow.GetCurrentTenant();

        // Validate role assignment permissions - Owner can only be assigned by SuperAdmin/Admin
        if (req.TenantRole == TenantRoles.Owner)
        {
            var currentUserRoles = await userManager.GetRolesAsync(currentUser);
            var canAssignOwner = currentUserRoles.Contains(AppRoles.SuperAdmin) ||
                currentUserRoles.Contains(AppRoles.Admin);

            if (!canAssignOwner)
            {
                return Result<InvitationDto>.Fail("Only Super Admins and Admins can invite Owner users.");
            }
        }

        var normalizedEmail = userManager.NormalizeEmail(req.Email);
        var existingInvitation = await masterUow.Repository<Invitation>()
            .GetAsync(i => i.Email.ToUpper() == normalizedEmail
                        && i.TenantId == tenant.Id
                        && i.Status == InvitationStatus.Pending, ct);

        if (existingInvitation is not null)
        {
            return Result<InvitationDto>.Fail("An active invitation already exists for this email.");
        }

        var existingUser = await masterUow.Repository<User>()
            .GetAsync(u => u.NormalizedEmail == normalizedEmail, ct);

        if (existingUser is not null)
        {
            // An imported driver is a member without a password; inviting them is how they get one.
            var isAlreadyMember = await CheckExistingMembershipAsync(existingUser.Id, req.Type, ct);
            if (isAlreadyMember && await userManager.HasPasswordAsync(existingUser))
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
                return Result<InvitationDto>.Fail("Customer not found.", ErrorCodes.NotFound);
            }
            customerName = customer.Name;
        }

        var token = TokenGenerator.GenerateSecureToken(64);
        var invitation = new Invitation
        {
            Email = req.Email,
            TokenHash = TokenHasher.Hash(token),
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
        await invitationEmailSender.SendAsync(invitation, token, currentUser.GetFullName(), tenant, isReminder: false);

        var dto = invitation.ToDto(
            tenant.DisplayName,
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
}
