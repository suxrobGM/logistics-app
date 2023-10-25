using Logistics.Shared.Enums;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class UpdateInvoiceCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal? PaymentAmount { get; set; }
}
