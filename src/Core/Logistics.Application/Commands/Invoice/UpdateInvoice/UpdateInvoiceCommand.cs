using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

public class UpdateInvoiceCommand : ICommand
{
    public Guid Id { get; set; }
    public InvoiceStatus? InvoiceStatus { get; set; }
}
