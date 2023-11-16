namespace Logistics.Shared.Models;

public class AddressDto
{
    public required string Line1 { get; set; }
    public string? Line2 { get; init; }
    public required string City { get; set; }
    public required string ZipCode { get; set; }
    public required string Region { get; set; }
    public required string Country { get; set; }
}
