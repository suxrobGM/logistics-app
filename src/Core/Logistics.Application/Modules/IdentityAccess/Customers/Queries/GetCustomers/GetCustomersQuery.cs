using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Queries;

public class GetCustomersQuery : SearchableQuery, IQuery<PagedResult<CustomerDto>>;
