using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Commands;

internal sealed class RemoveRoleFromUserHandler : RequestHandler<RemoveRoleFromUserCommand, Result>
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<User> _userManager;

    public RemoveRoleFromUserHandler(
        UserManager<User> userManager,
        RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public override async Task<Result> Handle(
        RemoveRoleFromUserCommand req, CancellationToken ct)
    {
        req.Role = req.Role?.ToLower();
        var user = await _userManager.FindByIdAsync(req.UserId!);

        if (user == null)
        {
            return Result.Fail("Could not find the specified user");
        }

        var appRole = await _roleManager.FindByNameAsync(req.Role!);

        if (appRole == null)
        {
            return Result.Fail("Could not find the specified role name");
        }

        await _userManager.RemoveFromRoleAsync(user, appRole.Name!);
        return Result.Succeed();
    }
}
