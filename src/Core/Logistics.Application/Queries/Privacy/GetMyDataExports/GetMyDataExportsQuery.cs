using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetMyDataExportsQuery : IQuery<Result<List<DataExportRequestDto>>>
{
}
