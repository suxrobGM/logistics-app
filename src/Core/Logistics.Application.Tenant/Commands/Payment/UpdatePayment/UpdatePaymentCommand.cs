using Logistics.Shared.Enums;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class UpdatePaymentCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
    public PaymentMethod? Method { get; set; }
    public decimal? Amount { get; set; }
    public PaymentStatus? Status { get; set; }
    public PaymentFor? PaymentFor { get; set; }
    public string? Comment { get; set; }
}
