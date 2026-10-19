using Logistics.Application.Abstractions.Eld;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Application.Modules.Compliance.Ifta.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Eld.Services;

internal sealed class EldSyncService(
    ITenantUnitOfWork tenantUow,
    IEldProviderFactory eldProviderFactory,
    ITruckLocationRecorder locationRecorder,
    IRealtimeLiveTrackingService liveTracking,
    ILogger<EldSyncService> logger) : IEldSyncService
{
    private static readonly TimeSpan HistoryWindow = TimeSpan.FromDays(8);

    public async Task SyncLiveAsync(EldProviderConfiguration config, CancellationToken ct)
    {
        var provider = eldProviderFactory.GetProvider(config);
        var mappings = await GetDriverMappingsAsync(config, ct);

        if (mappings.Count > 0)
        {
            var clocks = (await provider.GetAllDriversHosStatusAsync())
                .Where(h => !string.IsNullOrEmpty(h.ExternalDriverId))
                .GroupBy(h => h.ExternalDriverId)
                .ToDictionary(g => g.Key, g => g.First());

            var employeeIds = mappings.Select(m => m.EmployeeId).ToList();
            var statuses = (await tenantUow.Repository<DriverHosStatus>()
                    .GetListAsync(s => employeeIds.Contains(s.EmployeeId), ct))
                .ToDictionary(s => s.EmployeeId);

            foreach (var mapping in mappings)
            {
                if (clocks.TryGetValue(mapping.ExternalDriverId, out var hosData))
                {
                    await ApplyHosStatusAsync(mapping, hosData, statuses.GetValueOrDefault(mapping.EmployeeId), ct);
                }
            }
        }

        var locations = provider is IEldGpsTrackingProvider gps
            ? await SyncVehicleLocationsAsync(config, gps, ct)
            : [];

        config.LastSyncedAt = DateTime.UtcNow;
        await tenantUow.SaveChangesAsync(ct);

        var tenantId = tenantUow.GetCurrentTenant().Id.ToString();
        foreach (var location in locations)
        {
            await liveTracking.BroadcastGeolocationDataAsync(tenantId, location, ct);
        }
    }

    public async Task SyncHistoryAsync(EldProviderConfiguration config, CancellationToken ct)
    {
        var provider = eldProviderFactory.GetProvider(config);
        var mappings = await GetDriverMappingsAsync(config, ct);

        foreach (var mapping in mappings)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            await SyncDriverLogsAsync(provider, mapping, ct);
            await UpsertViolationsAsync(provider, mapping, ct);
        }

        await tenantUow.SaveChangesAsync(ct);
    }

    public async Task SyncDriverHosStatusAsync(
        IEldProviderService provider, EldDriverMapping mapping, CancellationToken ct)
    {
        var hosData = await provider.GetDriverHosStatusAsync(mapping.ExternalDriverId);
        if (hosData is null)
        {
            return;
        }

        var status = await tenantUow.Repository<DriverHosStatus>()
            .GetAsync(s => s.EmployeeId == mapping.EmployeeId, ct);
        await ApplyHosStatusAsync(mapping, hosData, status, ct);
        await tenantUow.SaveChangesAsync(ct);
    }

    public async Task SyncDriverViolationsAsync(
        IEldProviderService provider, EldDriverMapping mapping, CancellationToken ct)
    {
        var added = await UpsertViolationsAsync(provider, mapping, ct);
        if (added > 0)
        {
            var status = await tenantUow.Repository<DriverHosStatus>()
                .GetAsync(s => s.EmployeeId == mapping.EmployeeId, ct);
            if (status is not null)
            {
                status.IsInViolation = true;
                status.LastUpdatedAt = DateTime.UtcNow;
            }
        }

        await tenantUow.SaveChangesAsync(ct);
    }

    private async Task<List<EldDriverMapping>> GetDriverMappingsAsync(
        EldProviderConfiguration config, CancellationToken ct)
    {
        var mappings = await tenantUow.Repository<EldDriverMapping>()
            .GetListAsync(m => m.ProviderType == config.ProviderType && m.IsSyncEnabled, ct);

        return config.ProviderType == EldProviderType.Demo && mappings.Count == 0
            ? await CreateDemoMappingsAsync(config, ct)
            : mappings;
    }

    private async Task ApplyHosStatusAsync(
        EldDriverMapping mapping, EldDriverHosDataDto hosData, DriverHosStatus? status, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        if (status is null)
        {
            status = new DriverHosStatus
            {
                EmployeeId = mapping.EmployeeId,
                CurrentDutyStatus = hosData.CurrentDutyStatus,
                StatusChangedAt = hosData.StatusChangedAt ?? now
            };
            await tenantUow.Repository<DriverHosStatus>().AddAsync(status, ct);
        }
        else if (hosData.StatusChangedAt.HasValue)
        {
            status.StatusChangedAt = hosData.StatusChangedAt.Value;
        }
        else if (status.CurrentDutyStatus != hosData.CurrentDutyStatus)
        {
            // Providers that report no change time: the first sync that sees the new status dates it
            status.StatusChangedAt = now;
        }

        status.CurrentDutyStatus = hosData.CurrentDutyStatus;
        status.DrivingMinutesRemaining = hosData.DrivingMinutesRemaining;
        status.OnDutyMinutesRemaining = hosData.OnDutyMinutesRemaining;
        status.CycleMinutesRemaining = hosData.CycleMinutesRemaining;
        status.TimeUntilBreakRequired = hosData.TimeUntilBreakRequired;
        status.IsInViolation = hosData.IsInViolation;
        status.NextMandatoryBreakAt = hosData.NextMandatoryBreakAt;
        status.LastUpdatedAt = now;
        mapping.LastSyncedAt = now;
    }

    /// <summary>
    /// Replaces the driver's stored logs inside the window with what the provider returns now, so edits
    /// and deletions made in the ELD show up. An empty answer is treated as "could not read" and keeps
    /// what is stored rather than wiping the driver's history.
    /// </summary>
    private async Task SyncDriverLogsAsync(IEldProviderService provider, EldDriverMapping mapping, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var windowStart = now - HistoryWindow;
        var fetched = (await provider.GetDriverHosLogsAsync(mapping.ExternalDriverId, windowStart, now))
            .Where(l => l.StartTime >= windowStart && l.StartTime <= now)
            .GroupBy(LogKey)
            .ToDictionary(g => g.Key, g => g.First());

        if (fetched.Count == 0)
        {
            return;
        }

        // A fetched log whose stored copy started before the window (the ELD moved its start) is loaded too, so it
        // is updated in place instead of inserted twice against the unique (employee, provider, log id) index.
        var repo = tenantUow.Repository<HosLog>();
        var keys = fetched.Keys.ToList();
        var stored = await repo.GetListAsync(l =>
            l.EmployeeId == mapping.EmployeeId &&
            l.ProviderType == mapping.ProviderType &&
            (l.StartTime >= windowStart || keys.Contains(l.ExternalLogId!)), ct);

        var seen = new HashSet<string>();
        foreach (var log in stored)
        {
            if (log.ExternalLogId is not null && fetched.TryGetValue(log.ExternalLogId, out var entry) && seen.Add(log.ExternalLogId))
            {
                Apply(log, entry);
            }
            else
            {
                repo.Delete(log);
            }
        }

        foreach (var (key, entry) in fetched.Where(f => !seen.Contains(f.Key)))
        {
            var log = new HosLog
            {
                EmployeeId = mapping.EmployeeId,
                ProviderType = mapping.ProviderType,
                ExternalLogId = key,
                LogDate = entry.LogDate,
                DutyStatus = entry.DutyStatus,
                StartTime = entry.StartTime
            };
            Apply(log, entry);
            await repo.AddAsync(log, ct);
        }

        static void Apply(HosLog log, EldHosLogEntryDto entry)
        {
            log.LogDate = entry.LogDate;
            log.DutyStatus = entry.DutyStatus;
            log.StartTime = entry.StartTime;
            log.EndTime = entry.EndTime;
            log.DurationMinutes = entry.DurationMinutes;
            log.Location = entry.Location;
            log.Latitude = entry.Latitude;
            log.Longitude = entry.Longitude;
            log.Remark = entry.Remark;
        }
    }

    /// <returns>How many violations were new.</returns>
    private async Task<int> UpsertViolationsAsync(IEldProviderService provider, EldDriverMapping mapping, CancellationToken ct)
    {
        var windowStart = DateTime.UtcNow - HistoryWindow;
        var fetched = (await provider.GetDriverViolationsAsync(mapping.ExternalDriverId, windowStart, DateTime.UtcNow))
            .GroupBy(ViolationKey)
            .ToDictionary(g => g.Key, g => g.First());

        if (fetched.Count == 0)
        {
            return 0;
        }

        var repo = tenantUow.Repository<HosViolation>();
        var keys = fetched.Keys.ToList();
        var known = (await repo.GetListAsync(v =>
                v.ProviderType == mapping.ProviderType &&
                v.ExternalViolationId != null &&
                keys.Contains(v.ExternalViolationId), ct))
            .Select(v => v.ExternalViolationId!)
            .ToHashSet();

        var ruleSetCode = RuleSetSelector.CodeFor(tenantUow.GetCurrentTenant().Settings.Region);
        var added = 0;
        foreach (var (key, violation) in fetched.Where(f => !known.Contains(f.Key)))
        {
            await repo.AddAsync(new HosViolation
            {
                EmployeeId = mapping.EmployeeId,
                ViolationDate = violation.ViolationDate,
                ViolationType = violation.ViolationType,
                Description = violation.Description,
                SeverityLevel = violation.SeverityLevel,
                ExternalViolationId = key,
                ProviderType = mapping.ProviderType,
                RuleSetCode = ruleSetCode
            }, ct);
            added++;
        }

        if (added > 0)
        {
            logger.LogInformation("Recorded {Count} new HOS violations for employee {EmployeeId}",
                added, mapping.EmployeeId);
        }

        return added;
    }

    private async Task<IReadOnlyList<TruckGeolocationDto>> SyncVehicleLocationsAsync(
        EldProviderConfiguration config, IEldGpsTrackingProvider gps, CancellationToken ct)
    {
        var vehicleMappings = await tenantUow.Repository<EldVehicleMapping>()
            .GetListAsync(m => m.ProviderType == config.ProviderType && m.IsSyncEnabled, ct);
        if (vehicleMappings.Count == 0)
        {
            return [];
        }

        var locations = (await gps.GetAllVehicleLocationsAsync(ct))
            .Where(l => !string.IsNullOrEmpty(l.ExternalVehicleId) && IsValidCoordinate(l))
            .GroupBy(l => l.ExternalVehicleId)
            .ToDictionary(g => g.Key, g => g.MaxBy(l => l.Timestamp)!);

        var truckIds = vehicleMappings.Select(m => m.TruckId).ToList();
        var trucks = (await tenantUow.Repository<Truck>().GetListAsync(t => truckIds.Contains(t.Id), ct))
            .ToDictionary(t => t.Id);

        var tenantId = tenantUow.GetCurrentTenant().Id;
        var updates = new List<TruckGeolocationDto>();
        foreach (var mapping in vehicleMappings)
        {
            if (!locations.TryGetValue(mapping.ExternalVehicleId, out var location) ||
                !trucks.TryGetValue(mapping.TruckId, out var truck))
            {
                continue;
            }

            var point = new GeoPoint(location.Longitude, location.Latitude);
            var timestamp = location.Timestamp == default ? DateTime.UtcNow : location.Timestamp;
            await locationRecorder.RecordAsync(truck, point, timestamp, config.ProviderType, location.OdometerMiles, ct: ct);
            mapping.LastSyncedAt = DateTime.UtcNow;

            updates.Add(new TruckGeolocationDto
            {
                TruckId = truck.Id,
                TenantId = tenantId,
                CurrentLocation = point,
                TruckNumber = truck.Number
            });
        }

        return updates;
    }

    /// <summary>
    /// Maps the Demo provider's drivers onto the tenant's first employees so the demo works without
    /// any setup.
    /// </summary>
    private async Task<List<EldDriverMapping>> CreateDemoMappingsAsync(EldProviderConfiguration config, CancellationToken ct)
    {
        var demoDrivers = (await eldProviderFactory.GetProvider(config).GetAllDriversAsync()).ToList();
        var employees = await tenantUow.Repository<Employee>().Query()
            .OrderBy(e => e.JoinedDate)
            .Take(demoDrivers.Count)
            .ToListAsync(ct);

        var mappings = employees.Zip(demoDrivers, (employee, driver) => new EldDriverMapping
        {
            EmployeeId = employee.Id,
            ProviderConfigurationId = config.Id,
            ExternalDriverId = driver.ExternalDriverId,
            ExternalDriverName = driver.Name,
            ProviderType = EldProviderType.Demo
        }).ToList();

        await tenantUow.Repository<EldDriverMapping>().AddRangeAsync(mappings, ct);
        logger.LogInformation("Auto-created {Count} demo driver mappings", mappings.Count);
        return mappings;
    }

    private static string LogKey(EldHosLogEntryDto log) =>
        log.ExternalLogId ?? $"{log.ExternalDriverId}:{log.StartTime:O}";

    private static string ViolationKey(EldViolationDataDto violation) =>
        violation.ExternalViolationId ?? $"{violation.ExternalDriverId}:{violation.ViolationType}:{violation.ViolationDate:O}";

    private static bool IsValidCoordinate(EldVehicleLocationDto location) =>
        location.Latitude is >= -90 and <= 90 &&
        location.Longitude is >= -180 and <= 180 &&
        !(location.Latitude == 0 && location.Longitude == 0);
}
