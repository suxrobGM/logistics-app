using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class UpdatePayrollInvoiceCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public Guid? EmployeeId { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
}
