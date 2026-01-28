using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetCustomersQuery : SearchableQuery, IAppRequest<PagedResult<CustomerDto>>;
