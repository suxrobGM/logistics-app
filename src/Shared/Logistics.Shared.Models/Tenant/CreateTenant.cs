namespace Logistics.Shared.Models;

public record CreateTenant
{
    public string? Name { get; set; }
    public string? CompanyName { get; set; }
}
