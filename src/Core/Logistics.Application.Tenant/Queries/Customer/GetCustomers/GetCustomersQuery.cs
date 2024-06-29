using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetCustomersQuery : SearchableQuery, IRequest<PagedResult<CustomerDto>>
{
}
