namespace Logistics.Shared.Models;

/// <summary>
/// The tenant's learned dispatch policy. Carries no model name and no generation cost: plans differ
/// by quota, not model tier, and tenants never see which model ran.
/// </summary>
public record AIDispatchPolicyDto
{
    public Guid Id { get; set; }

    /// <summary>Markdown learned from decision history. Read-only to the dispatcher.</summary>
    public string? GeneratedContent { get; set; }

    /// <summary>Dispatcher-authored directives. Editable, and never overwritten by the nightly job.</summary>
    public string? ManualContent { get; set; }

    /// <summary>False pauses both injection into new sessions and nightly learning.</summary>
    public bool IsEnabled { get; set; } = true;

    public DateTime? GeneratedAt { get; set; }
    public int DecisionsAnalyzed { get; set; }
    public DateTime? LastEditedAt { get; set; }
    public Guid? LastEditedByUserId { get; set; }

    /// <summary>
    /// A tenant with no policy row. Defined once so the read and regenerate endpoints cannot
    /// describe the same tenant differently.
    /// </summary>
    public static AIDispatchPolicyDto Empty => new() { IsEnabled = true };
}
