using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class UpdateTruckCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
    public string? TruckNumber { get; set; }
    public string[]? DriverIds { get; set; }
}
