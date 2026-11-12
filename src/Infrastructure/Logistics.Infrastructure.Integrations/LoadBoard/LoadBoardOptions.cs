using Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;

namespace Logistics.Infrastructure.Integrations.LoadBoard;

public record LoadBoardOptions
{
    public const string SectionName = "LoadBoard";
    public DatOptions? Dat { get; set; }
}
