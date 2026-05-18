using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

public class GetLoadExceptionsQuery : IQuery<Result<IEnumerable<LoadExceptionDto>>>
{
    public Guid LoadId { get; set; }
}
