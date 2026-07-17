using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

[RequiresFeature(TenantFeature.FuelCards)]
public class DeleteFuelCardProviderConfigurationCommand : ICommand<Result>, IHaveId
{
    public Guid Id { get; set; }
}
