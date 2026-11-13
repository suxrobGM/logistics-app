using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AISettings.Commands;

public sealed class ResetTenantQuotasCommand : ICommand<Result>
{
    public List<Guid> TenantIds { get; init; } = [];
}
