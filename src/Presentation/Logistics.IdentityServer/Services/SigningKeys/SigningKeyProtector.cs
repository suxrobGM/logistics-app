using System.Security.Cryptography;
using System.Text.Json;

using Logistics.Domain.Entities;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;

namespace Logistics.IdentityServer.Services.SigningKeys;

/// <summary>
///     Turns a <see cref="SigningKey"/> row into signing credentials and back. The key material is
///     protected with the master DB key ring, so it is worthless if the table alone leaks.
/// </summary>
public class SigningKeyProtector(IDataProtectionProvider dataProtectionProvider)
{
    private const string Purpose = "LogisticsX.SigningKeys";

    // RSAParameters exposes its members as fields, so the default property-only serializer
    // would silently write an empty object.
    private static readonly JsonSerializerOptions SerializerOptions = new() { IncludeFields = true };

    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(Purpose);

    public SigningKey Create()
    {
        using var rsa = RSA.Create(2048);

        return new SigningKey
        {
            Created = DateTime.UtcNow,
            Algorithm = SecurityAlgorithms.RsaSha256,
            Data = _protector.Protect(
                JsonSerializer.Serialize(rsa.ExportParameters(true), SerializerOptions))
        };
    }

    public SigningCredentials Unprotect(SigningKey key)
    {
        var parameters = JsonSerializer.Deserialize<RSAParameters>(
            _protector.Unprotect(key.Data), SerializerOptions);

        var rsa = RSA.Create();
        rsa.ImportParameters(parameters);

        // The key id is the kid in JWKS and in every token header, so it must be the row id.
        return new SigningCredentials(new RsaSecurityKey(rsa) { KeyId = key.Id.ToString("N") }, key.Algorithm);
    }
}
