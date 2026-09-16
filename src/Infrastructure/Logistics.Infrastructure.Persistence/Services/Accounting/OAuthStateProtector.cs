using Logistics.Application.Abstractions.Accounting;
using Microsoft.AspNetCore.DataProtection;

namespace Logistics.Infrastructure.Persistence.Services.Accounting;

public sealed class OAuthStateProtector(IDataProtectionProvider provider) : IOAuthStateProtector
{
    private const string Purpose = "Logistics.OAuthState.v1";
    private readonly IDataProtector _protector = provider.CreateProtector(Purpose);

    public string Protect(Guid tenantId) => _protector.Protect(tenantId.ToString());

    public Guid? TryUnprotect(string state)
    {
        try
        {
            return Guid.Parse(_protector.Unprotect(state));
        }
        catch (Exception ex) when (ex is System.Security.Cryptography.CryptographicException or FormatException)
        {
            return null;
        }
    }
}
