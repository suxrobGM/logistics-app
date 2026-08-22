using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

[RequiresFeature(TenantFeature.AIRateNegotiation)]
public class CreateLaneRateFloorCommand : ICommand<Result<Guid>>, ILaneRateFloorFields
{
    public string OriginCountry { get; set; } = "US";
    public string? OriginState { get; set; }
    public string DestinationCountry { get; set; } = "US";
    public string? DestinationState { get; set; }
    public decimal MinRatePerMile { get; set; }
    public decimal? MinTotalRateAmount { get; set; }
    public string MinTotalRateCurrency { get; set; } = "USD";
    public string? Notes { get; set; }
}
