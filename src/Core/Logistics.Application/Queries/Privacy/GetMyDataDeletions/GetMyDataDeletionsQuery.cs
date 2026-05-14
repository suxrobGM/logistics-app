using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetMyDataDeletionsQuery : IQuery<Result<List<DataDeletionRequestDto>>>
{
}
