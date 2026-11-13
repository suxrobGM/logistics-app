using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.AI.Negotiation.Services;

/// <summary>Currency of a load board listing's rate. A listing without a total rate carries no currency and is treated as being in the tenant currency.</summary>
public static class ListingCurrency
{
    public static string Get(LoadBoardListing listing, Tenant tenant) => listing.TotalRate?.Currency ?? tenant.Currency;

    /// <summary>True when the listing's rate is in the tenant currency. Rate floors are stored in tenant currency, so only such listings can be negotiated.</summary>
    public static bool IsTenantCurrency(LoadBoardListing listing, Tenant tenant) =>
        string.Equals(Get(listing, tenant), tenant.Currency, StringComparison.OrdinalIgnoreCase);
}
