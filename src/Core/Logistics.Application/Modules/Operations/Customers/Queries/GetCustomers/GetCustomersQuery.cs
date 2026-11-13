using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Customers.Queries;

public class GetCustomersQuery : SearchableQuery, IQuery<PagedResult<CustomerDto>>;
