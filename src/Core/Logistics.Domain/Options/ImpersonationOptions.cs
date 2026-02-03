namespace Logistics.Domain.Options;

/// <summary>
/// Configuration options for the admin impersonation feature.
/// </summary>
public sealed class ImpersonationOptions
{
    public const string SectionName = "Impersonation";

    /// <summary>
    /// Master password required for impersonation.
    /// Should be stored in Azure Key Vault or environment variables in production.
    /// </summary>
    public string MasterPassword { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in minutes (default: 5 minutes).
    /// </summary>
    public int TokenExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// URL of the TMS Portal for impersonation redirect.
    /// </summary>
    public string TmsPortalUrl { get; set; } = "http://localhost:7003";
}
