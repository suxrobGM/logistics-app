namespace Logistics.Domain.Options;

/// <summary>
/// Defaults applied when provisioning a new tenant database
/// (host, credentials, and the name template used by ITenantDatabaseService).
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
}
