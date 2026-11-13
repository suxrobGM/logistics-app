using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.AI.Negotiation.Services;

/// <summary>
/// The currency a listing's money figures are quoted in. A listing with no total rate publishes no
/// currency, so it reads as the tenant's.
/// </summary>
public static class ListingCurrency
{
    public static string Get(LoadBoardListing listing, Tenant tenant) => listing.TotalRate?.Currency ?? tenant.Currency;

    /// <summary>Rate floors are in the tenant currency, so a listing in any other can't be negotiated.</summary>
    public static bool IsTenantCurrency(LoadBoardListing listing, Tenant tenant) =>
        string.Equals(Get(listing, tenant), tenant.Currency, StringComparison.OrdinalIgnoreCase);
}
