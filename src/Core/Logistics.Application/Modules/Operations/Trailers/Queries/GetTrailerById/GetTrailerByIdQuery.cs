using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trailers.Queries;

public class GetTrailerByIdQuery : IQuery<Result<TrailerDto>>, IHaveId
{
    public Guid Id { get; set; }
}
