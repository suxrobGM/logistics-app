using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class SetDefaultPaymentMethodCommand : IRequest<Result>
{
    public string PaymentMethodId { get; set; } = null!;
}
