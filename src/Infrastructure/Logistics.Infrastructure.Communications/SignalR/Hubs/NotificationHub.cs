using Logistics.Infrastructure.Communications.SignalR.Clients;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>Streams notifications to the caller's tenant.</summary>
public class NotificationHub : TenantHub<INotificationHubClient>;
