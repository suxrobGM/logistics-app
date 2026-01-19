namespace Logistics.Infrastructure.Services.Truckstop;

internal record TruckstopTokenResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
}

internal record TruckstopSearchResponse
{
    public List<TruckstopLoad>? Loads { get; set; }
    public int TotalCount { get; set; }
}

internal record TruckstopLoad
{
    public string? LoadId { get; set; }
    public TruckstopLocation? Origin { get; set; }
    public TruckstopLocation? Destination { get; set; }
    public decimal? Rate { get; set; }
    public decimal? RatePerMile { get; set; }
    public int? Miles { get; set; }
    public int? Weight { get; set; }
    public int? Length { get; set; }
    public DateTime? PickupDate { get; set; }
    public DateTime? PickupDateEnd { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DateTime? DeliveryDateEnd { get; set; }
    public string? EquipmentType { get; set; }
    public string? Commodity { get; set; }
    public TruckstopBroker? Broker { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

internal record TruckstopLocation
{
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? PostalCode { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

internal record TruckstopBroker
{
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? McNumber { get; set; }
}

internal record TruckstopBookingResponse
{
    public string? ConfirmationNumber { get; set; }
}

internal record TruckstopPostTruckResponse
{
    public string? TruckId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

internal record TruckstopTrucksResponse
{
    public List<TruckstopTruck>? Trucks { get; set; }
}

internal record TruckstopTruck
{
    public string? TruckId { get; set; }
    public TruckstopLocation? Origin { get; set; }
    public string? EquipmentType { get; set; }
    public DateTime? AvailableDate { get; set; }
    public DateTime? AvailableDateEnd { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

internal record TruckstopWebhookPayload
{
    public string? Event { get; set; }
    public string? LoadId { get; set; }
}
