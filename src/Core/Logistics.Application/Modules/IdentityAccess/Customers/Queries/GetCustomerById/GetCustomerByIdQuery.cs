using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Queries;

public class GetCustomerByIdQuery : IQuery<Result<CustomerDto>>
{
    public Guid Id { get; set; }
}
