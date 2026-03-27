using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public sealed class ResetTenantQuotasCommand : IAppRequest<Result>
{
    public List<Guid> TenantIds { get; init; } = [];
}
