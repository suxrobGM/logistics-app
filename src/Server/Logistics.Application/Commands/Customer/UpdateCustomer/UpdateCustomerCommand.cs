using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateCustomerCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
}
