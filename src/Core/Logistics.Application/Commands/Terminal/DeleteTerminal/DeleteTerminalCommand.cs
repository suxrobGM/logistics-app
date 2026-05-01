using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class DeleteTerminalCommand : IAppRequest<Result>
{
    public Guid Id { get; set; }
}
