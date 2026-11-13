using Logistics.Application.Abstractions.Payments;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>Reports AI overage usage to Stripe. Copilot and dispatch both use it, so they bill the same sessions.</summary>
internal sealed class AgentOverageReporter(
    IStripeUsageService stripeUsageService,
    ILogger<AgentOverageReporter> logger)
{
    /// <summary>Reports the session's overage to Stripe when it is billable, using <see cref="AIOverageBilling.Billable"/> so the metered amount matches the figure shown to the tenant.</summary>
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
                "Reported AI session overage for {ConversationKind} session {SessionId} (${CostUsd} model cost)",
                session.Conversation.Kind, session.Id, session.EstimatedCostUsd);
        }
        catch (Exception ex)
        {
            // Swallowed on purpose: the turn already ran and its output is the user's. Losing the
            // meter event costs us the charge, failing here would cost them the answer.
            logger.LogWarning(ex, "Failed to report AI session overage for session {SessionId}", session.Id);
        }
    }
}
