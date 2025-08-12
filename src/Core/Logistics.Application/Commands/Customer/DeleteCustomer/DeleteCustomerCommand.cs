using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteCustomerCommand : IAppRequest
{
    public Guid Id { get; set; }
}
