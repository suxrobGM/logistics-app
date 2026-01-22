using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetDemoRequestsQuery : SearchableQuery, IAppRequest<PagedResult<DemoRequestDto>>
{
}
