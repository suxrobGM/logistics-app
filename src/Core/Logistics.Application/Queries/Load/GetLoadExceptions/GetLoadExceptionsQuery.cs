using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetLoadExceptionsQuery : IAppRequest<Result<IEnumerable<LoadExceptionDto>>>
{
    public Guid LoadId { get; set; }
}
