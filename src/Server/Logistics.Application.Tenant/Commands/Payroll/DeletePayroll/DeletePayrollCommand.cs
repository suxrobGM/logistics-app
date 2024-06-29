using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class DeletePayrollCommand : IRequest<Result>
{
    public string Id { get; set; } = default!;
}
