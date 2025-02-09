namespace Logistics.Infrastructure.EF.Options;

public record MasterDbContextOptions
{
    public string DbConnectionSection { get; set; } = "MasterDatabase";
    public string? ConnectionString { get; set; }
}