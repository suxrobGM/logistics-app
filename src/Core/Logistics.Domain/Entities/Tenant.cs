using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public class Tenant : Entity, IMasterEntity
{
    /// <summary>
    ///     Unique name of the tenant.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Display name of the tenant.
    /// </summary>
    public string? CompanyName { get; set; }

    public Address? CompanyAddress { get; set; }
    public required string ConnectionString { get; set; }
    public required string BillingEmail { get; set; }
    public string? DotNumber { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? LogoPath { get; set; }
    public string? PhoneNumber { get; set; }

    /// <summary>
    ///     Regional and localization settings for this tenant.
    /// </summary>
    public TenantSettings Settings { get; set; } = new();

    public virtual Subscription? Subscription { get; set; }

    /// <summary>
    ///     Users that belong to this tenant
    /// </summary>
    public virtual List<User> Users { get; } = [];

    /// <summary>
    ///     Feature toggle configurations for this tenant.
    /// </summary>
    public virtual List<TenantFeatureConfig> FeatureConfigs { get; } = [];

    #region Stripe Connect

    /// <summary>
    ///     Stripe Connected Account ID for receiving payments from customers.
    /// </summary>
    public string? StripeConnectedAccountId { get; set; }

    /// <summary>
    ///     Status of the Stripe Connect onboarding.
    /// </summary>
    public StripeConnectStatus ConnectStatus { get; set; } = StripeConnectStatus.NotConnected;

    /// <summary>
    ///     Whether the connected account can receive payouts.
    /// </summary>
    public bool PayoutsEnabled { get; set; }

    /// <summary>
    ///     Whether the connected account can accept charges.
    /// </summary>
    public bool ChargesEnabled { get; set; }

    #endregion
}
