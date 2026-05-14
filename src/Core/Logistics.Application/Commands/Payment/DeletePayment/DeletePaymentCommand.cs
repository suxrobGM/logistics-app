using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeletePaymentCommand : ICommand
{
    public Guid Id { get; set; }
}
