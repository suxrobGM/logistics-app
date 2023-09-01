using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Admin.Commands;

internal sealed class RemoveUserRoleHandler : RequestHandler<RemoveUserRoleCommand, ResponseResult>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public RemoveUserRoleHandler(
        UserManager<User> userManager,
        RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    
    protected override async Task<ResponseResult> HandleValidated(
        RemoveUserRoleCommand request, CancellationToken cancellationToken)
    {
        request.Role = request.Role?.ToLower();
        var user = await _userManager.FindByIdAsync(request.UserId!);

        if (user == null)
            return ResponseResult.CreateError("Could not find the specified user");

        var appRole = await _roleManager.FindByNameAsync(request.Role!);
        
        if (appRole == null)
            return ResponseResult.CreateError("Could not find the specified role name");

        await _userManager.RemoveFromRoleAsync(user, appRole.Name!);
        return ResponseResult.CreateSuccess();
    }
}