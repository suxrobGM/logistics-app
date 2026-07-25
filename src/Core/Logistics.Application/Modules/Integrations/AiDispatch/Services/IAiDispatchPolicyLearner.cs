using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Services;

/// <summary>
/// Turns the current tenant's approve/reject decision history into a short dispatch policy
/// document stored on <c>AiDispatchPolicy</c> and injected into the agent's system prompt.
/// Operates on the current tenant; the nightly Hangfire job iterates tenants.
/// </summary>
public interface IAiDispatchPolicyLearner : IApplicationService
{
    /// <summary>
    /// Runs a learning pass. Skips - successfully, with a reason - when learning is off, the LLM
    /// is disabled for the tenant, there is too little labelled history, or nothing new has been
    /// decided since the last run.
    /// </summary>
    /// <param name="force">
    /// Bypasses the "no new decisions" and rate-limit skips (used by the manual regenerate endpoint).
    /// Never bypasses the minimum-evidence or feature gates - those protect against fabricated rules.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task<Result<AiDispatchPolicyLearningOutcome>> LearnForCurrentTenantAsync(
        bool force = false,
        CancellationToken ct = default);
}

/// <summary>
/// What a learning pass did, for job logging and the manual-regenerate response.
/// </summary>
/// <param name="Generated">True when a new policy document was written.</param>
/// <param name="SkipReason">Why the pass did nothing; null when <paramref name="Generated"/> is true.</param>
/// <param name="DecisionsAnalyzed">Decisions read from history.</param>
/// <param name="CostUsd">Estimated LLM cost of this pass.</param>
public sealed record AiDispatchPolicyLearningOutcome(
    bool Generated,
    string? SkipReason,
    int DecisionsAnalyzed,
    decimal CostUsd);
