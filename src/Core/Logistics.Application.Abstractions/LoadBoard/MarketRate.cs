namespace Logistics.Application.Abstractions.LoadBoard;

/// <summary>
/// The board's current spot rate for a lane (DAT RateView), linehaul in USD.
/// </summary>
public sealed record MarketRate(decimal RatePerMile, decimal? RatePerTrip, decimal? LowPerMile, decimal? HighPerMile);
