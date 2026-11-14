using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics.Domain.Primitives.ValueObjects;

[ComplexType]
public record Address
{
    public required string Line1 { get; set; }
    public string? Line2 { get; set; }
    public required string City { get; set; }
    public required string ZipCode { get; set; }
    public required string State { get; set; }
    public required string Country { get; set; }

    /// <summary>One line, e.g. "600 Woodward Avenue, Detroit, Michigan, 48226, US". The record default lists property names.</summary>
    public override string ToString()
    {
        string?[] parts = [Line1, Line2, City, State, ZipCode, Country];
        return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}
