using System.Text.Json.Serialization;

namespace Logistics.Infrastructure.Integrations.Eld.TtEld;

internal record TtEldDriversResponse
{
    [JsonPropertyName("data")]
    public List<TtEldDriverData>? Data { get; init; }

    [JsonPropertyName("meta")]
    public TtEldPaginationMeta? Meta { get; init; }
}

internal record TtEldDriverData
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; init; }

    [JsonPropertyName("second_name")]
    public string? SecondName { get; init; }
}

internal record TtEldPaginationMeta
{
    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("perPage")]
    public int PerPage { get; init; }

    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; init; }
}

internal record TtEldUnitsResponse
{
    [JsonPropertyName("data")]
    public List<TtEldUnitData>? Data { get; init; }

    [JsonPropertyName("meta")]
    public TtEldPaginationMeta? Meta { get; init; }
}

internal record TtEldUnitData
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("vin")]
    public string? Vin { get; init; }

    [JsonPropertyName("truck_number")]
    public string? TruckNumber { get; init; }

    [JsonPropertyName("driver")]
    public TtEldDriverRef? Driver { get; init; }

    [JsonPropertyName("codriver")]
    public TtEldDriverRef? Codriver { get; init; }
}

internal record TtEldDriverRef
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; init; }

    [JsonPropertyName("second_name")]
    public string? SecondName { get; init; }
}

internal record TtEldTrackingV2Response
{
    [JsonPropertyName("units")]
    public List<TtEldTrackingUnit>? Units { get; init; }
}

internal record TtEldTrackingUnit
{
    [JsonPropertyName("truck_number")]
    public string? TruckNumber { get; init; }

    [JsonPropertyName("vin")]
    public string? Vin { get; init; }

    [JsonPropertyName("coordinates")]
    public TtEldCoordinates? Coordinates { get; init; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; init; }
}

internal record TtEldCoordinates
{
    [JsonPropertyName("lat")]
    public double Lat { get; init; }

    [JsonPropertyName("lng")]
    public double Lng { get; init; }
}

internal record TtEldTrackingPoint
{
    [JsonPropertyName("address")]
    public string? Address { get; init; }

    [JsonPropertyName("coordinates")]
    public TtEldCoordinates? Coordinates { get; init; }

    [JsonPropertyName("rotation")]
    public int? Rotation { get; init; }

    [JsonPropertyName("speed")]
    public int? Speed { get; init; }

    [JsonPropertyName("driverId")]
    public string? DriverId { get; init; }

    [JsonPropertyName("odometer")]
    public int? Odometer { get; init; }

    [JsonPropertyName("date")]
    public string? Date { get; init; }
}
