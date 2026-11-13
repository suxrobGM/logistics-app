using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record LoadBoardSearchCriteria
{
    public LoadBoardPlace? Origin { get; set; }

    /// <summary>
    /// Deadhead radius in meters around the origin.
    /// </summary>
    public int OriginRadiusMeters { get; set; } = 80_000;

    public LoadBoardPlace? Destination { get; set; }

    /// <summary>
    /// Radius in meters around the destination.
    /// </summary>
    public int DestinationRadiusMeters { get; set; } = 80_000;

    public DateTime? PickupDateStart { get; set; }
    public DateTime? PickupDateEnd { get; set; }

    /// <summary>
    /// Equipment to match; empty or null matches any.
    /// </summary>
    public LoadBoardEquipmentType[]? EquipmentTypes { get; set; }

    public decimal? MinRatePerKm { get; set; }
    public decimal? MinTotalRate { get; set; }

    /// <summary>
    /// Maximum load weight in kilograms.
    /// </summary>
    public decimal? MaxWeightKg { get; set; }

    /// <summary>
    /// Maximum load length in meters.
    /// </summary>
    public decimal? MaxLengthMeters { get; set; }

    public int MaxResults { get; set; } = 100;
}

public record LoadBoardSearchResultDto
{
    public IEnumerable<LoadBoardListingDto> Listings { get; set; } = [];
    public int TotalCount { get; set; }
    public Dictionary<LoadBoardProviderType, int> CountByProvider { get; set; } = [];

    /// <summary>Error message per load board that failed. A failed board does not fail the search, so the UI shows these to avoid an empty result reading as no freight.</summary>
    public Dictionary<LoadBoardProviderType, string>? Errors { get; set; }
}
