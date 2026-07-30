namespace Logistics.Application.Abstractions.Payments.Stripe;

/// <summary>
/// <c>ISystemSettingsService</c> keys for Stripe resources created once and reused across runs.
/// </summary>
public static class StripeSettingKeys
{
    /// <summary>
    /// Id of the billing meter that AI overage usage is reported against. Written by the seeder,
    /// read when creating a metered price and when reporting usage - all three must agree.
    /// </summary>
    public const string AIOverageMeterId = "Stripe:AIOverageMeterId";
}
