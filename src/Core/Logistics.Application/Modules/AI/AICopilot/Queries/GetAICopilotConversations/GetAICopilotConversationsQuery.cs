using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AICopilot.Queries;

[RequiresFeature(TenantFeature.AICopilot)]
public class GetAICopilotConversationsQuery : PagedQuery, IQuery<PagedResult<AgentConversationDto>>;
