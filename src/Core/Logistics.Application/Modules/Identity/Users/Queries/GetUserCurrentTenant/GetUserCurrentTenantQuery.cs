using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.Users.Queries;

public class GetUserCurrentTenantQuery : IQuery<Result<TenantDto>>
{
    public Guid UserId { get; set; }
}
