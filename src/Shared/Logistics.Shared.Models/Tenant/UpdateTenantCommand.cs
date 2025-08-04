namespace Logistics.Shared.Models;

public record UpdateTenantCommand
{
    public Guid? Id { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
}
