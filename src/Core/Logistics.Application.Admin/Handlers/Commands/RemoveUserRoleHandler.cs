using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Admin.Handlers.Commands;

public class RemoveUserRoleHandler : RequestHandlerBase<RemoveUserRoleCommand, ResponseResult>
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

    protected override bool Validate(
        RemoveUserRoleCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.UserId))
        {
            errorDescription = "UserId is an empty string";
        }
        else if (string.IsNullOrEmpty(request.Role))
        {
            errorDescription = "Role name is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}