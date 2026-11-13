using Logistics.Infrastructure.Integrations.Eld.Providers.Geotab;
using Logistics.Infrastructure.Integrations.Eld.Providers.Motive;
using Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;
using Logistics.Infrastructure.Integrations.Eld.Providers.TtEld;

namespace Logistics.Infrastructure.Integrations.Eld;

public record EldOptions
{
    public const string SectionName = "Eld";
    public SamsaraOptions Samsara { get; set; } = new();
    public MotiveOptions Motive { get; set; } = new();
    public TtEldOptions TtEld { get; set; } = new();
    public GeotabOptions Geotab { get; set; } = new();
}
