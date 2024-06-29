using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class DeleteLoadCommand : IRequest<Result>
{
    public string Id { get; set; } = default!;
}
