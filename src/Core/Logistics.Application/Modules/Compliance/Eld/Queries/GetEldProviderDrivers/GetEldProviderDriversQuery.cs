using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Queries;

public class GetEldProviderDriversQuery : IQuery<Result<List<EldDriverDto>>>
{
    public Guid ProviderId { get; set; }
}
