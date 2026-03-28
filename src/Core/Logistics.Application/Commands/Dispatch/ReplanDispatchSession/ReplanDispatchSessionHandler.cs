using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class ReplanDispatchSessionHandler(
    ICurrentUserService currentUser,
    ITenantUnitOfWork tenantUow,
    IServiceScopeFactory scopeFactory,
    ILogger<ReplanDispatchSessionHandler> logger) : IAppRequestHandler<ReplanDispatchSessionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ReplanDispatchSessionCommand request, CancellationToken ct)
    {
        var originalSession = await tenantUow.Repository<DispatchSession>()
            .GetByIdAsync(request.OriginalSessionId, ct);

        if (originalSession is null)
            return Result<Guid>.Fail("Original session not found");

        // Build rejection context from rejected decisions
        var rejectedDecisions = originalSession.Decisions
            .Where(d => d.Status == DispatchDecisionStatus.Rejected)
            .ToList();

        if (rejectedDecisions.Count == 0)
            return Result<Guid>.Fail("No rejected decisions found in the original session");

        var rejectionContext = string.Join("; ", rejectedDecisions.Select(d =>
        {
            var reason = d.RejectionReason ?? "no reason given";
            return $"{d.ToolName} (load: {d.LoadId}, truck: {d.TruckId}) — rejected: {reason}";
        }));

        var tenant = tenantUow.GetCurrentTenant();
        var agentRequest = new DispatchAgentRequest(
            TenantId: tenant.Id,
            Mode: originalSession.Mode,
            TriggeredByUserId: currentUser.GetUserId(),
            Instructions: request.AdditionalInstructions,
            RejectionContext: rejectionContext);

        // Fire-and-forget: run the agent in a background scope
        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var backgroundUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
            backgroundUow.SetCurrentTenant(tenant);

            var agentService = scope.ServiceProvider.GetRequiredService<IDispatchAgentService>();
            try
            {
                await agentService.RunAsync(agentRequest, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background re-plan dispatch agent failed for tenant {TenantId}", tenant.Id);
            }
        }, CancellationToken.None);

        return Result<Guid>.Ok(Guid.Empty);
    }
}
