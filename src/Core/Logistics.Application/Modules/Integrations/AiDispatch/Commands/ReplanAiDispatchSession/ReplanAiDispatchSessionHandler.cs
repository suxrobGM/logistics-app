using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Abstractions.AiDispatch;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Commands;

internal sealed class ReplanAiDispatchSessionHandler(
    ICurrentUserService currentUser,
    ITenantUnitOfWork tenantUow,
    IBackgroundJobRunner<AiDispatchRequest> backgroundRunner) : IAppRequestHandler<ReplanAiDispatchSessionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ReplanAiDispatchSessionCommand request, CancellationToken ct)
    {
        var originalSession = await tenantUow.Repository<AiDispatchSession>()
            .GetByIdAsync(request.OriginalSessionId, ct);

        if (originalSession is null)
            return Result<Guid>.Fail("Original session not found");

        var rejectedDecisions = originalSession.Decisions
            .Where(d => d.Status == AiDispatchDecisionStatus.Rejected)
            .ToList();

        if (rejectedDecisions.Count == 0)
            return Result<Guid>.Fail("No rejected decisions found in the original session");

        var rejectionContext = string.Join("; ", rejectedDecisions.Select(d =>
        {
            var reason = d.RejectionReason ?? "no reason given";
            return $"{d.ToolName} (load: {d.LoadId}, truck: {d.TruckId}) - rejected: {reason}";
        }));

        var tenant = tenantUow.GetCurrentTenant();

        backgroundRunner.Enqueue(new AiDispatchRequest(
            TenantId: tenant.Id,
            Mode: originalSession.Mode,
            TriggeredByUserId: currentUser.GetUserId(),
            Instructions: request.AdditionalInstructions,
            RejectionContext: rejectionContext));

        return Result<Guid>.Ok(Guid.Empty);
    }
}
