using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetMyDataDeletionsQuery : IAppRequest<Result<List<DataDeletionRequestDto>>>
{
}
