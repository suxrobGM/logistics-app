using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Customers.Queries;

public class GetCustomerByIdQuery : IQuery<Result<CustomerDto>>, IHaveId
{
    public Guid Id { get; set; }
}
