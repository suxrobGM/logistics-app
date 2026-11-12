using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Application.Modules.Integrations.AIDispatch.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

internal sealed class GetAIDispatchAutonomyHandler(
    ITenantUnitOfWork tenantUow,
    IAgentTrustService trustService,
    IAgentDecisionDtoMapper decisionDtoMapper)
    : IRequestHandler<GetAIDispatchAutonomyQuery, Result<AIDispatchAutonomyDto>>
{
    public async Task<Result<AIDispatchAutonomyDto>> Handle(GetAIDispatchAutonomyQuery request, CancellationToken ct)
    {
        var policy = await tenantUow.Repository<AIDispatchPolicy>().Query().FirstOrDefaultAsync(ct)
                     ?? new AIDispatchPolicy();

        var types = new List<AutoApproveTypeDto>();
        foreach (var type in AIDispatchPolicy.AutoApproveEligibleTypes.Order())
        {
            types.Add(new AutoApproveTypeDto
            {
                Type = type,
                IsOptedIn = policy.IsAutoApproveOptedIn(type),
                Streak = await trustService.GetStreakAsync(type, ct)
            });
        }

        var now = DateTime.UtcNow;
        var autoExecuted = tenantUow.Repository<AgentDecision>().Query()
            .DispatchOnly()
            .Where(d => d.AutoApproved && d.Status == AgentDecisionStatus.Executed && d.RevertedAt == null);

        var monthAgo = now.AddDays(-30);
        var autoExecutedCount = await autoExecuted.CountAsync(d => d.ExecutedAt >= monthAgo, ct);

        var windowStart = now - AgentDecision.RevertWindow;
        var revertable = await autoExecuted
            .Where(d => d.ExecutedAt >= windowStart)
            .OrderByDescending(d => d.ExecutedAt)
            .ToListAsync(ct);

        return Result<AIDispatchAutonomyDto>.Ok(new AIDispatchAutonomyDto
        {
            RequiredStreak = policy.AutoApproveStreak,
            MinutesSavedPerDecision = policy.MinutesSavedPerDecision,
            Types = types,
            AutoExecutedLast30Days = autoExecutedCount,
            MinutesSavedLast30Days = autoExecutedCount * policy.MinutesSavedPerDecision,
            Revertable = await decisionDtoMapper.MapAsync(revertable, ct)
        });
    }
}
