namespace Logistics.Domain.Options;

public record TenantsDatabaseOptions
{
    public const string SectionName = "TenantsDatabaseConfig";
    public string? DatabaseNameTemplate { get; set; }
    public string? DatabaseHost { get; set; }
    public int? DatabasePort { get; set; }
    public string? DatabaseUserId { get; set; }
    public string? DatabasePassword { get; set; }
}
