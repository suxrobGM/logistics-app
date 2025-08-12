using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public class GetCustomerByIdQuery : IRequest<Result<CustomerDto>>
{
    public Guid Id { get; set; }
}
