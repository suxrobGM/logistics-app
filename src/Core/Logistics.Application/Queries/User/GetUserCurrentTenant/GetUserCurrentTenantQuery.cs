using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetUserCurrentTenantQuery : IQuery<Result<TenantDto>>
{
    public Guid UserId { get; set; }
}
