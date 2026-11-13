using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.Negotiation.Commands;

[RequiresFeature(TenantFeature.AINegotiation)]
public class UpdateLaneRateFloorCommand : ICommand<Result>, ILaneRateFloorFields, IHaveId
{
    public Guid Id { get; set; }
    public string OriginCountry { get; set; } = "US";
    public string? OriginState { get; set; }
    public string DestinationCountry { get; set; } = "US";
    public string? DestinationState { get; set; }
    public decimal MinRatePerKm { get; set; }
    public decimal? MinTotalRate { get; set; }
    public string? Notes { get; set; }
}
