using Logistics.Domain.Core;
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

    public virtual Subscription? Subscription { get; set; }

    /// <summary>
    /// Users that belong to this tenant
    /// </summary>
    public virtual List<User> Users { get; } = [];
}
