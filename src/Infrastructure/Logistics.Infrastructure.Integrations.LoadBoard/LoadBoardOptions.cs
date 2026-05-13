using Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers.OneTwo3;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers.Truckstop;

namespace Logistics.Infrastructure.Integrations.LoadBoard;

public record LoadBoardOptions
{
    public const string SectionName = "LoadBoard";
    public DatOptions? Dat { get; set; }
    public TruckstopOptions? Truckstop { get; set; }
    public OneTwo3LoadboardOptions? OneTwo3Loadboard { get; set; }
}
