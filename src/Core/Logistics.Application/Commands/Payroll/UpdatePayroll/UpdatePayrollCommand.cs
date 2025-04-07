using Logistics.Domain.ValueObjects;
using Logistics.Shared.Models;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdatePayrollCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? EmployeeId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public PaymentMethodType? PaymentMethod { get; set; }
    public Address? PaymentBillingAddress { get; set; }
}
