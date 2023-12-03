using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared;
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
        GetUserByIdQuery req, CancellationToken cancellationToken)
    {
        var userEntity = await _userManager.FindByIdAsync(req.UserId);

        if (userEntity is null)
        {
            return ResponseResult<UserDto>.CreateError($"Could not find an user with ID '{req.UserId}'");
        }

        var userRoles = await _userManager.GetRolesAsync(userEntity);
        
        var user = userEntity.ToDto(userRoles);
        return ResponseResult<UserDto>.CreateSuccess(user);
    }
}
