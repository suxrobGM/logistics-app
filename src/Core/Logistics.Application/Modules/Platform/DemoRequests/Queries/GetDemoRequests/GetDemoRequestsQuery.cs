using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DemoRequests.Queries;

public sealed class GetDemoRequestsQuery : SearchableQuery, IQuery<PagedResult<DemoRequestDto>>
{
}
