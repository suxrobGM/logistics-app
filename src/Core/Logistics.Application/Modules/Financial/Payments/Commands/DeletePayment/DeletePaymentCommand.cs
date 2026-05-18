using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

public class DeletePaymentCommand : ICommand
{
    public Guid Id { get; set; }
}
