using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class UpdateTenantAiSettingsCommand : ICommand
{
    public LlmProvider? Provider { get; set; }
    public string? Model { get; set; }
}
