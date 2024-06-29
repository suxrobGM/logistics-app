using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetPayrollByIdQuery : IRequest<Result<PayrollDto>>
{
    public string Id { get; set; } = default!;
}
