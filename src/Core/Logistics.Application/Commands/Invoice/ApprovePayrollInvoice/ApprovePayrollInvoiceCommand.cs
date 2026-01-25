using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class ApprovePayrollInvoiceCommand : IAppRequest
{
    public Guid Id { get; set; }
    public string? Notes { get; set; }
}
