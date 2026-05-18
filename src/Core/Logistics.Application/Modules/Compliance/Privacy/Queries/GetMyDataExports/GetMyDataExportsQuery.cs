using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Privacy.Queries;

public class GetMyDataExportsQuery : IQuery<Result<List<DataExportRequestDto>>>
{
}
