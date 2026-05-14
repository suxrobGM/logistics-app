using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetEldProviderDriversQuery : IQuery<Result<List<EldDriverDto>>>
{
    public Guid ProviderId { get; set; }
}
