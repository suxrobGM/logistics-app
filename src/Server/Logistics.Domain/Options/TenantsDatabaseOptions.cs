namespace Logistics.Domain.Options;

public class TenantsDatabaseOptions
{
    public string? DatabaseNameTemplate { get; set; }
    public string? DatabaseHost { get; set; }
    public string? DatabaseUserId { get; set; }
    public string? DatabasePassword { get; set; }
}
