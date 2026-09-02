using System.ComponentModel;
using System.Globalization;
using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Domain.Primitives.Enums;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Logistics.LicenseIssuer.Commands;

internal sealed class IssueCommand : Command<IssueCommand.Settings>
{
    /// <summary>
    /// Options are nullable and checked in <see cref="Settings.Validate"/> rather than declared
    /// required, so a missing one reports the same message on every Spectre version.
    /// </summary>
    internal sealed class Settings : CommandSettings
    {
        [CommandOption("--licensee <NAME>")]
        [Description("Legal entity the license is granted to.")]
        public string? Licensee { get; init; }

        [CommandOption("--tier <TIER>")]
        [Description("InternalUse, Hosted or PerpetualSource.")]
        public ProductLicenseTier? Tier { get; init; }

        [CommandOption("--expires <DATE>")]
        [Description("Expiry date in yyyy-MM-dd form, taken as UTC midnight.")]
        public DateOnly? Expires { get; init; }

        [CommandOption("--max-tenants <N>")]
        [Description("Tenant cap for the Hosted tier.")]
        public int? MaxTenants { get; init; }

        [CommandOption("--key-id <ID>")]
        [Description("Signing key id written to the JWT header. Defaults to the current yyyy-MM.")]
        public string? KeyId { get; init; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(Licensee))
            {
                return ValidationResult.Error("--licensee is required.");
            }

            if (Tier is null)
            {
                return ValidationResult.Error("--tier must be InternalUse, Hosted or PerpetualSource.");
            }

            if (Expires is null)
            {
                return ValidationResult.Error("--expires must be a date in yyyy-MM-dd form.");
            }

            return MaxTenants is <= 0
                ? ValidationResult.Error("--max-tenants must be a positive integer.")
                : ValidationResult.Success();
        }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        using var privateKey = SigningKeys.LoadPrivateKey();
        if (privateKey is null)
        {
            Console.Error.WriteLine($"{SigningKeys.PrivateKeyVariable} is not set.");
            return 1;
        }

        var keyId = settings.KeyId ?? DateTime.UtcNow.ToString("yyyy-MM", CultureInfo.InvariantCulture);

        Console.WriteLine(ProductLicenseToken.Sign(
            ProductLicenseToken.CreateSigningCredentials(privateKey, keyId),
            settings.Licensee!,
            settings.Tier!.Value.ToString(),
            settings.Expires!.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            settings.MaxTenants));
        return 0;
    }
}
