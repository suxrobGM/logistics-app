using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class CreatePayrollInvoiceCommand : IAppRequest
{
    public Guid EmployeeId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
