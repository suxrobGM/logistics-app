using System.ComponentModel.DataAnnotations.Schema;
using Logistics.Domain.Core;

namespace Logistics.Domain.ValueObjects;

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
        Region = "NULL",
        Country = "NULL"
    };
    
    public required string Line1 { get; init; }
    public string? Line2 { get; init; }
    public required string City { get; init; }
    public required string ZipCode { get; init; }
    public required string Region { get; init; }
    public required string Country { get; init; }

    public bool IsNull() => this == NullAddress;
    public bool IsNotNull() => !IsNull();
    
    
    // protected override IEnumerable<object?> GetEqualityComponents()
    // {
    //     yield return Line1;
    //     yield return Line2;
    //     yield return City;
    //     yield return ZipCode;
    //     yield return Region;
    //     yield return Country;
    // }
}
