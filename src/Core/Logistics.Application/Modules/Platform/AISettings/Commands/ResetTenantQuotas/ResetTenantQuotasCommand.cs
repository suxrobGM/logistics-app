using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.AISettings.Commands;

public sealed class ResetTenantQuotasCommand : ICommand<Result>
{
    public List<Guid> TenantIds { get; init; } = [];
}
