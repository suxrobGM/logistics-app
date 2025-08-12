using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class DeleteTruckCommand : IRequest<Result>
{
    public Guid Id { get; set; }
}
