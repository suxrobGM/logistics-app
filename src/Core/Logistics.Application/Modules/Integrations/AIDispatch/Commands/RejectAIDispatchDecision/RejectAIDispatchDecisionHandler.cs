using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.AI;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class RejectAIDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser,
    IAIDispatchBroadcastService broadcastService,
    IOptions<LlmOptions> llmOptions) : IAppRequestHandler<RejectAIDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(RejectAIDispatchDecisionCommand request, CancellationToken ct)
    {
        var guard = await AIDispatchDecisionGuard.LoadAsync(
            tenantUow, llmOptions.Value, request.DecisionId, ct);
        if (!guard.IsSuccess)
            return Result.Fail(guard.Error!);

        var decision = guard.Value!;
        decision.Reject(currentUser.GetUserId() ?? Guid.Empty, request.Reason);

        var note = string.IsNullOrWhiteSpace(request.Reason)
            ? $"Rejected: {decision.ToolName}"
            : $"Rejected: {decision.ToolName} - {request.Reason}";

        await AIDispatchDecisionNotes.AppendAsync(tenantUow, broadcastService, decision, note, ct);
        return Result.Ok();
    }
}
