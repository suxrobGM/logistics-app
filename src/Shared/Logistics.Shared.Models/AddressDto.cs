namespace Logistics.Shared.Models;

public record AddressDto
{
    public required string Line1 { get; set; }
    public string? Line2 { get; set; }
    public required string City { get; set; }
    public required string ZipCode { get; set; }
    public required string State { get; set; }
    public required string Country { get; set; }

    public static AddressDto Empty() => new()
    {
        Line1 = string.Empty,
        Line2 = string.Empty,
        City = string.Empty,
        ZipCode = string.Empty,
        State = string.Empty,
        Country = string.Empty
    };
}
