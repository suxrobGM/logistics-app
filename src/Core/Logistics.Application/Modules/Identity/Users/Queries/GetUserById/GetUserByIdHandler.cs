using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Modules.Identity.Users.Queries;

internal sealed class GetUserByIdHandler(UserManager<User> userManager)
    : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(
        GetUserByIdQuery req, CancellationToken ct)
    {
        var userEntity = await userManager.FindByIdAsync(req.UserId);

        if (userEntity is null)
        {
            return Result<UserDto>.Fail($"Could not find an user with ID '{req.UserId}'", ErrorCodes.NotFound);
        }

        var user = userEntity.ToDto();
        return Result<UserDto>.Ok(user);
    }
}
