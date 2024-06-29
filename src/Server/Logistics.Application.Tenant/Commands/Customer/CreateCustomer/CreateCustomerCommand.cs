using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class CreateCustomerCommand : IRequest<Result>
{
    public string Name { get; set; } = default!;
}
