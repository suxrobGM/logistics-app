using Logistics.Infrastructure.Communications.SignalR.Clients;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>Streams private copilot events to the authenticated user.</summary>
public class CopilotHub : TenantHub<IAICopilotHubClient>
{
    public static string GroupName(Guid tenantId, Guid userId) => $"copilot:{tenantId}:{userId}";

    protected override string GroupNameFor(Guid tenantId, Guid userId) => GroupName(tenantId, userId);
}
