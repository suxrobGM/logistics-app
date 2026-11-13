namespace Logistics.Application.Abstractions.Integrations;

/// <summary>
/// The board's current spot rate for a lane (DAT RateView), linehaul in USD.
/// </summary>
public sealed record MarketRate(decimal RatePerKm, decimal? RatePerTrip, decimal? LowPerKm, decimal? HighPerKm);
