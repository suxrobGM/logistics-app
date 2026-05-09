using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetDataExportRequestQuery : IAppRequest<Result<DataExportRequestDto>>
{
    public Guid Id { get; set; }
}
