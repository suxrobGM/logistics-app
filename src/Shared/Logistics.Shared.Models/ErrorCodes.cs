namespace Logistics.Shared.Models;

/// <summary>
///     Well-known error codes for programmatic error handling.
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    ///     The requested feature is not included in the tenant's current subscription plan.
    /// </summary>
    public const string FeatureNotInPlan = "FEATURE_NOT_IN_PLAN";

    /// <summary>
    ///     The feature has been disabled by a platform administrator.
    /// </summary>
    public const string FeatureDisabledByAdmin = "FEATURE_DISABLED_BY_ADMIN";

    /// <summary>
    ///     A resource limit defined by the subscription plan has been reached (e.g., max trucks).
    /// </summary>
    public const string ResourceLimitReached = "RESOURCE_LIMIT_REACHED";

    /// <summary>
    ///     The broker's credit is below the tenant's configured threshold, or its FMCSA operating
    ///     authority is inactive. Retryable by re-issuing the booking with the override flag set.
    /// </summary>
    public const string BrokerCreditBelowThreshold = "BROKER_CREDIT_BELOW_THRESHOLD";

    /// <summary>
    ///     No rate floor covers the listing's lane and the tenant has no default floor, so a
    ///     counter-offer cannot be checked against anything. Fixed by adding a lane floor.
    /// </summary>
    public const string NegotiationFloorMissing = "NEGOTIATION_FLOOR_MISSING";

    /// <summary>
    ///     The proposed counter-offer is below the tenant's rate floor for that lane.
    /// </summary>
    public const string NegotiationBelowFloor = "NEGOTIATION_BELOW_FLOOR";

    /// <summary>
    ///     Weekly AI budget spent and the owner opted to pause AI instead of billing overage
    ///     (TenantSettings.BlockAIOverage). Self-imposed - deliberately not an upgrade-prompt code.
    /// </summary>
    public const string AIBudgetReached = "AI_BUDGET_REACHED";

    /// <summary>
    ///     The one wording for <see cref="AIBudgetReached"/>, shared by the copilot send result, the
    ///     copilot transcript notice and the blocked dispatch session so all three name the same
    ///     setting and the same place to change it.
    /// </summary>
    public const string AIBudgetReachedMessage =
        "Your company's weekly AI budget is used up and AI is paused until it resets. " +
        "An owner can allow overage billing in Company Settings.";
}
