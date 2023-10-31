using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetPayrollByIdQuery : IRequest<ResponseResult<PayrollDto>>
{
    public string? Id { get; set; }
}
