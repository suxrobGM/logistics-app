using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

public class DeleteContainerCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
