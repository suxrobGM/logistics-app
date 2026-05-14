using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetTerminalByIdQuery : IQuery<Result<TerminalDto>>
{
    public Guid Id { get; set; }
}
