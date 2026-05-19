using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.IdentityAccess.Invitations.Services;

internal sealed class InvitationExpiryService(
    IMasterUnitOfWork masterUow,
    ILogger<InvitationExpiryService> logger)
    : IInvitationExpiryService
{
    public async Task ExpireStaleAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var repo = masterUow.Repository<Invitation>();

        var stale = await repo.Query()
            .Where(i => i.Status == InvitationStatus.Pending && i.ExpiresAt < now)
            .ToListAsync(ct);

        if (stale.Count == 0)
        {
            return;
        }

        foreach (var invitation in stale)
        {
            invitation.Status = InvitationStatus.Expired;
            repo.Update(invitation);
        }

        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation("Expired {Count} stale invitations during scheduled sweep.", stale.Count);
    }
}
