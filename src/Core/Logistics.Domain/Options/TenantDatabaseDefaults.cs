namespace Logistics.Domain.Options;

/// <summary>
/// Host, credentials, and the name template for tenant databases. A tenant stores only its
/// database name; the connection string is built from these settings at runtime.
/// </summary>
public record TenantDatabaseDefaults
{
    public const string SectionName = "TenantDatabaseDefaults";

    /// <summary>
    /// Template like <c>"{tenant}_logisticsx"</c> - <c>{tenant}</c> is replaced with the tenant slug.
    /// </summary>
    public string? NameTemplate { get; set; }
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? UserId { get; set; }
    public string? Password { get; set; }

    public string BuildConnectionString(string databaseName)
    {
        return $"Host={Host}; Port={Port}; Database={databaseName}; Username={UserId}; Password={Password}";
    }
}
