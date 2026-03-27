using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.Eld)]
public class DeleteEldProviderConfigurationCommand : IAppRequest
{
    public Guid ProviderId { get; set; }
}
