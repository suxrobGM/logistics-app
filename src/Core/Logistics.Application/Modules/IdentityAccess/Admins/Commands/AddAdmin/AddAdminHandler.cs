using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Common.Extensions;
using Logistics.Application.Common.Security;
using Logistics.Application.Modules.IdentityAccess.Invitations.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Modules.IdentityAccess.Admins.Commands;

internal sealed class AddAdminHandler(
    IMasterUnitOfWork masterUow,
    UserManager<User> userManager,
    ICurrentUserService currentUserService,
    IInvitationEmailSender invitationEmailSender)
    : IRequestHandler<AddAdminCommand, Result<AddAdminResult>>
{
    public async Task<Result<AddAdminResult>> Handle(AddAdminCommand req, CancellationToken ct)
    {
        var currentUserId = currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Result<AddAdminResult>.Fail("User not authenticated.");
        }

        // Existing user -> grant the Admin role immediately.
        var existingUser = await userManager.FindByEmailAsync(req.Email);
        if (existingUser is not null)
        {
            if (await userManager.IsInRoleAsync(existingUser, AppRoles.SuperAdmin))
            {
                return Result<AddAdminResult>.Fail("This user is already a super admin.");
            }

            if (await userManager.IsInRoleAsync(existingUser, AppRoles.Admin))
            {
                return Result<AddAdminResult>.Fail("This user is already an admin.");
            }

            var addResult = await userManager.AddToRoleAsync(existingUser, AppRoles.Admin);
            if (!addResult.Succeeded)
            {
                var errors = addResult.DescribeErrors();
                return Result<AddAdminResult>.Fail($"Failed to grant admin role: {errors}");
            }

            return Result<AddAdminResult>.Ok(new AddAdminResult { Invited = false, UserId = existingUser.Id });
        }

        // No user yet -> create a pending app-user invitation and email it.
        var existingInvitation = await masterUow.Repository<Invitation>()
            .GetAsync(i => i.Email == req.Email
                        && i.Type == InvitationType.AppUser
                        && i.Status == InvitationStatus.Pending, ct);

        if (existingInvitation is not null)
        {
            return Result<AddAdminResult>.Fail("An active admin invitation already exists for this email.");
        }

        var currentUser = await userManager.FindByIdAsync(currentUserId.Value.ToString());

        var token = TokenGenerator.GenerateSecureToken(64);
        var invitation = new Invitation
        {
            Email = req.Email,
            TokenHash = TokenHasher.Hash(token),
            TenantId = null,
            Type = InvitationType.AppUser,
            AppRole = AppRoles.Admin,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            InvitedByUserId = currentUserId.Value,
            PersonalMessage = req.PersonalMessage,
            LastSentAt = DateTime.UtcNow
        };

        await masterUow.Repository<Invitation>().AddAsync(invitation, ct);
        await masterUow.SaveChangesAsync(ct);

        await invitationEmailSender.SendAsync(invitation, token, currentUser?.GetFullName(),
            tenant: null, isReminder: false);

        return Result<AddAdminResult>.Ok(new AddAdminResult { Invited = true });
    }
}
