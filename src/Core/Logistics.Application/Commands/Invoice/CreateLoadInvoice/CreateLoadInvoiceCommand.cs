using Logistics.Shared.Models;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateLoadInvoiceCommand : IRequest<Result>
{
    public string CustomerId { get; set; } = null!;
    public string LoadId { get; set; } = null!;
    public PaymentMethodType PaymentMethod { get; set; }
    public decimal PaymentAmount { get; set; }
}
