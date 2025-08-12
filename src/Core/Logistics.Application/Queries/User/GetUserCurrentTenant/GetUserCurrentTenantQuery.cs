using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetUserCurrentTenantQuery : IAppRequest<Result<TenantDto>>
{
    public Guid UserId { get; set; }
}
