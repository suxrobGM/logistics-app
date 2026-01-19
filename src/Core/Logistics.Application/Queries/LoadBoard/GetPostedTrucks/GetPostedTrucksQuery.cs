using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetPostedTrucksQuery : IAppRequest<Result<List<PostedTruckDto>>>
{
    public LoadBoardProviderType? ProviderType { get; set; }
    public PostedTruckStatus? Status { get; set; }
    public Guid? TruckId { get; set; }
}
