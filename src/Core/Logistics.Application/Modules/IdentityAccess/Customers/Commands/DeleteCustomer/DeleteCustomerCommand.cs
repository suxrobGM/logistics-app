using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Commands;

public class DeleteCustomerCommand : ICommand, IHaveId
{
    public Guid Id { get; set; }
}
