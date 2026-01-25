namespace Logistics.Domain.Options;

public sealed class CustomerPortalOptions
{
    public const string SectionName = "CustomerPortal";

    public string BaseUrl { get; set; } = "http://localhost:7004";
}
