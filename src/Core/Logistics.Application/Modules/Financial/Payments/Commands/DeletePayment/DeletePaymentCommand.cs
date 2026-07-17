using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

public class DeletePaymentCommand : ICommand, IHaveId
{
    public Guid Id { get; set; }
}
