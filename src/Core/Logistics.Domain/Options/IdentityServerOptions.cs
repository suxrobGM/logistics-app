namespace Logistics.Domain.Options;

public sealed class IdentityServerOptions
{
    public const string SectionName = "IdentityServer";

    public string Authority { get; set; } = "http://localhost:7001";

    /// <summary>
    /// Public-facing URL for user-facing links (emails, redirects).
    /// Falls back to <see cref="Authority"/> when not set (e.g., local development).
    /// </summary>
    public string? ExternalAuthority { get; set; }

    public string UserFacingAuthority => ExternalAuthority ?? Authority;
}
