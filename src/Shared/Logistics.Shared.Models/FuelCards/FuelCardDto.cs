using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record FuelCardDto
{
    public Guid Id { get; set; }
    public FuelCardProviderType ProviderType { get; set; }
    public required string ExternalCardId { get; set; }
    public string? UnitNumber { get; set; }
    public Guid? TruckId { get; set; }
    public string? TruckNumber { get; set; }
    public bool IsActive { get; set; }
}
