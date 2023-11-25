using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class DeleteLoadCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
}
