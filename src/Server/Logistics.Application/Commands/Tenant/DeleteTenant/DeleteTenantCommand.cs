using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Commands;

public class DeleteTenantCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
}
