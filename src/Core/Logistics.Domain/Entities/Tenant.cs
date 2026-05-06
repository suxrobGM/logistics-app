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

    public required Address CompanyAddress { get; set; }
    public required string ConnectionString { get; set; }
    public required string BillingEmail { get; set; }
    public string? DotNumber { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? LogoPath { get; set; }
    public string? PhoneNumber { get; set; }

    #region Regulatory & tax IDs

    /// <summary>
    /// US Motor Carrier number (FMCSA).
    /// </summary>
    public string? McNumber { get; set; }

    /// <summary>
    /// EU/UK VAT ID (e.g. DE123456789, GB987654321).
    /// </summary>
    public string? VatNumber { get; set; }

    /// <summary>
    /// Economic Operators Registration and Identification number (EU/UK customs).
    /// </summary>
    public string? EoriNumber { get; set; }

    /// <summary>
    /// Companies House / Handelsregister / RCS / equivalent registry number.
    /// </summary>
    public string? CompanyRegistrationNumber { get; set; }

    /// <summary>
    /// ISO-2 country code where the tenant is tax-resident.
    /// Defaults to <see cref="CompanyAddress"/>.<c>Country</c> when null.
    /// </summary>
    public string? TaxResidencyCountry { get; set; }

    #endregion

    /// <summary>
    ///     Regional and localization settings for this tenant.
    /// </summary>
    public TenantSettings Settings { get; set; } = new();

    /// <summary>
    ///     Whether this tenant requires an active subscription to access the platform.
    ///     Set to false for internal/test tenants.
    /// </summary>
    public bool IsSubscriptionRequired { get; set; } = true;

    /// <summary>
    ///     When set, AI quota counting starts from this date instead of the ISO week start.
    ///     Used by admins to reset a tenant's weekly AI session quota mid-week.
    /// </summary>
    public DateTime? QuotaResetAt { get; set; }

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
