using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Commands;

public sealed class ResetTenantQuotasCommand : ICommand<Result>
{
    public List<Guid> TenantIds { get; init; } = [];
}
