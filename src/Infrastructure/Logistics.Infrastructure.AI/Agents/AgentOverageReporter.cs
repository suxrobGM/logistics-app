using Logistics.Application.Abstractions.Payments.Stripe;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>
/// Meters an over-budget session to Stripe. Shared by both agent surfaces so they cannot drift on
/// which sessions bill.
/// </summary>
internal sealed class AgentOverageReporter(
    IStripeUsageService stripeUsageService,
    ILogger<AgentOverageReporter> logger)
{
    /// <summary>
    /// Completed sessions only. A failed or cancelled run still spent its budget, but billing for
    /// a turn that produced no answer is not defensible - that gap is priced into
    /// <c>AIOverageBilling.CostMarkup</c>.
    /// </summary>
    public async Task ReportIfOverBudgetAsync(AgentSession session, Guid tenantId)
    {
        if (!session.IsOverage || session.Status != AgentSessionStatus.Completed)
        {
            return;
        }

        try
        {
            await stripeUsageService.ReportAISessionOverageAsync(tenantId, session.EstimatedCostUsd);
            logger.LogInformation(
                "Reported AI session overage for {SessionType} session {SessionId} (${CostUsd} model cost)",
                session.Type, session.Id, session.EstimatedCostUsd);
        }
        catch (Exception ex)
        {
            // Swallowed on purpose: the turn already ran and its output is the user's. Losing the
            // meter event costs us the charge, failing here would cost them the answer.
            logger.LogWarning(ex, "Failed to report AI session overage for session {SessionId}", session.Id);
        }
    }
}
