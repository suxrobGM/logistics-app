using Logistics.Domain.ValueObjects;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class UpdatePayrollCommand : IRequest<Result>
{
    public string Id { get; set; } = default!;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? EmployeeId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public Address? PaymentBillingAddress { get; set; }
}
