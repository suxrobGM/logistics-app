using System.Collections.Concurrent;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>
///     Tracks active connections for the messaging hub.
/// </summary>
public class ChatHubContext
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
    ///     Records that a connection passed the join check for a conversation.
    /// </summary>
    public void AddAuthorizedConversation(string connectionId, Guid conversationId)
    {
        if (connectedClients.TryGetValue(connectionId, out var connection))
        {
            connection.AuthorizedConversations.TryAdd(conversationId, 0);
        }
    }

    /// <summary>
    ///     Whether a connection already passed the join check for a conversation.
    /// </summary>
    public bool IsAuthorizedForConversation(string connectionId, Guid conversationId)
    {
        return connectedClients.TryGetValue(connectionId, out var connection) &&
               connection.AuthorizedConversations.ContainsKey(conversationId);
    }

    /// <summary>
    ///     Drops a conversation's authorization when the connection leaves it.
    /// </summary>
    public void RemoveAuthorizedConversation(string connectionId, Guid conversationId)
    {
        if (connectedClients.TryGetValue(connectionId, out var connection))
        {
            connection.AuthorizedConversations.TryRemove(conversationId, out _);
        }
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

        /// <summary>Conversations this connection has passed the join check for. Used as a set.</summary>
        public ConcurrentDictionary<Guid, byte> AuthorizedConversations { get; } = new();
    }
}
