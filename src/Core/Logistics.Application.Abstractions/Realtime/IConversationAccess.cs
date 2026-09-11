using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Abstractions.Realtime;

/// <summary>
/// Decides whether a caller may join a conversation's realtime group. Lives here so SignalR hubs
/// can enforce it without depending on the Application assembly, like <see cref="ITruckGeolocationUpdater"/>.
/// </summary>
public interface IConversationAccess : IApplicationService
{
    /// <summary>
    /// True when the conversation exists in the caller's own tenant and the caller is a participant,
    /// or the conversation is the tenant-wide chat.
    /// </summary>
    Task<bool> CanUserJoinConversationAsync(
        Guid tenantId, Guid conversationId, Guid userId, CancellationToken ct = default);
}
