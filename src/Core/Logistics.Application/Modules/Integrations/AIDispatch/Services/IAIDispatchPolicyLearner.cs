using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Services;

/// <summary>
/// Turns the current tenant's approve/reject decision history into a short dispatch policy stored on
/// <c>AIDispatchPolicy</c> and injected into the agent's system prompt. The nightly Hangfire job
/// iterates tenants.
/// </summary>
public interface IAIDispatchPolicyLearner : IApplicationService
{
    /// <summary>
    /// Runs a learning pass. Skips - successfully, with a reason - when learning is off, the LLM is
    /// disabled, history is too thin, or nothing new has been decided since the last run.
    /// </summary>
    /// <param name="force">
    /// Bypasses the "no new decisions" and rate-limit skips (manual regenerate). Never bypasses the
    /// minimum-evidence or feature gates - those guard against fabricated rules.
    /// </param>
    Task<Result<AIDispatchPolicyLearningOutcome>> LearnForCurrentTenantAsync(
        bool force = false,
        CancellationToken ct = default);
}

/// <summary>What a learning pass did, for job logging and the manual-regenerate response.</summary>
/// <param name="SkipReason">Why the pass did nothing; null when <paramref name="Generated"/> is true.</param>
public sealed record AIDispatchPolicyLearningOutcome(
    bool Generated,
    string? SkipReason,
    int DecisionsAnalyzed,
    decimal CostUsd);
