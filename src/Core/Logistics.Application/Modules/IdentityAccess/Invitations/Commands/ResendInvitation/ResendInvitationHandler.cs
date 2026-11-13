using Logistics.Application.Abstractions;
using Logistics.Application.Common.Security;
using Logistics.Application.Modules.IdentityAccess.Invitations.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;


namespace Logistics.Application.Modules.IdentityAccess.Invitations.Commands;

internal sealed class ResendInvitationHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IInvitationEmailSender invitationEmailSender)
    : IRequestHandler<ResendInvitationCommand, Result>
{
    public async Task<Result> Handle(ResendInvitationCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        var invitation = await masterUow.Repository<Invitation>()
            .Query()
            .FirstOrDefaultAsync(i => i.Id == req.Id && i.TenantId == tenant.Id, ct);

        if (invitation is null)
        {
            return Result.Fail("Invitation not found.", ErrorCodes.NotFound);
        }

        // Only the hash is stored, so a resend issues a fresh token; the previous link stops working.
        var token = TokenGenerator.GenerateSecureToken(64);
        if (invitation.Resend(TokenHasher.Hash(token)) is { } resendBlocked)
        {
            return Result.Fail(resendBlocked);
        }

        await masterUow.SaveChangesAsync(ct);

        await invitationEmailSender.SendAsync(invitation, token,
            invitation.InvitedByUser?.GetFullName(), tenant, isReminder: true);

        return Result.Ok();
    }
}
