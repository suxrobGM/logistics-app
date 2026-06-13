using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.IdentityAccess.Invitations.Queries;

internal sealed class ValidateInvitationTokenHandler(
    IMasterUnitOfWork masterUow,
    UserManager<User> userManager)
    : IAppRequestHandler<ValidateInvitationTokenQuery, Result<InvitationValidationResult>>
{
    public async Task<Result<InvitationValidationResult>> Handle(ValidateInvitationTokenQuery req, CancellationToken ct)
    {
        var invitation = await masterUow.Repository<Invitation>()
            .Query()
            .FirstOrDefaultAsync(i => i.Token == req.Token, ct);

        if (invitation is null)
        {
            return Result<InvitationValidationResult>.Ok(new InvitationValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid invitation link."
            });
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            var statusMessage = invitation.Status switch
            {
                InvitationStatus.Accepted => "This invitation has already been accepted.",
                InvitationStatus.Expired => "This invitation has expired.",
                InvitationStatus.Cancelled => "This invitation has been cancelled.",
                _ => "This invitation is no longer valid."
            };

            return Result<InvitationValidationResult>.Ok(new InvitationValidationResult
            {
                IsValid = false,
                ErrorMessage = statusMessage
            });
        }

        if (invitation.IsExpired)
        {
            return Result<InvitationValidationResult>.Ok(new InvitationValidationResult
            {
                IsValid = false,
                ErrorMessage = "This invitation has expired."
            });
        }

        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(invitation.Email);
        var isAppUser = invitation.Type == InvitationType.AppUser;

        return Result<InvitationValidationResult>.Ok(new InvitationValidationResult
        {
            IsValid = true,
            Email = invitation.Email,
            TenantName = isAppUser
                ? PlatformConstants.PlatformName
                : invitation.Tenant?.CompanyName ?? invitation.Tenant?.Name ?? string.Empty,
            RoleDisplayName = isAppUser
                ? InvitationMapper.GetAppRoleDisplayName(invitation.AppRole)
                : InvitationMapper.GetRoleDisplayName(invitation.TenantRole),
            UserExists = existingUser is not null
        });
    }
}
