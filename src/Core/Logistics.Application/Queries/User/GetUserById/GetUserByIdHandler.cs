using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Queries;

internal sealed class GetUserByIdHandler : RequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly UserManager<User> _userManager;

    public GetUserByIdHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public override async Task<Result<UserDto>> Handle(
        GetUserByIdQuery req, CancellationToken ct)
    {
        var userEntity = await _userManager.FindByIdAsync(req.UserId);

        if (userEntity is null)
        {
            return Result<UserDto>.Fail($"Could not find an user with ID '{req.UserId}'");
        }

        var userRoles = await _userManager.GetRolesAsync(userEntity);

        var user = userEntity.ToDto(userRoles);
        return Result<UserDto>.Succeed(user);
    }
}
