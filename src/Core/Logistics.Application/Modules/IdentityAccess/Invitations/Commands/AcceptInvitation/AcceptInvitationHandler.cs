using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Logistics.Application.Abstractions.Notifications;

namespace Logistics.Application.Modules.IdentityAccess.Invitations.Commands;

internal sealed class AcceptInvitationHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    UserManager<User> userManager,
    INotificationService notificationService)
    : IAppRequestHandler<AcceptInvitationCommand, Result<AcceptInvitationResult>>
{
    public async Task<Result<AcceptInvitationResult>> Handle(AcceptInvitationCommand req, CancellationToken ct)
    {
        // Find and validate invitation
        var invitation = await masterUow.Repository<Invitation>()
            .GetAsync(i => i.Token == req.Token, ct);

        if (invitation is null)
        {
            return Result<AcceptInvitationResult>.Fail("Invalid or expired invitation.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return Result<AcceptInvitationResult>.Fail("This invitation has already been used or cancelled.");
        }

        if (invitation.IsExpired)
        {
            invitation.Status = InvitationStatus.Expired;
            masterUow.Repository<Invitation>().Update(invitation);
            await masterUow.SaveChangesAsync(ct);
            return Result<AcceptInvitationResult>.Fail("This invitation has expired.");
        }

        // App-level invitations (e.g. Admin) are not tenant-scoped: create the user
        // without a tenant, assign the app role, and finish before any tenant DB switch.
        if (invitation.Type == InvitationType.AppUser)
        {
            return await AcceptAppUserInvitationAsync(req, invitation, ct);
        }

        var tenant = invitation.Tenant;
        if (tenant is null)
        {
            return Result<AcceptInvitationResult>.Fail("Organization not found.");
        }

        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(invitation.Email);
        User user;

        if (existingUser is not null)
        {
            user = existingUser;
            // Update user's tenant if not set
            if (user.TenantId is null)
            {
                user.TenantId = invitation.TenantId;
                await userManager.UpdateAsync(user);
            }
        }
        else
        {
            // Create new user
            user = new User
            {
                UserName = invitation.Email,
                Email = invitation.Email,
                EmailConfirmed = true, // Pre-confirmed via invitation
                FirstName = req.FirstName,
                LastName = req.LastName,
                TenantId = invitation.TenantId
            };

            var createResult = await userManager.CreateAsync(user, req.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return Result<AcceptInvitationResult>.Fail($"Failed to create user: {errors}");
            }
        }

        // Switch to tenant database
        await tenantUow.SetCurrentTenantByIdAsync(tenant.Id);

        // Create Employee or CustomerUser based on invitation type
        if (invitation.Type == InvitationType.Employee)
        {
            var result = await CreateEmployeeAsync(user, invitation, ct);
            if (!result.IsSuccess)
            {
                return Result<AcceptInvitationResult>.Fail(result.Error ?? "Failed to create employee.");
            }
        }
        else
        {
            var result = await CreateCustomerUserAsync(user, invitation, ct);
            if (!result.IsSuccess)
            {
                return Result<AcceptInvitationResult>.Fail(result.Error ?? "Failed to create customer user.");
            }
        }

        // Update invitation status
        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;
        invitation.AcceptedByUserId = user.Id;

        masterUow.Repository<Invitation>().Update(invitation);
        await masterUow.SaveChangesAsync(ct);
        await tenantUow.SaveChangesAsync(ct);

        // Send notification
        var roleDisplayName = InvitationMapper.GetRoleDisplayName(invitation.TenantRole);
        await notificationService.SendNotificationAsync(
            "New Team Member",
            $"{user.GetFullName()} has joined as {roleDisplayName}");

        return Result<AcceptInvitationResult>.Ok(new AcceptInvitationResult
        {
            UserId = user.Id,
            Email = user.Email!,
            TenantName = tenant.CompanyName ?? tenant.Name,
            RoleDisplayName = roleDisplayName
        });
    }

    private async Task<Result<AcceptInvitationResult>> AcceptAppUserInvitationAsync(
        AcceptInvitationCommand req, Invitation invitation, CancellationToken ct)
    {
        var appRole = invitation.AppRole ?? AppRoles.Admin;

        var user = await userManager.FindByEmailAsync(invitation.Email);
        if (user is null)
        {
            user = new User
            {
                UserName = invitation.Email,
                Email = invitation.Email,
                EmailConfirmed = true, // Pre-confirmed via invitation
                FirstName = req.FirstName,
                LastName = req.LastName
            };

            var createResult = await userManager.CreateAsync(user, req.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return Result<AcceptInvitationResult>.Fail($"Failed to create user: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, appRole))
        {
            await userManager.AddToRoleAsync(user, appRole);
        }

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;
        invitation.AcceptedByUserId = user.Id;

        masterUow.Repository<Invitation>().Update(invitation);
        await masterUow.SaveChangesAsync(ct);

        var roleDisplayName = InvitationMapper.GetAppRoleDisplayName(appRole);
        await notificationService.SendNotificationAsync(
            "New Administrator",
            $"{user.GetFullName()} has joined as {roleDisplayName}");

        return Result<AcceptInvitationResult>.Ok(new AcceptInvitationResult
        {
            UserId = user.Id,
            Email = user.Email!,
            TenantName = PlatformConstants.PlatformName,
            RoleDisplayName = roleDisplayName
        });
    }

    private async Task<Result> CreateEmployeeAsync(User user, Invitation invitation, CancellationToken ct)
    {
        // Check if employee already exists
        var existingEmployee = await tenantUow.Repository<Employee>().GetByIdAsync(user.Id, ct);
        if (existingEmployee is not null)
        {
            return Result.Fail("User is already an employee of this organization.");
        }

        var tenantRole = await tenantUow.Repository<TenantRole>()
            .GetAsync(r => r.Name == invitation.TenantRole, ct);

        var employee = Employee.CreateEmployeeFromUser(user);
        if (tenantRole is not null)
        {
            employee.Role = tenantRole;
        }

        await tenantUow.Repository<Employee>().AddAsync(employee, ct);
        return Result.Ok();
    }

    private async Task<Result> CreateCustomerUserAsync(User user, Invitation invitation, CancellationToken ct)
    {
        if (!invitation.CustomerId.HasValue)
        {
            return Result.Fail("Customer ID is missing from the invitation.");
        }

        // Check if customer user already exists
        var existingCustomerUser = await tenantUow.Repository<CustomerUser>()
            .GetAsync(cu => cu.UserId == user.Id && cu.CustomerId == invitation.CustomerId.Value, ct);

        if (existingCustomerUser is not null)
        {
            return Result.Fail("User is already associated with this customer.");
        }

        var customer = await tenantUow.Repository<Customer>().GetByIdAsync(invitation.CustomerId.Value, ct);
        if (customer is null)
        {
            return Result.Fail("Customer not found.");
        }

        var customerUser = new CustomerUser
        {
            UserId = user.Id,
            CustomerId = invitation.CustomerId.Value,
            Email = user.Email!,
            DisplayName = user.GetFullName(),
            IsActive = true
        };

        await tenantUow.Repository<CustomerUser>().AddAsync(customerUser, ct);

        // Also add UserTenantAccess entry for portal tenant selection
        var tenantAccess = new UserTenantAccess
        {
            UserId = user.Id,
            TenantId = invitation.TenantId!.Value,
            CustomerUserId = customerUser.Id,
            CustomerName = customer.Name,
            IsActive = true
        };

        await masterUow.Repository<UserTenantAccess>().AddAsync(tenantAccess, ct);

        return Result.Ok();
    }
}
