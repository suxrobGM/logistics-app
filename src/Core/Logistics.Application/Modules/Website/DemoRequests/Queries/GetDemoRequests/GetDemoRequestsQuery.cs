using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Website.DemoRequests.Queries;

public sealed class GetDemoRequestsQuery : SearchableQuery, IQuery<PagedResult<DemoRequestDto>>
{
}
