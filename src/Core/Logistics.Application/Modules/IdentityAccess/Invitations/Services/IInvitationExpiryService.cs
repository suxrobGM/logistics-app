namespace Logistics.Application.Modules.IdentityAccess.Invitations.Services;

/// <summary>
/// Bulk-transitions pending invitations whose <c>ExpiresAt</c> is in the past to
/// <see cref="Logistics.Domain.Primitives.Enums.InvitationStatus.Expired"/>. Invoked daily by Hangfire.
/// </summary>
public interface IInvitationExpiryService : IApplicationService
{
    Task ExpireStaleAsync(CancellationToken ct = default);
}
