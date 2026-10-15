using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record LoadBoardSearchCriteria
{
    public LoadBoardPlace? Origin { get; set; }

    /// <summary>
    /// Deadhead radius in miles around the origin.
    /// </summary>
    public int OriginRadius { get; set; } = 50;

    public LoadBoardPlace? Destination { get; set; }

    /// <summary>
    /// Radius in miles around the destination.
    /// </summary>
    public int DestinationRadius { get; set; } = 50;

    public DateTime? PickupDateStart { get; set; }
    public DateTime? PickupDateEnd { get; set; }

    /// <summary>
    /// Equipment to match; empty or null matches any.
    /// </summary>
    public LoadBoardEquipmentType[]? EquipmentTypes { get; set; }

    public decimal? MinRatePerMile { get; set; }
    public decimal? MinTotalRate { get; set; }

    /// <summary>
    /// Maximum load weight in pounds.
    /// </summary>
    public int? MaxWeight { get; set; }

    /// <summary>
    /// Maximum load length in feet.
    /// </summary>
    public int? MaxLength { get; set; }

    public int MaxResults { get; set; } = 100;
}

public record LoadBoardSearchResultDto
{
    public IEnumerable<LoadBoardListingDto> Listings { get; set; } = [];
    public int TotalCount { get; set; }
    public Dictionary<LoadBoardProviderType, int> CountByProvider { get; set; } = [];

    /// <summary>
    /// Why a provider returned nothing, keyed by provider. A failed provider never fails the
    /// whole search, so the caller must show these or it reads as "no freight".
    /// </summary>
    public Dictionary<LoadBoardProviderType, string>? Errors { get; set; }
}
