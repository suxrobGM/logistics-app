using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class UpdatePayrollInvoiceCommand : IAppRequest
{
    public Guid Id { get; set; }
    public Guid? EmployeeId { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
}
