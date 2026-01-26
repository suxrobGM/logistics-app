namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.OneTwo3;

internal record OneTwo3SearchResponse
{
    public List<OneTwo3Load>? Loads { get; set; }
    public int TotalCount { get; set; }
    public int RemainingSearches { get; set; }
}

internal record OneTwo3Load
{
    public string? Id { get; set; }
    public OneTwo3Location? Origin { get; set; }
    public OneTwo3Location? Destination { get; set; }
    public decimal? TotalRate { get; set; }
    public decimal? RatePerMile { get; set; }
    public int? Miles { get; set; }
    public int? Weight { get; set; }
    public int? Length { get; set; }
    public DateTime? PickupStart { get; set; }
    public DateTime? PickupEnd { get; set; }
    public DateTime? DeliveryStart { get; set; }
    public DateTime? DeliveryEnd { get; set; }
    public string? Equipment { get; set; }
    public string? Commodity { get; set; }
    public OneTwo3Poster? Poster { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

internal record OneTwo3Location
{
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
}

internal record OneTwo3Poster
{
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? McNumber { get; set; }
}

internal record OneTwo3BookingResponse
{
    public string? ReferenceId { get; set; }
}

internal record OneTwo3PostTruckResponse
{
    public string? TruckPostId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

internal record OneTwo3TrucksResponse
{
    public List<OneTwo3Truck>? Trucks { get; set; }
}

internal record OneTwo3Truck
{
    public string? Id { get; set; }
    public OneTwo3Location? Origin { get; set; }
    public string? Equipment { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

internal record OneTwo3WebhookPayload
{
    public string? EventType { get; set; }
    public string? LoadId { get; set; }
}
