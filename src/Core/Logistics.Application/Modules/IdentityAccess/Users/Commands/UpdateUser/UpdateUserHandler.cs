using Logistics.Application.Abstractions;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Users.Commands;

internal sealed class UpdateUserHandler(IMasterUnitOfWork masterUow, IUserService userService)
    : IRequestHandler<UpdateUserCommand, Result>
{
    public async Task<Result> Handle(UpdateUserCommand req, CancellationToken ct)
    {
        var user = await masterUow.Repository<User>().GetByIdAsync(req.Id, ct);

        if (user is null)
        {
            return Result.Fail("Could not find the specified user");
        }

        user.FirstName = PropertyUpdater.UpdateIfChanged(req.FirstName, user.FirstName);
        user.LastName = PropertyUpdater.UpdateIfChanged(req.LastName, user.LastName);
        user.PhoneNumber = PropertyUpdater.UpdateIfChanged(req.PhoneNumber, user.PhoneNumber);

        masterUow.Repository<User>().Update(user);
        await masterUow.SaveChangesAsync(ct);
        await userService.SyncTenantProfilesAsync(user, ct);
        return Result.Ok();
    }
}
