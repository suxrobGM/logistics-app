using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// The tenant's learned dispatch policy - a short markdown document the nightly job derives from the
/// dispatcher's approve/reject history and injects into the agent's system prompt. One row per tenant;
/// <c>agent_decisions</c> is already the append-only record it was derived from.
/// </summary>
public class AIDispatchPolicy : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// Markdown written by the nightly learning job, overwritten on every successful run.
    /// Dispatcher text belongs in <see cref="ManualContent"/>.
    /// </summary>
    public string? GeneratedContent { get; private set; }

    /// <summary>
    /// Dispatcher-authored directives. Never touched by the learning job, so regeneration cannot
    /// clobber human input.
    /// </summary>
    public string? ManualContent { get; private set; }

    /// <summary>When false the policy is neither injected into new sessions nor regenerated nightly.</summary>
    public bool IsEnabled { get; private set; } = true;

    public DateTime? GeneratedAt { get; private set; }

    /// <summary>Number of decisions the last learning pass read.</summary>
    public int DecisionsAnalyzed { get; private set; }

    /// <summary>Model that produced <see cref="GeneratedContent"/>. Platform observability only - keep out of DTOs.</summary>
    public string? ModelUsed { get; private set; }

    /// <summary>
    /// Newest <c>CreatedAt</c> among the decisions analyzed - the watermark that lets the nightly job
    /// skip tenants with no new labelled data instead of re-billing the same history.
    /// </summary>
    public DateTime? LastDecisionAt { get; private set; }

    /// <summary>When learning last ran. Set even for a pass that produced nothing, to avoid a tight retry loop.</summary>
    public DateTime? LastRunAt { get; private set; }

    public DateTime? LastEditedAt { get; private set; }

    /// <summary>
    /// Who last edited <see cref="ManualContent"/>. Separate from <see cref="AuditableEntity.UpdatedBy"/>,
    /// which the nightly job would otherwise overwrite.
    /// </summary>
    public Guid? LastEditedByUserId { get; private set; }

    public const int DefaultAutoApproveStreak = 25;
    public const int DefaultMinutesSavedPerDecision = 5;

    /// <summary>Never add a type that spends money, contacts someone outside the carrier, or notifies a driver.</summary>
    public static readonly IReadOnlySet<AgentDecisionType> AutoApproveEligibleTypes = new HashSet<AgentDecisionType>
    {
        AgentDecisionType.AssignLoad,
        AgentDecisionType.CreateTrip,
        AgentDecisionType.CreateInvoice
    };

    public List<AgentDecisionType> AutoApproveTypes { get; private set; } = [];

    /// <summary>Approvals in a row, with no rejection or revert, before a type runs on its own.</summary>
    public int AutoApproveStreak { get; private set; } = DefaultAutoApproveStreak;

    public int MinutesSavedPerDecision { get; private set; } = DefaultMinutesSavedPerDecision;

    public bool IsAutoApproveOptedIn(AgentDecisionType type) =>
        AutoApproveEligibleTypes.Contains(type) && AutoApproveTypes.Contains(type);

    /// <summary>Drops types outside <see cref="AutoApproveEligibleTypes"/>.</summary>
    public void SetAutoApprove(
        IEnumerable<AgentDecisionType> types, int streak, int minutesSavedPerDecision, Guid? userId)
    {
        AutoApproveTypes = [.. types.Distinct().Where(AutoApproveEligibleTypes.Contains)];
        AutoApproveStreak = streak;
        MinutesSavedPerDecision = minutesSavedPerDecision;
        LastEditedAt = DateTime.UtcNow;
        LastEditedByUserId = userId;
    }

    /// <summary>
    /// Records the result of a learning pass. A null <paramref name="generatedContent"/> clears the
    /// learned section, and over-long text is clamped to whole lines so callers need not know the limit.
    /// </summary>
    public void ApplyLearnedPolicy(
        string? generatedContent,
        int decisionsAnalyzed,
        DateTime? lastDecisionAt,
        string modelUsed)
    {
        GeneratedContent = DispatchPolicyText.KeepWholeLinesWithin(
            generatedContent, DispatchPolicyText.MaxStoredChars);
        DecisionsAnalyzed = decisionsAnalyzed;
        LastDecisionAt = lastDecisionAt;
        ModelUsed = modelUsed;
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
