using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Queries;

public class GetEldMappingSuggestionsQuery : IQuery<Result<EldMappingSuggestionsDto>>
{
    public Guid ProviderId { get; set; }
}
