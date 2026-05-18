using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Terminals.Commands;

public class DeleteTerminalCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
