using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Users.Queries;

public class GetUserCurrentTenantQuery : IQuery<Result<TenantDto>>
{
    public Guid UserId { get; set; }
}
