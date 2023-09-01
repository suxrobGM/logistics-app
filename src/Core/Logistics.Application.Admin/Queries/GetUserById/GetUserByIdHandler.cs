using Logistics.Models;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetUserByIdHandler : RequestHandler<GetUserByIdQuery, ResponseResult<UserDto>>
{
    private readonly UserManager<User> _userManager;

    public GetUserByIdHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task<ResponseResult<UserDto>> HandleValidated(
        GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userEntity = await _userManager.FindByIdAsync(request.UserId!);

        if (userEntity == null)
            return ResponseResult<UserDto>.CreateError("Could not find the specified user");

        var userRoles = await _userManager.GetRolesAsync(userEntity);
        
        var user = new UserDto
        {
            Id = userEntity.Id,
            UserName = userEntity.UserName!,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            Roles = userRoles,
            Email = userEntity.Email,
            PhoneNumber = userEntity.PhoneNumber
        };

        return ResponseResult<UserDto>.CreateSuccess(user);
    }
}
