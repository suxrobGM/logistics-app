namespace Logistics.Application.Abstractions.ProductLicense;

/// <summary>
/// Wire contract of a license key: an ES256 JWT issued by the author. Shared by the validator,
/// the issuer tool, and the tests.
/// </summary>
public static class ProductLicenseClaims
{
    public const string Issuer = "LogisticsX";
    public const string Audience = "logisticsx";

    /// <summary>Legal entity the license was sold to.</summary>
    public const string Licensee = "licensee";

    /// <summary>A <c>ProductLicenseTier</c> name.</summary>
    public const string Tier = "tier";

    /// <summary>Optional tenant cap for the Hosted tier.</summary>
    public const string MaxTenants = "max_tenants";
}
