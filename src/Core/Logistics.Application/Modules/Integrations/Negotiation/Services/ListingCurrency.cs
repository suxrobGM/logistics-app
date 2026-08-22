using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

/// <summary>
/// The currency a listing's money figures are quoted in. A load board that publishes no total rate
/// publishes no currency either, so the floor context, the preview, the counter-offer and the email
/// all fall back through here rather than each picking their own default.
/// </summary>
public static class ListingCurrency
{
    public const string Default = "USD";

    public static string Of(LoadBoardListing listing) => listing.TotalRate?.Currency ?? Default;
}
