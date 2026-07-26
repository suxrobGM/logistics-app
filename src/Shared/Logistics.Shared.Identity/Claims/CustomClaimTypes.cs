namespace Logistics.Shared.Identity.Claims;

public static class CustomClaimTypes
{
    public const string Permission = "permission";
    public const string Role = "role";
    public const string Tenant = "tenant";

    /// <summary>
    /// OIDC subject. The API's JWT handler maps it onto <c>ClaimTypes.NameIdentifier</c>; the
    /// IdentityServer's cookie identity carries it raw, so readers must accept both.
    /// </summary>
    public const string Subject = "sub";
}
