using Logistics.Application.Common;
using Logistics.Domain.Entities;
using Logistics.Shared;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Admin.Commands;

internal sealed class RemoveRoleFromUserHandler : RequestHandler<RemoveRoleFromUserCommand, ResponseResult>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public RemoveRoleFromUserHandler(
        UserManager<User> userManager,
        RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    
    protected override async Task<ResponseResult> HandleValidated(
        RemoveRoleFromUserCommand req, CancellationToken cancellationToken)
    {
        req.Role = req.Role?.ToLower();
        var user = await _userManager.FindByIdAsync(req.UserId!);

        if (user == null)
            return ResponseResult.CreateError("Could not find the specified user");

        var appRole = await _roleManager.FindByNameAsync(req.Role!);
        
        if (appRole == null)
            return ResponseResult.CreateError("Could not find the specified role name");

        await _userManager.RemoveFromRoleAsync(user, appRole.Name!);
        return ResponseResult.CreateSuccess();
    }
}
