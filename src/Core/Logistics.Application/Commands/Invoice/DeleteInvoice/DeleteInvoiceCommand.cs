using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteInvoiceCommand : ICommand
{
    public Guid Id { get; set; }
}
