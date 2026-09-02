using System.Security.Cryptography;
using Spectre.Console.Cli;

namespace Logistics.LicenseIssuer.Commands;

internal sealed class KeygenCommand : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        Console.WriteLine($"{SigningKeys.PrivateKeyVariable}={Convert.ToBase64String(ecdsa.ExportPkcs8PrivateKey())}");
        Console.WriteLine();
        Console.WriteLine("Public key (ProductLicensePublicKey.SpkiBase64):");
        Console.WriteLine(Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo()));
        return 0;
    }
}
