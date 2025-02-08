using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateTruckCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
    public string? TruckNumber { get; set; }
    public string[]? DriverIds { get; set; }
}
