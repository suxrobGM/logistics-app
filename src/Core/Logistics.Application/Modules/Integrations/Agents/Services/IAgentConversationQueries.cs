using Logistics.Application.Abstractions.Common;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

/// <summary>
/// The conversation read path shared by the dispatch and copilot surfaces. Each surface keeps only
/// its own query, permission policy and feature gate.
/// </summary>
internal interface IAgentConversationQueries : IApplicationService
{
    Task<PagedResult<AgentConversationDto>> ListAsync(
        AgentConversationScope scope, int page, int pageSize, CancellationToken ct);

    /// <summary>
    /// Loads a conversation with its transcript. <paramref name="includeSessions"/> adds the
    /// per-turn sessions the dispatch board reports on; the copilot drawer never renders them.
    /// </summary>
    Task<Result<AgentConversationDto>> GetByIdAsync(
        AgentConversationScope scope, Guid conversationId, bool includeSessions, CancellationToken ct);
}
