using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Dvir.Queries;

[RequiresFeature(TenantFeature.Dvir)]
public record GetTruckOpenDvirDefectsQuery(Guid TruckId) : IQuery<Result<List<DvirDefectDto>>>;
