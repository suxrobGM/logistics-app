using Logistics.Application.Abstractions;
using Logistics.Application.Modules.IdentityAccess.Invitations.Services;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.IdentityAccess.Admins.Commands;

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
            return Result.Fail("Invitation not found.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return Result.Fail("Only pending invitations can be resent.");
        }

        if (invitation.IsExpired)
        {
            return Result.Fail("Cannot resend an expired invitation. Please create a new invitation.");
        }

        // Only the hash is stored, so a resend issues a fresh token; the previous link stops working.
        var token = TokenGenerator.GenerateSecureToken(64);
        invitation.TokenHash = TokenHasher.Hash(token);
        invitation.SendCount++;
        invitation.LastSentAt = DateTime.UtcNow;

        masterUow.Repository<Invitation>().Update(invitation);
        await masterUow.SaveChangesAsync(ct);

        await invitationEmailSender.SendAsync(invitation, token,
            invitation.InvitedByUser?.GetFullName() ?? "An administrator", tenant: null, isReminder: true);

        return Result.Ok();
    }
}
