namespace Logistics.Shared.Models;

public record CreateTenantCommand
{
    public string? Name { get; set; }
    public string? CompanyName { get; set; }
}
