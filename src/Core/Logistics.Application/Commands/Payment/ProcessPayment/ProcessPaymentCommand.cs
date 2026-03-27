using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class ProcessPaymentCommand : IAppRequest
{
    public Guid PaymentId { get; set; }
}
