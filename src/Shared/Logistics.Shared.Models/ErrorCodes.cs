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
}
