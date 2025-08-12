using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeletePaymentMethodCommand : IAppRequest
{
    public Guid Id { get; set; }
}
