using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class UpdateCustomerCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}
