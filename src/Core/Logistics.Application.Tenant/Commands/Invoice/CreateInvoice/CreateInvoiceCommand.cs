using Logistics.Shared.Enums;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class CreateInvoiceCommand : IRequest<ResponseResult>
{
    public string CustomerId { get; set; } = default!;
    public string LoadId { get; set; } = default!;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal PaymentAmount { get; set; }
}
