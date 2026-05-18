using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Users.Queries;

public sealed class GetCurrentUserPermissionsQuery : IQuery<Result<string[]>>
{
    public required Guid UserId { get; set; }
    public Guid? TenantId { get; set; }
}
