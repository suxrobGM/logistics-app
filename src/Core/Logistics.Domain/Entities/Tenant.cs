using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public class Tenant : Entity, IMasterEntity
{
    public required string Name { get; set; }
    public string? CompanyName { get; set; }
    public required Address CompanyAddress { get; set; }
    public required string ConnectionString { get; set; }
    public required string BillingEmail { get; set; }
    public string? DotNumber { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? LogoPath { get; set; }
    public string? PhoneNumber { get; set; }

    #region Stripe Connect

    /// <summary>
    /// Stripe Connected Account ID for receiving payments from customers.
    /// </summary>
    public string? StripeConnectedAccountId { get; set; }

    /// <summary>
    /// Status of the Stripe Connect onboarding.
    /// </summary>
    public StripeConnectStatus ConnectStatus { get; set; } = StripeConnectStatus.NotConnected;

    /// <summary>
    /// Whether the connected account can receive payouts.
    /// </summary>
    public bool PayoutsEnabled { get; set; }

    /// <summary>
    /// Whether the connected account can accept charges.
    /// </summary>
    public bool ChargesEnabled { get; set; }

    #endregion

    public virtual Subscription? Subscription { get; set; }

    /// <summary>
    /// Users that belong to this tenant
    /// </summary>
    public virtual List<User> Users { get; } = [];
}
