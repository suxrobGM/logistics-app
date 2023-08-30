namespace Logistics.Models;

public class OrganizationDto
{
    public required string TenantId { get; init; }
    public required string Name { get; init; }
    public required string DisplayName { get; init; }

    public override string ToString() => DisplayName;
}