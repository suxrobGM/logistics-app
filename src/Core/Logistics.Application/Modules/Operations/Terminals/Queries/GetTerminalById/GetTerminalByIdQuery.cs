using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Terminals.Queries;

[RequiresFeature(TenantFeature.IntermodalContainers)]
public class GetTerminalByIdQuery : IQuery<Result<TerminalDto>>, IHaveId
{
    public Guid Id { get; set; }
}
