using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Free time and hourly rate for waiting at a stop. A null <see cref="RatePerHour"/> means detention
/// is tracked and alerted but not billed.
/// </summary>
public sealed record DetentionTerms(int FreeMinutes, decimal? RatePerHour)
{
    public const int DefaultFreeMinutes = 120;

    /// <summary>The customer's own terms win field by field over the tenant's.</summary>
    public static DetentionTerms For(TenantSettings? tenant, Customer? customer) => new(
        customer?.DetentionFreeMinutes ?? tenant?.DetentionFreeMinutes ?? DefaultFreeMinutes,
        customer?.DetentionRatePerHour ?? tenant?.DetentionRatePerHour);
}
