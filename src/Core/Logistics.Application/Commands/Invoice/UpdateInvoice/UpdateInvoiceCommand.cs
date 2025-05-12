using Logistics.Shared.Models;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateInvoiceCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public InvoiceStatus? InvoiceStatus { get; set; }
}
