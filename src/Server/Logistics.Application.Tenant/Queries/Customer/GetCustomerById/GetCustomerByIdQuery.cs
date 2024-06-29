using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetCustomerByIdQuery : IRequest<Result<CustomerDto>>
{
    public string Id { get; set; } = default!;
}
