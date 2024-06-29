using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class DeleteCustomerCommand : IRequest<Result>
{
    public string Id { get; set; } = default!;
}
