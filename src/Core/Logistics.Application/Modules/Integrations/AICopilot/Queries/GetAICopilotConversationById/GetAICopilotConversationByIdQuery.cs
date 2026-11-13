using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Queries;

[RequiresFeature(TenantFeature.AICopilot)]
public class GetAICopilotConversationByIdQuery : IQuery<Result<AgentConversationDto>>, IHaveId
{
    public Guid Id { get; set; }
}
