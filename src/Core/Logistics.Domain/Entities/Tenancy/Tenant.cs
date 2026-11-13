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

    /// <summary>What customers and staff see: the company name, or the tenant's unique name when none is set.</summary>
    public string DisplayName => CompanyName ?? Name;

    public required Address CompanyAddress { get; set; }
    public required string DatabaseName { get; set; }
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

    /// <summary>Country of tax residency; null means the company address country.</summary>
    public string? TaxResidencyCountry { get; set; }

    #endregion

    /// <summary>
    ///     Regional and localization settings for this tenant.
    /// </summary>
    public TenantSettings Settings { get; set; } = new();

    /// <summary>ISO 4217 code of the tenant currency; every tenant-internal amount is in it.</summary>
    public string Currency => Settings.Currency.ToString();

    /// <summary>
    ///     Admin-set company type. Kept off <see cref="Settings"/> because tenant owners can save those.
    /// </summary>
    public List<TenantPreset> Presets { get; set; } = [];

    public bool IsSolo => Presets.Contains(TenantPreset.SoloOperator);

    /// <summary>Whether the tenant needs an active subscription; false for internal and test tenants and for tenants created by an admin.</summary>
    public bool IsSubscriptionRequired { get; set; } = true;

    /// <summary>When an admin last reset the tenant's weekly AI quota; usage is counted from the later of this and the start of the week.</summary>
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
