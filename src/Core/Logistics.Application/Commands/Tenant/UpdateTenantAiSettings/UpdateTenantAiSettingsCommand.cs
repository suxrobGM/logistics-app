using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public class UpdateTenantAiSettingsCommand : ICrossDatabaseCommand
{
    public LlmProvider? Provider { get; set; }
    public string? Model { get; set; }
}
