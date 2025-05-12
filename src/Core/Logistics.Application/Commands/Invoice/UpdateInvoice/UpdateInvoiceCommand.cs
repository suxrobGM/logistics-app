using Logistics.Shared.Models;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateInvoiceCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
    public InvoiceStatus? InvoiceStatus { get; set; }
}
