using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class GetAIDispatchConversationsQuery : PagedQuery, IQuery<PagedResult<AgentConversationDto>>;
