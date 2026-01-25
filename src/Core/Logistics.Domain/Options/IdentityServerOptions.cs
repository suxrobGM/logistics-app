namespace Logistics.Domain.Options;

public sealed class IdentityServerOptions
{
    public const string SectionName = "IdentityServer";

    public string Authority { get; set; } = "http://localhost:7001";
}
