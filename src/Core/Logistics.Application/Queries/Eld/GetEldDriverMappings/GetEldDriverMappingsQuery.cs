using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetEldDriverMappingsQuery : IAppRequest<Result<List<EldDriverMappingDto>>>
{
    public Guid ProviderId { get; set; }
}
