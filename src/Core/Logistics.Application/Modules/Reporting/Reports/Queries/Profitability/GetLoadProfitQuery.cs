using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

/// <summary>One load's margin, with its truck's costs spread over the truck's loads of the delivery month.</summary>
public record GetLoadProfitQuery(Guid LoadId) : IQuery<Result<LoadProfitDto>>;
