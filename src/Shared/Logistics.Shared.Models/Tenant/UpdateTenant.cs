namespace Logistics.Shared.Models;

public record UpdateTenant
{
    public string? Id { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
}
