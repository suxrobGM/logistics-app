using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetUserCurrentTenantQuery : IRequest<Result<TenantDto>>
{
    public Guid UserId { get; set; }
}
