using System.Collections.Concurrent;

namespace Logistics.Application.Hubs;

/// <summary>
/// Tracks active connections for the messaging hub.
/// </summary>
public class MessagingHubContext
{
    private readonly ConcurrentDictionary<string, ActiveConnection> _connectedClients = new();

    /// <summary>
    /// Adds a client connection.
    /// </summary>
    public void AddClient(string connectionId, Guid? userId = null, string? tenantId = null)
    {
        _connectedClients.TryAdd(connectionId, new ActiveConnection
        {
            ConnectionId = connectionId,
            UserId = userId,
            TenantId = tenantId,
            ConnectedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Updates the user ID for a connection.
    /// </summary>
    public void SetUserId(string connectionId, Guid userId)
    {
        if (_connectedClients.TryGetValue(connectionId, out var connection))
        {
            connection.UserId = userId;
        }
    }

    /// <summary>
    /// Updates the tenant ID for a connection.
    /// </summary>
    public void SetTenantId(string connectionId, string tenantId)
    {
        if (_connectedClients.TryGetValue(connectionId, out var connection))
        {
            connection.TenantId = tenantId;
        }
    }

    /// <summary>
    /// Gets the user ID for a connection.
    /// </summary>
    public Guid? GetUserId(string connectionId)
    {
        return _connectedClients.TryGetValue(connectionId, out var connection) ? connection.UserId : null;
    }

    /// <summary>
    /// Gets the tenant ID for a connection.
    /// </summary>
    public string? GetTenantId(string connectionId)
    {
        return _connectedClients.TryGetValue(connectionId, out var connection) ? connection.TenantId : null;
    }

    /// <summary>
    /// Removes a client connection.
    /// </summary>
    public void RemoveClient(string connectionId)
    {
        _connectedClients.TryRemove(connectionId, out _);
    }

    /// <summary>
    /// Gets all active connection IDs for a user.
    /// </summary>
    public IEnumerable<string> GetConnectionsForUser(Guid userId)
    {
        return _connectedClients.Values
            .Where(c => c.UserId == userId)
            .Select(c => c.ConnectionId);
    }

    /// <summary>
    /// Gets the count of active connections.
    /// </summary>
    public int GetConnectionCount() => _connectedClients.Count;

    private class ActiveConnection
    {
        public required string ConnectionId { get; init; }
        public Guid? UserId { get; set; }
        public string? TenantId { get; set; }
        public DateTime ConnectedAt { get; init; }
    }
}
