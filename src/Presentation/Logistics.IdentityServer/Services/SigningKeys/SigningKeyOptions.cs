namespace Logistics.IdentityServer.Services.SigningKeys;

public class SigningKeyOptions
{
    public const string SectionName = "SigningKeys";

    /// <summary>How old the newest key may get before a replacement is published.</summary>
    public TimeSpan Rotation { get; set; } = TimeSpan.FromDays(90);

    /// <summary>
    ///     How long a new key is published in JWKS before it starts signing. Relying parties cache
    ///     the JWKS document, so signing with a key the moment it appears hands them an unknown
    ///     <c>kid</c> and 401s every caller until their cache refreshes.
    /// </summary>
    public TimeSpan Propagation { get; set; } = TimeSpan.FromDays(2);

    /// <summary>How long a retired key stays in JWKS so tokens it signed still validate.</summary>
    public TimeSpan Retention { get; set; } = TimeSpan.FromDays(97);

    /// <summary>JWKS and every token issuance read the key set, so it is cached between reads.</summary>
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>Refresh-token grants that were already redeemed are deleted after this long.</summary>
    public TimeSpan UsedGrantRetention { get; set; } = TimeSpan.FromDays(1);
}
