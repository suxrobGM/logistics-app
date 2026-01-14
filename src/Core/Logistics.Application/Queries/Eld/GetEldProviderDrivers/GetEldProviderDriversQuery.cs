using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetEldProviderDriversQuery : IAppRequest<Result<List<EldDriverDto>>>
{
    public Guid ProviderId { get; set; }
}
