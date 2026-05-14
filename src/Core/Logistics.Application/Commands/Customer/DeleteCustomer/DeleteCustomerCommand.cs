using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteCustomerCommand : ICommand
{
    public Guid Id { get; set; }
}
