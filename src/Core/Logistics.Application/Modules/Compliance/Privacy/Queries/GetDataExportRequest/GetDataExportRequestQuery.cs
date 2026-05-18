using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Privacy.Queries;

public class GetDataExportRequestQuery : IQuery<Result<DataExportRequestDto>>
{
    public Guid Id { get; set; }
}
