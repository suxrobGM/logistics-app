using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Admins.Commands;

internal sealed class CancelAdminInvitationHandler(IMasterUnitOfWork masterUow)
    : IAppRequestHandler<CancelAdminInvitationCommand, Result>
{
    public async Task<Result> Handle(CancelAdminInvitationCommand req, CancellationToken ct)
    {
        var invitation = await masterUow.Repository<Invitation>()
            .GetAsync(i => i.Id == req.Id && i.Type == InvitationType.AppUser, ct);

        if (invitation is null)
        {
            return Result.Fail("Invitation not found.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return Result.Fail("Only pending invitations can be cancelled.");
        }

        invitation.Status = InvitationStatus.Cancelled;

        masterUow.Repository<Invitation>().Update(invitation);
        await masterUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
