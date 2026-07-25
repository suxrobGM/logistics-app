using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

[RequiresFeature(TenantFeature.IntermodalContainers)]
public class DeleteContainerCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
