using System.Collections.Concurrent;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>
///     Tracks active connections for the messaging hub.
/// </summary>
public class MessagingHubContext
{
    private readonly ConcurrentDictionary<string, ActiveConnection> connectedClients = new();

    /// <summary>
    ///     Adds a client connection.
    /// </summary>
    public void AddClient(string connectionId, Guid? userId = null, string? tenantId = null)
    {
        connectedClients.TryAdd(connectionId,
            new ActiveConnection
            {
                ConnectionId = connectionId, UserId = userId, TenantId = tenantId, ConnectedAt = DateTime.UtcNow
            });
    }

    /// <summary>
    ///     Updates the user ID for a connection.
    /// </summary>
    public void SetUserId(string connectionId, Guid userId)
    {
        if (connectedClients.TryGetValue(connectionId, out var connection))
        {
            connection.UserId = userId;
        }
    }

    /// <summary>
    ///     Updates the tenant ID for a connection.
    /// </summary>
    public void SetTenantId(string connectionId, string tenantId)
    {
        if (connectedClients.TryGetValue(connectionId, out var connection))
        {
            connection.TenantId = tenantId;
        }
    }

    /// <summary>
    ///     Gets the user ID for a connection.
    /// </summary>
    public Guid? GetUserId(string connectionId)
    {
        return connectedClients.TryGetValue(connectionId, out var connection) ? connection.UserId : null;
    }

    /// <summary>
    ///     Gets the tenant ID for a connection.
    /// </summary>
    public string? GetTenantId(string connectionId)
    {
        return connectedClients.TryGetValue(connectionId, out var connection) ? connection.TenantId : null;
    }

    /// <summary>
    ///     Removes a client connection.
    /// </summary>
    public void RemoveClient(string connectionId)
    {
        connectedClients.TryRemove(connectionId, out _);
    }

    /// <summary>
    ///     Gets all active connection IDs for a user.
    /// </summary>
    public IEnumerable<string> GetConnectionsForUser(Guid userId)
    {
        return connectedClients.Values
            .Where(c => c.UserId == userId)
            .Select(c => c.ConnectionId);
    }

    /// <summary>
    ///     Gets the count of active connections.
    /// </summary>
    public int GetConnectionCount()
    {
        return connectedClients.Count;
    }

    private class ActiveConnection
    {
        public required string ConnectionId { get; init; }
        public Guid? UserId { get; set; }
        public string? TenantId { get; set; }
        public DateTime ConnectedAt { get; init; }
    }
}
