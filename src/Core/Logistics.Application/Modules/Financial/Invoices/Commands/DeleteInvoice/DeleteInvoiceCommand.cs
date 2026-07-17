using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

public class DeleteInvoiceCommand : ICommand, IHaveId
{
    public Guid Id { get; set; }
}
