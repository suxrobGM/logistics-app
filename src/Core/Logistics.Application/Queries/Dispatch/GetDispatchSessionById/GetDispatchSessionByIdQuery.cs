using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public sealed class GetDispatchSessionByIdQuery : IAppRequest<Result<DispatchSessionDto>>
{
    public Guid SessionId { get; set; }
}
