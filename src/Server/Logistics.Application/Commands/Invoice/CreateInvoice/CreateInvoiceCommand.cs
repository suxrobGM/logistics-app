using Logistics.Shared;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateInvoiceCommand : IRequest<Result>
{
    public string CustomerId { get; set; } = null!;
    public string LoadId { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal PaymentAmount { get; set; }
}
