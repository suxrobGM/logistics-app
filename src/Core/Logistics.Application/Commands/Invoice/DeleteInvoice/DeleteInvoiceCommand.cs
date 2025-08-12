using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteInvoiceCommand : IAppRequest
{
    public Guid Id { get; set; }
}
