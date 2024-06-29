using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class CreateInvoiceCommand : IRequest<Result>
{
    public string CustomerId { get; set; } = default!;
    public string LoadId { get; set; } = default!;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal PaymentAmount { get; set; }
}
