using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Modules.IdentityAccess.Admins.Commands;

internal sealed class RevokeAdminHandler(UserManager<User> userManager)
    : IAppRequestHandler<RevokeAdminCommand, Result>
{
    public async Task<Result> Handle(RevokeAdminCommand req, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(req.UserId.ToString());
        if (user is null)
        {
            return Result.Fail("User not found.");
        }

        if (await userManager.IsInRoleAsync(user, AppRoles.SuperAdmin))
        {
            return Result.Fail("Cannot revoke admin access from a super admin.");
        }

        if (!await userManager.IsInRoleAsync(user, AppRoles.Admin))
        {
            return Result.Fail("This user is not an admin.");
        }

        var result = await userManager.RemoveFromRoleAsync(user, AppRoles.Admin);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Fail($"Failed to revoke admin role: {errors}");
        }

        return Result.Ok();
    }
}
