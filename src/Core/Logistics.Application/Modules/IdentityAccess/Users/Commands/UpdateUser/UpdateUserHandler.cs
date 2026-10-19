using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Users.Commands;

internal sealed class UpdateUserHandler(IUserService userService) : IRequestHandler<UpdateUserCommand, Result>
{
    public async Task<Result> Handle(UpdateUserCommand req, CancellationToken ct)
    {
        var found = await userService.UpdateUserAsync(new UpdateUserParams(req.Id)
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            PhoneNumber = req.PhoneNumber
        }, ct);

        return found ? Result.Ok() : Result.Fail("Could not find the specified user");
    }
}
