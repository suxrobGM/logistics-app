using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics.Domain.Primitives.ValueObjects;

[ComplexType]
public record Address
{
    // Temporary workaround for handling null values
    public static readonly Address NullAddress = new()
    {
        Line1 = "NULL",
        Line2 = "NULL",
        City = "NULL",
        ZipCode = "NULL",
        State = "NULL",
        Country = "NULL"
    };
    
    public required string Line1 { get; set; }
    public string? Line2 { get; set; }
    public required string City { get; set; }
    public required string ZipCode { get; set; }
    public required string State { get; set; }
    public required string Country { get; set; }

    public bool IsNull() => this == NullAddress;
    public bool IsNotNull() => !IsNull();
}
