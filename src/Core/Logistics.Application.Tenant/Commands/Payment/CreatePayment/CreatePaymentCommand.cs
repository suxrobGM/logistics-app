using Logistics.Shared.Enums;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class CreatePaymentCommand : IRequest<ResponseResult>
{
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public PaymentFor PaymentFor { get; set; }
    public string? Comment { get; set; }
}
