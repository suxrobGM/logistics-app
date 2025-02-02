using Logistics.Shared;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateInvoiceCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal? PaymentAmount { get; set; }
}
