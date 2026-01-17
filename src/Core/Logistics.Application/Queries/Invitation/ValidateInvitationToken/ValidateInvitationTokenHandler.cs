using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

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
            // Update status to expired
            invitation.Status = InvitationStatus.Expired;
            masterUow.Repository<Invitation>().Update(invitation);
            await masterUow.SaveChangesAsync(ct);

            return Result<InvitationValidationResult>.Ok(new InvitationValidationResult
            {
                IsValid = false,
                ErrorMessage = "This invitation has expired."
            });
        }

        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(invitation.Email);

        return Result<InvitationValidationResult>.Ok(new InvitationValidationResult
        {
            IsValid = true,
            Email = invitation.Email,
            TenantName = invitation.Tenant?.CompanyName ?? invitation.Tenant?.Name ?? string.Empty,
            RoleDisplayName = InvitationMapper.GetRoleDisplayName(invitation.TenantRole),
            UserExists = existingUser is not null
        });
    }
}
