using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class CreateTruckCommand : IRequest<Result>
{
    public string? TruckNumber { get; set; }
    public string[]? DriversIds { get; set; }
}
