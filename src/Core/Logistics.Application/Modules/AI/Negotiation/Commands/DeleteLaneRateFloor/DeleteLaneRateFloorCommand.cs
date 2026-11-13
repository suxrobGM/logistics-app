using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.AI.Negotiation.Commands;

[RequiresFeature(TenantFeature.AIRateNegotiation)]
public class DeleteLaneRateFloorCommand : ICommand, IHaveId
{
    public Guid Id { get; set; }
}
