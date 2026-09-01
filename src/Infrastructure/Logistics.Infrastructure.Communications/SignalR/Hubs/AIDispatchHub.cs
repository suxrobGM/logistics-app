using Logistics.Infrastructure.Communications.SignalR.Clients;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>Streams dispatch-board updates to the caller's tenant.</summary>
public class AIDispatchHub : TenantHub<IAIDispatchHubClient>
{
    public static string GroupName(Guid tenantId) => $"dispatch-board:{tenantId}";

    protected override string GroupNameFor(Guid tenantId, Guid userId) => GroupName(tenantId);
}
