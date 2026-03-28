using System.Collections.Concurrent;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// Singleton registry for managing CancellationTokenSources for active dispatch agent sessions.
/// Enables cancellation of running sessions from external requests (API cancel endpoint).
/// </summary>
internal sealed class DispatchSessionCancellationRegistry
{
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> sessions = new();

    /// <summary>
    /// Registers a session and returns a linked CancellationToken that can be cancelled
    /// either by the external token or by calling <see cref="TryCancel"/>.
    /// </summary>
    public CancellationToken Register(Guid sessionId, CancellationToken externalCt)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
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
