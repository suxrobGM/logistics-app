using System.Text.Json.Serialization;

namespace Logistics.Infrastructure.Integrations.Eld.Providers.TtEld;

// TT ELD wire format is mostly camelCase — see EldJsonOptions.CamelCase. Only fields whose
// JSON key uses snake_case keep an explicit [JsonPropertyName].

internal record TtEldDriversResponse
{
    public List<TtEldDriverData>? Data { get; init; }
    public TtEldPaginationMeta? Meta { get; init; }
}

internal record TtEldDriverData
{
    public string? Id { get; init; }
    [JsonPropertyName("first_name")] public string? FirstName { get; init; }
    [JsonPropertyName("second_name")] public string? SecondName { get; init; }
}

internal record TtEldPaginationMeta
{
    public int Page { get; init; }
    public int PerPage { get; init; }
    public int Total { get; init; }
    public int TotalPages { get; init; }
}

internal record TtEldUnitsResponse
{
    public List<TtEldUnitData>? Data { get; init; }
    public TtEldPaginationMeta? Meta { get; init; }
}

internal record TtEldUnitData
{
    public string? Id { get; init; }
    public string? Vin { get; init; }
    [JsonPropertyName("truck_number")] public string? TruckNumber { get; init; }
    public TtEldDriverRef? Driver { get; init; }
    public TtEldDriverRef? Codriver { get; init; }
}

internal record TtEldDriverRef
{
    public string? Id { get; init; }
    [JsonPropertyName("first_name")] public string? FirstName { get; init; }
    [JsonPropertyName("second_name")] public string? SecondName { get; init; }
}

internal record TtEldTrackingV2Response
{
    public List<TtEldTrackingUnit>? Units { get; init; }
}

internal record TtEldTrackingUnit
{
    [JsonPropertyName("truck_number")] public string? TruckNumber { get; init; }
    public string? Vin { get; init; }
    public TtEldCoordinates? Coordinates { get; init; }
    public string? Timestamp { get; init; }
}

internal record TtEldCoordinates
{
    public double Lat { get; init; }
    public double Lng { get; init; }
}

internal record TtEldTrackingPoint
{
    public string? Address { get; init; }
    public TtEldCoordinates? Coordinates { get; init; }
    public int? Rotation { get; init; }
    public int? Speed { get; init; }
    public string? DriverId { get; init; }
    public int? Odometer { get; init; }
    public string? Date { get; init; }
}
