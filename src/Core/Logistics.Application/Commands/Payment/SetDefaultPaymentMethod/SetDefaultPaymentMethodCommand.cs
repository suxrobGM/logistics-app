using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class SetDefaultPaymentMethodCommand : IRequest<Result>
{
    public Guid PaymentMethodId { get; set; }
}
