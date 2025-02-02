using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateCustomerCommand : IRequest<Result>
{
    public string Name { get; set; } = null!;
}
