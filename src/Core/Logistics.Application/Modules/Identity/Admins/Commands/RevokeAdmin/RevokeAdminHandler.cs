using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Mediator;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Modules.Identity.Admins.Commands;

internal sealed class RevokeAdminHandler(UserManager<User> userManager)
    : IRequestHandler<RevokeAdminCommand, Result>
{
    public async Task<Result> Handle(RevokeAdminCommand req, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(req.UserId.ToString());
        if (user is null)
        {
            return Result.Fail("User not found.", ErrorCodes.NotFound);
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
            var errors = result.DescribeErrors();
            return Result.Fail($"Failed to revoke admin role: {errors}");
        }

        return Result.Ok();
    }
}
