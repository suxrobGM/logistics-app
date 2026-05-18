using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Queries;

/// <summary>
/// Returns the regulatory HOS limits for the current tenant's region, used by
/// the dashboard / mobile UI to render counters and threshold colours.
/// </summary>
public class GetHosLimitsQuery : IQuery<Result<HosLimitsDto>>;
