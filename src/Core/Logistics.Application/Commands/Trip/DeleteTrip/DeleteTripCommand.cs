using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class DeleteTripCommand : IRequest<Result>
{
    public Guid Id { get; set; }
}
