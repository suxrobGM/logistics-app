using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class CreateCustomerCommand : IAppRequest<Result<CustomerDto>>
{
    public string Name { get; set; } = null!;
}
