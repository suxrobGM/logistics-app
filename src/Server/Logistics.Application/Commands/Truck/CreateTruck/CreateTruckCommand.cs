using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateTruckCommand : IRequest<Result>
{
    public string? TruckNumber { get; set; }
    public string[]? DriversIds { get; set; }
}
