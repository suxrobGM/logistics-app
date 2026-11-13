using Logistics.Application.Abstractions;
using Logistics.Application.Common.Security;
using Logistics.Application.Modules.Identity.Invitations.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Identity.Admins.Commands;

internal sealed class ResendAdminInvitationHandler(
    IMasterUnitOfWork masterUow,
    IInvitationEmailSender invitationEmailSender)
    : IRequestHandler<ResendAdminInvitationCommand, Result>
{
    public async Task<Result> Handle(ResendAdminInvitationCommand req, CancellationToken ct)
    {
        var invitation = await masterUow.Repository<Invitation>()
            .Query()
            .FirstOrDefaultAsync(i => i.Id == req.Id && i.Type == InvitationType.AppUser, ct);

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
            invitation.InvitedByUser?.GetFullName(), tenant: null, isReminder: true);

        return Result.Ok();
    }
}
