using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetTerminalByIdQuery : IAppRequest<Result<TerminalDto>>
{
    public Guid Id { get; set; }
}
