using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Terminals.Commands;

[RequiresFeature(TenantFeature.IntermodalContainers)]
public class DeleteTerminalCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
