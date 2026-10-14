using System.Globalization;
using System.Security.Cryptography;
using Logistics.Application.Abstractions.Accounting;
using Microsoft.AspNetCore.DataProtection;

namespace Logistics.Infrastructure.Persistence.Services.Accounting;

public sealed class OAuthStateProtector(IDataProtectionProvider provider, TimeProvider timeProvider)
    : IOAuthStateProtector
{
    private const string Purpose = "Logistics.OAuthState.v2";
    private readonly IDataProtector _protector = provider.CreateProtector(Purpose);

    public string Protect(Guid tenantId, Guid userId)
    {
        var expiresAt = timeProvider.GetUtcNow().Add(IOAuthStateProtector.Lifetime).ToUnixTimeSeconds();
        var nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        return _protector.Protect($"{tenantId:N}|{userId:N}|{expiresAt}|{nonce}");
    }

    public OAuthState? TryUnprotect(string state)
    {
        string payload;
        try
        {
            payload = _protector.Unprotect(state);
        }
        catch (CryptographicException)
        {
            return null;
        }

        var parts = payload.Split('|');
        if (parts.Length != 4
            || !Guid.TryParseExact(parts[0], "N", out var tenantId)
            || !Guid.TryParseExact(parts[1], "N", out var userId)
            || !long.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var expiresAt)
            || timeProvider.GetUtcNow().ToUnixTimeSeconds() > expiresAt)
        {
            return null;
        }

        return new OAuthState(tenantId, userId);
    }
}
