using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class DeleteTruckCommand : IRequest<Result>
{
    public string Id { get; set; } = default!;
}
