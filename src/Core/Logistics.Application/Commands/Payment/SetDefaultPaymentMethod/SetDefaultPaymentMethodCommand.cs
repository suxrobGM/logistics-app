using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class SetDefaultPaymentMethodCommand : IAppRequest
{
    public Guid PaymentMethodId { get; set; }
}
