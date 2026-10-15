using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

/// <summary>
/// Who a factored invoice must be paid to instead of the carrier.
/// </summary>
public record RemitToDto
{
    public required string Name { get; set; }
    public Address? Address { get; set; }
}
