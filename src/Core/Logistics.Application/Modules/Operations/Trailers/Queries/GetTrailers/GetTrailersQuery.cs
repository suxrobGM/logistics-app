using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trailers.Queries;

public class GetTrailersQuery : SearchableQuery, IQuery<PagedResult<TrailerDto>>
{
    public TrailerStatus? Status { get; set; }
    public TrailerType? Type { get; set; }
}
