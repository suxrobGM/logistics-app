namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;

internal record DatSearchResponse
{
    public List<DatLoad>? Loads { get; set; }
    public int TotalCount { get; set; }
}

internal record DatLoad
{
    public string? Id { get; set; }
    public DatLocation? Origin { get; set; }
    public DatLocation? Destination { get; set; }
    public decimal? RatePerMile { get; set; }
    public decimal? TotalRate { get; set; }
    public int? Distance { get; set; }
    public int? Weight { get; set; }
    public int? Length { get; set; }
    public DateTime? PickupDateStart { get; set; }
    public DateTime? PickupDateEnd { get; set; }
    public DateTime? DeliveryDateStart { get; set; }
    public DateTime? DeliveryDateEnd { get; set; }
    public string? EquipmentType { get; set; }
    public string? Commodity { get; set; }
    public DatBroker? Broker { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

internal record DatLocation
{
    public string? City { get; set; }
    public string? State { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

internal record DatBroker
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? McNumber { get; set; }
}

internal record DatBookingResponse
{
    public string? ConfirmationId { get; set; }
}

internal record DatPostTruckResponse
{
    public string? PostId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

internal record DatTrucksResponse
{
    public List<DatTruck>? Trucks { get; set; }
}

internal record DatTruck
{
    public string? Id { get; set; }
    public DatLocation? Origin { get; set; }
    public string? EquipmentType { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

internal record DatWebhookPayload
{
    public string? EventType { get; set; }
    public string? LoadId { get; set; }
}
