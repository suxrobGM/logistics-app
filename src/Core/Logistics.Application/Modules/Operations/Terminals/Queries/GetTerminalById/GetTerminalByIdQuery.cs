using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Terminals.Queries;

public class GetTerminalByIdQuery : IQuery<Result<TerminalDto>>, IHaveId
{
    public Guid Id { get; set; }
}
