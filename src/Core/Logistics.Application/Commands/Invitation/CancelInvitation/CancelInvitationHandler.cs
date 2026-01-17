using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CancelInvitationHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CancelInvitationCommand, Result>
{
    public async Task<Result> Handle(CancelInvitationCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        var invitation = await masterUow.Repository<Invitation>()
            .GetAsync(i => i.Id == req.Id && i.TenantId == tenant.Id, ct);

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
