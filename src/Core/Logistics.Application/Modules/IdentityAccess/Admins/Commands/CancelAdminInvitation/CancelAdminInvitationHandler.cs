using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Admins.Commands;

internal sealed class CancelAdminInvitationHandler(IMasterUnitOfWork masterUow)
    : IRequestHandler<CancelAdminInvitationCommand, Result>
{
    public async Task<Result> Handle(CancelAdminInvitationCommand req, CancellationToken ct)
    {
        var invitation = await masterUow.Repository<Invitation>()
            .GetAsync(i => i.Id == req.Id && i.Type == InvitationType.AppUser, ct);

        if (invitation is null)
        {
            return Result.Fail("Invitation not found.", ErrorCodes.NotFound);
        }

        if (invitation.Cancel() is { } cancelBlocked)
        {
            return Result.Fail(cancelBlocked);
        }

        await masterUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
