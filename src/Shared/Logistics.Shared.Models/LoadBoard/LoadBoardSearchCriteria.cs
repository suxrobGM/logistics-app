using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record LoadBoardSearchCriteria
{
    /// <summary>
    /// Origin location to search around
    /// </summary>
    public Address? OriginAddress { get; set; }

    /// <summary>
    /// Radius in miles from origin
    /// </summary>
    public int OriginRadius { get; set; } = 50;

    /// <summary>
    /// Destination location to search around
    /// </summary>
    public Address? DestinationAddress { get; set; }

    /// <summary>
    /// Radius in miles from destination
    /// </summary>
    public int DestinationRadius { get; set; } = 50;

    /// <summary>
    /// Pickup date range start
    /// </summary>
    public DateTime? PickupDateStart { get; set; }

    /// <summary>
    /// Pickup date range end
    /// </summary>
    public DateTime? PickupDateEnd { get; set; }

    /// <summary>
    /// Equipment types to filter by (e.g., "Flatbed", "Dry Van", "Reefer")
    /// </summary>
    public string[]? EquipmentTypes { get; set; }

    /// <summary>
    /// Minimum rate per mile
    /// </summary>
    public decimal? MinRatePerMile { get; set; }

    /// <summary>
    /// Minimum total rate
    /// </summary>
    public decimal? MinTotalRate { get; set; }

    /// <summary>
    /// Minimum weight in pounds
    /// </summary>
    public int? MinWeight { get; set; }

    /// <summary>
    /// Maximum weight in pounds
    /// </summary>
    public int? MaxWeight { get; set; }

    /// <summary>
    /// Maximum length in feet
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Providers to search (null = search all configured providers)
    /// </summary>
    public LoadBoardProviderType[]? Providers { get; set; }

    /// <summary>
    /// Maximum results to return
    /// </summary>
    public int MaxResults { get; set; } = 100;
}

public record LoadBoardSearchResultDto
{
    public IEnumerable<LoadBoardListingDto> Listings { get; set; } = [];
    public int TotalCount { get; set; }
    public Dictionary<LoadBoardProviderType, int> CountByProvider { get; set; } = [];
    public Dictionary<LoadBoardProviderType, string?>? Errors { get; set; }
}
