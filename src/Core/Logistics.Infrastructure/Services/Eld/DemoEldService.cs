using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Services;

/// <summary>
///     Demo ELD provider implementation for testing without real ELD devices.
///     Returns simulated HOS data for development and demonstration purposes.
/// </summary>
internal class DemoEldService(ILogger<DemoEldService> logger) : IEldProviderService
{
    private static readonly Random Random = new();

    private static readonly string[] DemoDriverNames =
    [
        "John Smith", "Maria Garcia", "James Johnson", "Emily Davis",
        "Michael Brown", "Sarah Wilson", "David Martinez", "Lisa Anderson"
    ];

    public EldProviderType ProviderType => EldProviderType.Demo;

    public void Initialize(EldProviderConfiguration configuration)
    {
        logger.LogInformation("Initialized Demo ELD provider");
    }

    public Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        // Demo provider always validates successfully with any non-empty key
        return Task.FromResult(!string.IsNullOrEmpty(apiKey));
    }

    public Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken)
    {
        return Task.FromResult<OAuthTokenResultDto?>(null);
    }

    public Task<EldDriverHosDataDto?> GetDriverHosStatusAsync(string externalDriverId)
    {
        var driverIndex = Math.Abs(externalDriverId.GetHashCode()) % DemoDriverNames.Length;
        var dto = GenerateDriverHosData(externalDriverId, DemoDriverNames[driverIndex]);
        return Task.FromResult<EldDriverHosDataDto?>(dto);
    }

    public Task<IEnumerable<EldDriverHosDataDto>> GetAllDriversHosStatusAsync()
    {
        var drivers = new List<EldDriverHosDataDto>();

        for (var i = 0; i < DemoDriverNames.Length; i++)
        {
            var driverId = $"demo-driver-{i + 1}";
            drivers.Add(GenerateDriverHosData(driverId, DemoDriverNames[i]));
        }

        logger.LogDebug("Demo provider returned {Count} drivers", drivers.Count);
        return Task.FromResult<IEnumerable<EldDriverHosDataDto>>(drivers);
    }

    public Task<IEnumerable<EldHosLogEntryDto>> GetDriverHosLogsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        var logs = new List<EldHosLogEntryDto>();
        var currentDate = startDate.Date;

        while (currentDate <= endDate.Date)
        {
            logs.AddRange(GenerateDayLogs(externalDriverId, currentDate));
            currentDate = currentDate.AddDays(1);
        }

        return Task.FromResult<IEnumerable<EldHosLogEntryDto>>(logs);
    }

    public Task<IEnumerable<EldViolationDataDto>> GetDriverViolationsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        // Demo returns occasional violations for testing
        var violations = new List<EldViolationDataDto>();
        var days = (endDate - startDate).Days;

        if (days > 7 && Random.NextDouble() > 0.7)
        {
            violations.Add(new EldViolationDataDto
            {
                ExternalViolationId = $"demo-violation-{Guid.NewGuid():N}",
                ExternalDriverId = externalDriverId,
                ViolationDate = startDate.AddDays(Random.Next(days)),
                ViolationType = HosViolationType.Break30Minute,
                Description = "30-minute break violation - Demo",
                SeverityLevel = 2
            });
        }

        return Task.FromResult<IEnumerable<EldViolationDataDto>>(violations);
    }

    public Task<IEnumerable<EldDriverDto>> GetAllDriversAsync()
    {
        var drivers = new List<EldDriverDto>();

        for (var i = 0; i < DemoDriverNames.Length; i++)
        {
            drivers.Add(new EldDriverDto
            {
                ExternalDriverId = $"demo-driver-{i + 1}",
                Name = DemoDriverNames[i],
                Email = $"driver{i + 1}@demo.example.com",
                Phone = $"555-010{i}",
                LicenseNumber = $"DL{Random.Next(100000, 999999)}"
            });
        }

        return Task.FromResult<IEnumerable<EldDriverDto>>(drivers);
    }

    public Task<IEnumerable<EldVehicleDto>> GetAllVehiclesAsync()
    {
        var vehicles = new List<EldVehicleDto>();

        for (var i = 0; i < 5; i++)
        {
            vehicles.Add(new EldVehicleDto
            {
                ExternalVehicleId = $"demo-vehicle-{i + 1}",
                Name = $"Truck {1000 + i}",
                Vin = $"1HGBH41JXMN{Random.Next(100000, 999999)}",
                LicensePlate = $"ABC{Random.Next(1000, 9999)}",
                Make = i % 2 == 0 ? "Freightliner" : "Kenworth",
                Model = i % 2 == 0 ? "Cascadia" : "T680",
                Year = 2020 + (i % 4)
            });
        }

        return Task.FromResult<IEnumerable<EldVehicleDto>>(vehicles);
    }

    public Task<EldWebhookResultDto> ProcessWebhookAsync(string payload, string? signature, string? webhookSecret)
    {
        // Demo provider doesn't process real webhooks
        return Task.FromResult(new EldWebhookResultDto
        {
            EventType = EldWebhookEventType.Unknown,
            IsValid = false,
            ErrorMessage = "Demo provider does not support webhooks"
        });
    }

    #region Data Generation

    private static EldDriverHosDataDto GenerateDriverHosData(string driverId, string driverName)
    {
        var statuses = new[] { DutyStatus.Driving, DutyStatus.OnDutyNotDriving, DutyStatus.OffDuty, DutyStatus.SleeperBerth };
        var status = statuses[Random.Next(statuses.Length)];

        // Simulate realistic remaining times based on status
        var drivingMinutes = status == DutyStatus.Driving
            ? Random.Next(30, 600) // 30 min to 10 hours
            : Random.Next(300, 660); // More time if not driving

        var onDutyMinutes = Random.Next(60, 840); // 1-14 hours
        var cycleMinutes = Random.Next(600, 4200); // 10-70 hours

        var isInViolation = Random.NextDouble() < 0.05; // 5% chance of violation

        return new EldDriverHosDataDto
        {
            ExternalDriverId = driverId,
            ExternalDriverName = driverName,
            CurrentDutyStatus = status,
            StatusChangedAt = DateTime.UtcNow.AddMinutes(-Random.Next(5, 180)),
            DrivingMinutesRemaining = drivingMinutes,
            OnDutyMinutesRemaining = onDutyMinutes,
            CycleMinutesRemaining = cycleMinutes,
            TimeUntilBreakRequired = status == DutyStatus.Driving
                ? TimeSpan.FromMinutes(Random.Next(30, 480))
                : null,
            IsInViolation = isInViolation
        };
    }

    private static IEnumerable<EldHosLogEntryDto> GenerateDayLogs(string driverId, DateTime date)
    {
        var logs = new List<EldHosLogEntryDto>();
        var currentTime = date.Date.AddHours(6); // Start at 6 AM
        var endTime = date.Date.AddHours(22); // End at 10 PM

        var statusSequence = new[]
        {
            (DutyStatus.OnDutyNotDriving, 15, 30),  // Pre-trip inspection
            (DutyStatus.Driving, 120, 240),         // Morning driving
            (DutyStatus.OnDutyNotDriving, 30, 60),  // Loading/unloading
            (DutyStatus.OffDuty, 30, 45),           // Break
            (DutyStatus.Driving, 120, 180),         // Afternoon driving
            (DutyStatus.SleeperBerth, 480, 600)     // Rest period
        };

        var logId = 1;
        foreach (var (status, minDuration, maxDuration) in statusSequence)
        {
            if (currentTime >= endTime) break;

            var duration = Random.Next(minDuration, maxDuration);
            var endLogTime = currentTime.AddMinutes(duration);

            logs.Add(new EldHosLogEntryDto
            {
                ExternalLogId = $"demo-log-{date:yyyyMMdd}-{driverId}-{logId++}",
                ExternalDriverId = driverId,
                LogDate = date.Date,
                DutyStatus = status,
                StartTime = currentTime,
                EndTime = endLogTime,
                DurationMinutes = duration,
                Location = GetRandomLocation(),
                Latitude = 39.0 + Random.NextDouble() * 2,
                Longitude = -104.0 + Random.NextDouble() * 2
            });

            currentTime = endLogTime;
        }

        return logs;
    }

    private static string GetRandomLocation()
    {
        var locations = new[]
        {
            "Denver, CO", "Kansas City, MO", "Dallas, TX", "Phoenix, AZ",
            "Salt Lake City, UT", "Albuquerque, NM", "Oklahoma City, OK",
            "Truck Stop - I-70 Mile 134", "Rest Area - US-40"
        };
        return locations[Random.Next(locations.Length)];
    }

    #endregion
}
