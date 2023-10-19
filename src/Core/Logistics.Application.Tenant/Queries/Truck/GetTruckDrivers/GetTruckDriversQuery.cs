using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetTruckDriversQuery : SearchableQuery, IRequest<PagedResponseResult<TruckDriversDto>>
{
}
