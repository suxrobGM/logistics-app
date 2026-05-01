namespace Logistics.Infrastructure.Persistence.Options;

public record TenantDbContextOptions
{
    /// <summary>
    /// ConnectionStrings entry used for the runtime non-HTTP fallback (background jobs,
    /// IDataProtector, etc.). Defaults to <c>UsTenantDatabase</c> - the US demo tenant.
    /// </summary>
    public string DefaultTenantDbConnectionSection { get; set; } = "UsTenantDatabase";
    public string? ConnectionString { get; set; }
}
