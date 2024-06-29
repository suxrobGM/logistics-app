using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class UpdateInvoiceCommand : IRequest<Result>
{
    public string Id { get; set; } = default!;
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal? PaymentAmount { get; set; }
}
