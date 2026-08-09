using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Services;

/// <summary>
/// Shared by dispatch decision approval and rejection: AI must be enabled for the tenant, and the
/// decision must exist, belong to a dispatch turn, and still be Suggested.
/// </summary>
internal interface IAIDispatchDecisionGuard : IApplicationService
{
    Task<Result<AgentDecision>> LoadAsync(Guid decisionId, CancellationToken ct);
}
