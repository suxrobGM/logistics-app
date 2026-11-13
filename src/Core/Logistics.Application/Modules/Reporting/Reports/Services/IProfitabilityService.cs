using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Services;

/// <summary>Builds profit per load, truck and lane from existing rows on each call; nothing is stored.</summary>
public interface IProfitabilityService : IApplicationService
{
    /// <summary>Every load delivered in [from, to), with truck costs and salaried pay spread across them.</summary>
    Task<ProfitabilityReportDto> BuildAsync(DateTime from, DateTime to, CancellationToken ct = default);
}
