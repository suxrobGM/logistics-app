using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateTruckCommand : IRequest<Result>
{
    public string? TruckNumber { get; set; }
    public Guid[]? DriversIds { get; set; }
}
