using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

[RequiresFeature(TenantFeature.LoadBoard)]
public class DeleteLoadBoardConfigurationCommand : ICommand
{
    public Guid Id { get; set; }
}
