using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

/// <summary>
/// Signs in to the board with the stored credentials and reports the board's answer.
/// </summary>
[RequiresFeature(TenantFeature.LoadBoard)]
public class TestLoadBoardConnectionCommand : ICommand
{
    public Guid Id { get; set; }
}
