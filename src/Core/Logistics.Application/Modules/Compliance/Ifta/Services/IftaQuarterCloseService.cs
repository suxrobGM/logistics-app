using System.Text.Json;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Ifta.Services;

internal sealed class IftaQuarterCloseService(
    ITenantUnitOfWork tenantUow,
    IIftaReportService reportService,
    ILogger<IftaQuarterCloseService> logger) : IIftaQuarterCloseService
{
    private static readonly TimeSpan AuditRetention = TimeSpan.FromDays(4 * 365);

    public async Task<bool> CloseQuarterForCurrentTenantAsync(CancellationToken ct = default)
    {
        var (year, quarter) = IftaQuarters.Previous(DateTime.UtcNow);

        var snapshotRepo = tenantUow.Repository<IftaQuarterSnapshot>();
        var exists = await snapshotRepo.GetAsync(s => s.Year == year && s.Quarter == quarter, ct) is not null;
        if (exists)
        {
            return false;
        }

        var report = await reportService.BuildReportAsync(year, quarter, ct);
        report.IsClosed = true;
        report.ClosedAt = DateTime.UtcNow;

        await snapshotRepo.AddAsync(new IftaQuarterSnapshot
        {
            Year = year,
            Quarter = quarter,
            ClosedAt = report.ClosedAt.Value,
            ReportJson = JsonSerializer.Serialize(report)
        }, ct);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation("Closed IFTA quarter Q{Quarter} {Year} ({Miles} mi, {Gallons} gal)",
            quarter, year, report.TotalMiles, report.TotalGallons);
        return true;
    }

    public async Task<int> PurgeExpiredLocationHistoryAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow - AuditRetention;
        return await tenantUow.Repository<TruckLocationHistory>().Query()
            .Where(h => h.Timestamp < cutoff)
            .ExecuteDeleteAsync(ct);
    }
}
