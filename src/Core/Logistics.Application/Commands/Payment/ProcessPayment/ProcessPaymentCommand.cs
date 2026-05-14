using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class ProcessPaymentCommand : ICommand
{
    public Guid PaymentId { get; set; }
}
