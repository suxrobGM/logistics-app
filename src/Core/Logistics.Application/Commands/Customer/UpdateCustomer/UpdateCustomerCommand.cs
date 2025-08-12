using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class UpdateCustomerCommand : IAppRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}
