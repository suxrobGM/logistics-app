using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

public class UpdateInvoiceCommand : IAppRequest
{
    public Guid Id { get; set; }
    public InvoiceStatus? InvoiceStatus { get; set; }
}
