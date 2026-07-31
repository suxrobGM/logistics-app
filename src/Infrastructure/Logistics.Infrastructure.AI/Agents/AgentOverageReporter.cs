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
    /// Eligibility is <see cref="AIOverageBilling.Billable"/>, shared with the tenant-visible
    /// accrued figure so the meter and the displayed number cannot drift apart.
    /// </summary>
    public async Task ReportIfOverBudgetAsync(AgentSession session, Guid tenantId)
    {
        if (!AIOverageBilling.IsBillable(session))
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
