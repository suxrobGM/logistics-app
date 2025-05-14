using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class CreatePayrollInvoiceCommand : IRequest<Result>
{
    public Guid EmployeeId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
