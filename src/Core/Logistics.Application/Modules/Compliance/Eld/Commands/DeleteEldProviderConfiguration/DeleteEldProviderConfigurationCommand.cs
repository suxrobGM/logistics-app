using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

[RequiresFeature(TenantFeature.Eld)]
public class DeleteEldProviderConfigurationCommand : ICommand
{
    public Guid ProviderId { get; set; }
}
