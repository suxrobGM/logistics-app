using Logistics.Shared.Enums;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class UpdatePayrollCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? EmployeeId { get; set; }
    public PaymentMethod? Method { get; set; }
    public decimal? Amount { get; set; }
    public PaymentStatus? Status { get; set; }
}
