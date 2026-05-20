using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Inspections.Queries;

public class GetInspectionPartCatalogQuery : IQuery<Result<InspectionPartCatalogDto>>
{
    public required LoadType LoadType { get; set; }
}
