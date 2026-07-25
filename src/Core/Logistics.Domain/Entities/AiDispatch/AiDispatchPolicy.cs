using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
/// The tenant's learned dispatch policy - a short markdown document the nightly job learns from the
/// dispatcher's approve/reject history and injected into the agent's system prompt.
/// <para>
/// Exactly one row per tenant. History is not versioned here on purpose: <c>ai_dispatch_decisions</c>
/// is already the append-only record of what the policy was derived from.
/// </para>
/// </summary>
public class AiDispatchPolicy : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// Markdown written by the nightly learning job. Overwritten on every successful run - never hand-edit,
    /// dispatcher text belongs in <see cref="ManualContent"/>.
    /// </summary>
    public string? GeneratedContent { get; private set; }

    /// <summary>
    /// Dispatcher-authored directives. The learning job never touches this, which is what lets
    /// the policy keep regenerating without clobbering human input.
    /// </summary>
    public string? ManualContent { get; private set; }

    /// <summary>
    /// When false the policy is neither injected into new sessions nor regenerated nightly.
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    public DateTime? GeneratedAt { get; private set; }

    /// <summary>Number of decisions the last learning pass read.</summary>
    public int DecisionsAnalyzed { get; private set; }

    /// <summary>
    /// Model that produced <see cref="GeneratedContent"/>. Platform observability only - tenants
    /// never see model names, so this must not reach the DTO.
    /// </summary>
    public string? ModelUsed { get; private set; }

    /// <summary>Cost of the last learning pass. Platform observability only, like <see cref="ModelUsed"/>.</summary>
    public decimal GenerationCostUsd { get; private set; }

    /// <summary>
    /// Newest <c>CreatedAt</c> among the decisions analyzed - the watermark that lets the nightly job
    /// skip tenants with no new labelled data instead of re-billing the same history.
    /// </summary>
    public DateTime? LastDecisionAt { get; private set; }

    /// <summary>
    /// When learning last ran, set even when the pass produced no policy so a tenant without
    /// enough evidence is not retried on a tight loop.
    /// </summary>
    public DateTime? LastRunAt { get; private set; }

    public DateTime? LastEditedAt { get; private set; }

    /// <summary>
    /// Who last edited <see cref="ManualContent"/>. Tracked separately from
    /// <see cref="AuditableEntity.UpdatedBy"/> because the nightly job writes this row too and would
    /// otherwise overwrite the attribution.
    /// </summary>
    public Guid? LastEditedByUserId { get; private set; }

    /// <summary>
    /// Records the result of a learning pass. A null <paramref name="generatedContent"/> clears
    /// the learned section - a policy the evidence no longer supports must stop steering the agent.
    /// Over-long text is clamped to whole lines so the column invariant holds without the caller
    /// having to know the limit.
    /// </summary>
    public void ApplyLearnedPolicy(
        string? generatedContent,
        int decisionsAnalyzed,
        DateTime? lastDecisionAt,
        string modelUsed,
        decimal costUsd)
    {
        GeneratedContent = DispatchPolicyText.KeepWholeLinesWithin(
            generatedContent, DispatchPolicyText.MaxStoredChars);
        DecisionsAnalyzed = decisionsAnalyzed;
        LastDecisionAt = lastDecisionAt;
        ModelUsed = modelUsed;
        GenerationCostUsd = costUsd;
        GeneratedAt = DateTime.UtcNow;
        LastRunAt = DateTime.UtcNow;
    }

    /// <summary>Marks a run that intentionally produced nothing, without touching the learned content.</summary>
    public void MarkRunCompleted()
    {
        LastRunAt = DateTime.UtcNow;
    }

    public void EditManual(string? manualContent, bool isEnabled, Guid? userId)
    {
        ManualContent = string.IsNullOrWhiteSpace(manualContent) ? null : manualContent;
        IsEnabled = isEnabled;
        LastEditedAt = DateTime.UtcNow;
        LastEditedByUserId = userId;
    }
}
