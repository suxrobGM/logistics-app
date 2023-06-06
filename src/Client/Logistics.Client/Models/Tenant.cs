namespace Logistics.Client.Models;

public record Tenant
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? ConnectionString { get; set; }
}
