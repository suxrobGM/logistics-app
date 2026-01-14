namespace Logistics.Infrastructure.Services;

public record EldOptions
{
    public SamsaraOptions? Samsara { get; set; }
    public MotiveOptions? Motive { get; set; }
}

public record SamsaraOptions
{
    public string? BaseUrl { get; set; } = "https://api.samsara.com";
}

public record MotiveOptions
{
    public string? BaseUrl { get; set; } = "https://api.keeptruckin.com/v1";
}
