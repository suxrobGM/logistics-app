using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class DeleteEmployeeCommand : IRequest<Result>
{
    public string UserId { get; set; } = default!;
}
