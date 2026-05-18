using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

public class ProcessPaymentCommand : ICommand
{
    public Guid PaymentId { get; set; }
}
