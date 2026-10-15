namespace Logistics.Shared.Models;

public record ImportPresetDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
}
