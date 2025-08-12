using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetCustomerByIdQuery : IAppRequest<Result<CustomerDto>>
{
    public Guid Id { get; set; }
}
