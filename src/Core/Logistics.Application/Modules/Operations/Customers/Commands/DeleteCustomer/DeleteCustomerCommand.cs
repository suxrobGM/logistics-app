
namespace Logistics.Application.Modules.Operations.Customers.Commands;

public class DeleteCustomerCommand : ICommand, IHaveId
{
    public Guid Id { get; set; }
}
