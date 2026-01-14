using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetCurrentUserPermissionsQuery : IAppRequest<Result<string[]>>
{
    public required Guid UserId { get; set; }
    public Guid? TenantId { get; set; }
}
