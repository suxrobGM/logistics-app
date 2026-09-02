using Logistics.LicenseIssuer;
using Logistics.LicenseIssuer.Commands;
using Spectre.Console.Cli;

// Signs and inspects LogisticsX commercial license keys. The private key comes from the
// LOGISTICSX_LICENSE_PRIVATE_KEY environment variable and never touches the repo.

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("license");

    config.AddCommand<KeygenCommand>("keygen")
        .WithDescription("Print a new P-256 key pair. Keep the private key in a password manager "
                         + "and paste the public key into ProductLicensePublicKey.cs.");

    config.AddCommand<IssueCommand>("issue")
        .WithDescription($"Print a signed license key. Reads {SigningKeys.PrivateKeyVariable}.")
        .WithExample("issue", "--licensee", "\"Acme Freight\"", "--tier", "Hosted", "--expires", "2027-01-01");

    config.AddCommand<InspectCommand>("inspect")
        .WithDescription("Validate a key against a public key and print its claims.")
        .WithExample("inspect", "<key>", "--public-key", "<spki-base64>");
});

return app.Run(args);
