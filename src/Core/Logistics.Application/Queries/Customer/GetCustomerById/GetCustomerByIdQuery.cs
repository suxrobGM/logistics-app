using Logistics.Shared.Models;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetCustomerByIdQuery : IRequest<Result<CustomerDto>>
{
    public string Id { get; set; } = null!;
}
