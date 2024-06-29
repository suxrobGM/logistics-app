using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class UpdateCustomerCommand : IRequest<Result>
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
}
