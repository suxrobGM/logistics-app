namespace Logistics.Infrastructure.Integrations.Eld;

public record EldOptions
{
    public const string SectionName = "Eld";
    public SamsaraOptions? Samsara { get; set; }
    public MotiveOptions? Motive { get; set; }
    public TtEldOptions? TtEld { get; set; }
    public GeotabOptions? Geotab { get; set; }
}

public record SamsaraOptions
{
    public string BaseUrl { get; set; } = "https://api.samsara.com";
}

public record MotiveOptions
{
    public string BaseUrl { get; set; } = "https://api.keeptruckin.com/v1";
}

public record TtEldOptions
{
    public string BaseUrl { get; set; } = "https://read.tteld.com";
}

/// <summary>
/// Options for the Geotab MyGeotab integration. <see cref="BaseUrl"/> is the
/// initial server hint; MyGeotab redirects to a federated server (e.g. my3.geotab.com)
/// returned by the authentication call.
/// </summary>
public record GeotabOptions
{
    public string BaseUrl { get; set; } = "https://my.geotab.com";
}
