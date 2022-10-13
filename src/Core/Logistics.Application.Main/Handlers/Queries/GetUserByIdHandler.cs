using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Main.Handlers.Queries;

internal sealed class GetUserByIdHandler : RequestHandlerBase<GetUserByIdQuery, ResponseResult<UserDto>>
{
    private readonly UserManager<User> _userManager;

    public GetUserByIdHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task<ResponseResult<UserDto>> HandleValidated(
        GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userEntity = await _userManager.FindByIdAsync(request.Id!);

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

    protected override bool Validate(GetUserByIdQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
