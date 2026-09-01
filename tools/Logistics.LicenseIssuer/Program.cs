using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Domain.Primitives.Enums;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

// Signs and inspects LogisticsX commercial license keys. The private key comes from the
// LOGISTICSX_LICENSE_PRIVATE_KEY environment variable and never touches the repo.

const string PrivateKeyVariable = "LOGISTICSX_LICENSE_PRIVATE_KEY";

return args switch
{
    ["keygen"] => KeyGen(),
    ["issue", .. var rest] => Issue(ParseOptions(rest)),
    ["inspect", var token, .. var rest] => await InspectAsync(token, ParseOptions(rest)),
    _ => Usage()
};

static int Usage()
{
    Console.Error.WriteLine("""
        Usage:
          keygen
              Prints a new P-256 key pair. Keep the private key in a password manager and paste
              the public key into ProductLicensePublicKey.cs.

          issue --licensee "<company>" --tier <InternalUse|Hosted|PerpetualSource> --expires <yyyy-MM-dd>
                [--max-tenants <n>] [--key-id <id>]
              Prints a signed license key. Reads LOGISTICSX_LICENSE_PRIVATE_KEY.

          inspect <key> --public-key <spki-base64>
              Validates a key against a public key and prints its claims.
        """);
    return 1;
}

static int KeyGen()
{
    using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    Console.WriteLine($"{PrivateKeyVariable}={Convert.ToBase64String(ecdsa.ExportPkcs8PrivateKey())}");
    Console.WriteLine();
    Console.WriteLine($"Public key (ProductLicensePublicKey.SpkiBase64):");
    Console.WriteLine(Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo()));
    return 0;
}

static int Issue(Dictionary<string, string> options)
{
    var privateKey = Environment.GetEnvironmentVariable(PrivateKeyVariable);
    if (string.IsNullOrWhiteSpace(privateKey))
    {
        Console.Error.WriteLine($"{PrivateKeyVariable} is not set.");
        return 1;
    }

    if (!options.TryGetValue("licensee", out var licensee) || string.IsNullOrWhiteSpace(licensee))
    {
        Console.Error.WriteLine("--licensee is required.");
        return 1;
    }

    if (!options.TryGetValue("tier", out var tierName)
        || !Enum.TryParse<ProductLicenseTier>(tierName, ignoreCase: true, out var tier))
    {
        Console.Error.WriteLine("--tier must be InternalUse, Hosted or PerpetualSource.");
        return 1;
    }

    if (!options.TryGetValue("expires", out var expiresText)
        || !DateTime.TryParseExact(expiresText, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var expires))
    {
        Console.Error.WriteLine("--expires must be a date in yyyy-MM-dd form.");
        return 1;
    }

    var claims = new Dictionary<string, object>
    {
        [ProductLicenseClaims.Licensee] = licensee,
        [ProductLicenseClaims.Tier] = tier.ToString()
    };

    if (options.TryGetValue("max-tenants", out var maxTenantsText))
    {
        if (!int.TryParse(maxTenantsText, out var maxTenants) || maxTenants <= 0)
        {
            Console.Error.WriteLine("--max-tenants must be a positive integer.");
            return 1;
        }

        claims[ProductLicenseClaims.MaxTenants] = maxTenants;
    }

    using var ecdsa = ECDsa.Create();
    ecdsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
    var signingKey = new ECDsaSecurityKey(ecdsa)
    {
        KeyId = options.GetValueOrDefault("key-id", DateTime.UtcNow.ToString("yyyy-MM", CultureInfo.InvariantCulture))
    };

    var now = DateTime.UtcNow;
    var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
    {
        Issuer = ProductLicenseClaims.Issuer,
        Audience = ProductLicenseClaims.Audience,
        IssuedAt = now,
        NotBefore = now,
        Expires = expires,
        Claims = claims,
        SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.EcdsaSha256)
    });

    Console.WriteLine(token);
    return 0;
}

static async Task<int> InspectAsync(string token, Dictionary<string, string> options)
{
    if (!options.TryGetValue("public-key", out var publicKey))
    {
        Console.Error.WriteLine("--public-key is required.");
        return 1;
    }

    using var ecdsa = ECDsa.Create();
    ecdsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);

    var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
    {
        ValidIssuer = ProductLicenseClaims.Issuer,
        ValidAudience = ProductLicenseClaims.Audience,
        IssuerSigningKey = new ECDsaSecurityKey(ecdsa),
        ValidAlgorithms = [SecurityAlgorithms.EcdsaSha256],
        RequireSignedTokens = true,
        RequireExpirationTime = true,
        ValidateLifetime = false
    });

    if (!result.IsValid)
    {
        Console.Error.WriteLine($"Invalid: {result.Exception?.GetType().Name}: {result.Exception?.Message}");
        return 1;
    }

    var jwt = (JsonWebToken)result.SecurityToken;
    Console.WriteLine($"kid:     {jwt.Kid}");
    Console.WriteLine($"expires: {jwt.ValidTo:u}{(jwt.ValidTo < DateTime.UtcNow ? " (EXPIRED)" : "")}");
    Console.WriteLine(JsonSerializer.Serialize(
        JsonDocument.Parse(jwt.EncodedPayload.Length == 0 ? "{}" : Base64UrlEncoder.Decode(jwt.EncodedPayload)),
        new JsonSerializerOptions { WriteIndented = true }));
    return 0;
}

static Dictionary<string, string> ParseOptions(string[] rest)
{
    var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i + 1 < rest.Length; i += 2)
    {
        if (rest[i].StartsWith("--", StringComparison.Ordinal))
        {
            options[rest[i][2..]] = rest[i + 1];
        }
    }

    return options;
}
