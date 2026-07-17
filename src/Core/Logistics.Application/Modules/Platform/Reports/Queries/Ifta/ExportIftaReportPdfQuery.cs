using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Reports.Queries;

[RequiresFeature(TenantFeature.Ifta)]
public class ExportIftaReportPdfQuery : IQuery<Result<byte[]>>
{
    public int Year { get; set; }

    /// <summary>Calendar quarter, 1-4.</summary>
    public int Quarter { get; set; }
}
