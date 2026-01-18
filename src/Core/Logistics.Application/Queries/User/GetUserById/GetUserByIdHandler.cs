using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Queries;

internal sealed class GetUserByIdHandler(UserManager<User> userManager)
    : IAppRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(
        GetUserByIdQuery req, CancellationToken ct)
    {
        var userEntity = await userManager.FindByIdAsync(req.UserId);

        if (userEntity is null)
        {
            return Result<UserDto>.Fail($"Could not find an user with ID '{req.UserId}'");
        }

        var user = userEntity.ToDto();
        return Result<UserDto>.Ok(user);
    }
}
