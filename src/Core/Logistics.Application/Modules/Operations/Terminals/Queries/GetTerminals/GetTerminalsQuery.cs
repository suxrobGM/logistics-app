using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Terminals.Queries;

public class GetTerminalsQuery : SearchableQuery, IQuery<PagedResult<TerminalDto>>
{
    public TerminalType? Type { get; set; }
    public string? CountryCode { get; set; }
}
