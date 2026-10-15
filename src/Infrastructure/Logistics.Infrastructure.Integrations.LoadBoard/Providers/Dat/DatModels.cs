namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;

// DAT wire shapes, camelCase on the wire. Verification status (see docs/loadboard-dat-setup.md):
// - tokens: verified (public client code for /access/v1/token/organization and /token/user)
// - search v1 request and match: verified against a captured production response (2021)
// - posting: the load shape is verified; truck posts are assumed to mirror it
// - RateView lookup: verified request and response

#region Identity

internal sealed record DatOrganizationTokenRequest(string Username, string Password);

internal sealed record DatUserTokenRequest(string Username);

internal sealed record DatTokenResponse
{
    public string? AccessToken { get; init; }

    /// <summary>ISO-8601 UTC, e.g. <c>2026-10-14T21:05:00.000Z</c>.</summary>
    public DateTime? ExpiresWhen { get; init; }
}

#endregion

#region Search (freight matching v1)

internal sealed record DatSearchRequest(DatSearchCriteria Criteria);

internal sealed record DatSearchCriteria
{
    public required DatDesiredSpecs DesiredSpecs { get; init; }
    public int MaxAgeInMinutes { get; init; }
    public DatAvailability? Availability { get; init; }
    public required DatPlace Origin { get; init; }
    public DatPlace? Destination { get; init; }
}

internal sealed record DatDesiredSpecs
{
    public string[]? EquipmentTypes { get; init; }

    /// <summary>FULL, PARTIAL or BOTH.</summary>
    public string Capacity { get; init; } = "BOTH";
}

internal sealed record DatAvailability
{
    public DateTime? Earliest { get; init; }
    public DateTime? Latest { get; init; }
}

internal sealed record DatPlace(DatPoint Point);

internal sealed record DatPoint
{
    public string? City { get; init; }
    public string? State { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public DatMiles? Deadhead { get; init; }
}

internal sealed record DatMiles
{
    public int? Miles { get; init; }
}

internal sealed record DatSearchCreatedResponse
{
    public string? Id { get; init; }
}

internal sealed record DatMatch
{
    public string? MatchId { get; init; }
    public string? PostingId { get; init; }
    public DatPlace? Origin { get; init; }
    public DatPlace? Destination { get; init; }
    public DatTripLength? TripLength { get; init; }
    public DatMatchSpecs? Specs { get; init; }
    public DatAvailability? Availability { get; init; }
    public DatRate? Rate { get; init; }
    public DatPoster? Poster { get; init; }
    public DatCredit? Credit { get; init; }
}

internal sealed record DatTripLength
{
    public double? DistanceMiles { get; init; }
}

internal sealed record DatMatchSpecs
{
    public string? RequiredEquipmentType { get; init; }
    public int? LengthInFeet { get; init; }
    public int? WeightInPounds { get; init; }
}

internal sealed record DatRate
{
    /// <summary>FLAT (a trip total) or PER_MILE.</summary>
    public string? RateBasedOn { get; init; }
    public DatMoney? BaseRate { get; init; }
}

internal sealed record DatMoney
{
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
}

internal sealed record DatPoster
{
    public string? Company { get; init; }
    public DatContact? Contact { get; init; }
}

internal sealed record DatContact
{
    public string? Email { get; init; }
    public DatPhone? Phone { get; init; }
}

internal sealed record DatPhone
{
    public string? Number { get; init; }
    public string? Extension { get; init; }
}

internal sealed record DatCredit
{
    public int? Score { get; init; }
    public int? DaysToPay { get; init; }
}

#endregion

#region Posting v2

/// <summary>
/// A truck post. Assumed to mirror the verified <c>/posting/v2/loads</c> body
/// (<c>freight</c> / <c>lane</c> / <c>exposure</c>).
/// </summary>
internal sealed record DatTruckPostingRequest
{
    public required DatPostingFreight Freight { get; init; }
    public required DatPostingLane Lane { get; init; }
    public required DatPostingExposure Exposure { get; init; }
    public string? ReferenceId { get; init; }
}

internal sealed record DatPostingFreight
{
    public required string EquipmentType { get; init; }
    public string FullPartial { get; init; } = "FULL";
    public int? LengthFeet { get; init; }
    public int? WeightPounds { get; init; }
}

internal sealed record DatPostingLane
{
    public required DatPostingPlace Origin { get; init; }
    public DatPostingPlace? Destination { get; init; }
}

internal sealed record DatPostingPlace
{
    public string? City { get; init; }
    public string? StateProv { get; init; }
    public string? PostalCode { get; init; }
}

internal sealed record DatPostingExposure
{
    public DateTime EarliestAvailabilityWhen { get; init; }
    public DateTime? LatestAvailabilityWhen { get; init; }
    public string PreferredContactMethod { get; init; } = "PHONE";

    /// <summary>When the post comes off the board (assumed field name).</summary>
    public DateTime? EndWhen { get; init; }
}

internal sealed record DatPosting
{
    public string? Id { get; init; }
    public DatPostingExposure? Exposure { get; init; }
}

#endregion

#region RateView

internal sealed record DatRateLookupRequest
{
    public required DatRatePlace Origin { get; init; }
    public required DatRatePlace Destination { get; init; }
    public string RateType { get; init; } = "SPOT";
    public required string Equipment { get; init; }
    public bool IncludeMyRate { get; init; }
    public DatRateEscalation TargetEscalation { get; init; } = new();
    public DatRateTimePeriod RateTimePeriod { get; init; } = new();
}

internal sealed record DatRatePlace(string City, string StateOrProvince);

internal sealed record DatRateEscalation
{
    public string EscalationType { get; init; } = "BEST_FIT";
}

internal sealed record DatRateTimePeriod
{
    public string RateTense { get; init; } = "CURRENT";
}

internal sealed record DatRateLookupResponse
{
    public List<DatRateResponseItem>? RateResponses { get; init; }
}

internal sealed record DatRateResponseItem
{
    public DatRateResult? Response { get; init; }
}

internal sealed record DatRateResult
{
    public DatRateValues? Rate { get; init; }
}

internal sealed record DatRateValues
{
    public DatRateRange? PerMile { get; init; }
    public DatRateRange? PerTrip { get; init; }
    public double? Mileage { get; init; }
}

internal sealed record DatRateRange
{
    public decimal? RateUsd { get; init; }
    public decimal? HighUsd { get; init; }
    public decimal? LowUsd { get; init; }
}

#endregion
