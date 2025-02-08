using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateCustomerCommand : IRequest<Result>
{
    public string Name { get; set; } = null!;
}
