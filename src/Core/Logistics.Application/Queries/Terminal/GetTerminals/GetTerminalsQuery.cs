using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetTerminalsQuery : SearchableQuery, IAppRequest<PagedResult<TerminalDto>>
{
    public TerminalType? Type { get; set; }
    public string? CountryCode { get; set; }
}
