using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateTruckCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string? TruckNumber { get; set; }
    public Guid[]? DriverIds { get; set; }
}
