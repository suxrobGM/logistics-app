using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class RejectPayrollInvoiceCommand : IAppRequest
{
    public Guid Id { get; set; }
    public required string Reason { get; set; }
}
