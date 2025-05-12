using Logistics.Domain.ValueObjects;
using Logistics.Shared.Models;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdatePayrollInvoiceCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public string? EmployeeId { get; set; }
}
