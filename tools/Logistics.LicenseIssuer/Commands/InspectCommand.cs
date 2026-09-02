using System.ComponentModel;
using System.Text.Json;
using Logistics.Application.Abstractions.ProductLicense;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Logistics.LicenseIssuer.Commands;

internal sealed class InspectCommand : AsyncCommand<InspectCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<KEY>")]
        [Description("The signed license key to validate.")]
        public string Key { get; init; } = string.Empty;

        [CommandOption("--public-key <SPKI_BASE64>")]
        [Description("Public key to validate against, as base64 SubjectPublicKeyInfo.")]
        public string? PublicKey { get; init; }

        public override ValidationResult Validate() =>
            string.IsNullOrWhiteSpace(PublicKey)
                ? ValidationResult.Error("--public-key is required.")
                : ValidationResult.Success();
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        using var publicKey = SigningKeys.LoadPublicKey(settings.PublicKey!);

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(
            settings.Key, ProductLicenseToken.CreateValidationParameters(publicKey));

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
}
