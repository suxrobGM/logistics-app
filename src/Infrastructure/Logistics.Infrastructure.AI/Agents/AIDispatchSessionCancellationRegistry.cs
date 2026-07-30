using System.Collections.Concurrent;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>
/// Singleton registry for managing CancellationTokenSources for active dispatch agent sessions.
/// Enables cancellation of running sessions from external requests (API cancel endpoint).
/// </summary>
internal sealed class AIDispatchSessionCancellationRegistry
{
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> sessions = new();

    /// <summary>
    /// Registers a session and returns a linked CancellationToken that can be cancelled by the
    /// external token, by <see cref="TryCancel"/>, or by <paramref name="deadline"/> elapsing.
    /// </summary>
    /// <param name="deadline">
    /// Wall-clock ceiling on the whole session. The per-request HTTP timeout only bounds one call;
    /// without this, a session making 25 individually-fast calls has no upper bound at all.
    /// </param>
    public CancellationToken Register(Guid sessionId, CancellationToken externalCt, TimeSpan? deadline = null)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
        if (deadline is { } timeout && timeout > TimeSpan.Zero)
            cts.CancelAfter(timeout);

        sessions[sessionId] = cts;
        return cts.Token;
    }

    /// <summary>
    /// Cancels a running session. Returns true if the session was found and cancelled.
    /// </summary>
    public bool TryCancel(Guid sessionId)
    {
        if (!sessions.TryGetValue(sessionId, out var cts))
            return false;

        cts.Cancel();
        return true;
    }

    /// <summary>
    /// Unregisters a session and disposes its CancellationTokenSource.
    /// Must be called in a finally block after the session completes.
    /// </summary>
    public void Unregister(Guid sessionId)
    {
        if (sessions.TryRemove(sessionId, out var cts))
            cts.Dispose();
    }
}
