using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Roles;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds realistic HOS (Hours of Service) status records for all drivers.
/// Required for the AI dispatch agent to check feasibility and make assignments.
/// </summary>
internal class DriverHosStatusSeeder(ILogger<DriverHosStatusSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(DriverHosStatusSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 160;
    public override IReadOnlyList<string> DependsOn => [nameof(EmployeeSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<DriverHosStatus>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var hosRepo = context.TenantUnitOfWork.Repository<DriverHosStatus>();
        var employeeRepo = context.TenantUnitOfWork.Repository<Employee>();

        var drivers = context.CreatedEmployees?.Drivers
            ?? await employeeRepo.GetListAsync(
                e => e.Role != null && e.Role.Name == TenantRoles.Driver, ct: cancellationToken);

        if (drivers.Count == 0)
        {
            logger.LogWarning("No drivers available for HOS status seeding");
            LogCompleted(0);
            return;
        }

        var count = 0;
        foreach (var driver in drivers)
        {
            var hos = CreateHosStatus(driver);
            await hosRepo.AddAsync(hos, cancellationToken);
            count++;
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(count);
    }

    private DriverHosStatus CreateHosStatus(Employee driver)
    {
        // Simulate realistic HOS scenarios
        var scenario = random.NextDouble();

        return scenario switch
        {
            // 50% — Fresh driver, plenty of hours (just started shift or after reset)
            < 0.50 => CreateFreshDriver(driver),
            // 25% — Mid-shift driver, moderate hours remaining
            < 0.75 => CreateMidShiftDriver(driver),
            // 15% — Low hours, approaching limits
            < 0.90 => CreateLowHoursDriver(driver),
            // 5% — Off duty / sleeper
            < 0.95 => CreateOffDutyDriver(driver),
            // 5% — In violation
            _ => CreateViolationDriver(driver)
        };
    }

    private DriverHosStatus CreateFreshDriver(Employee driver)
    {
        var drivingRemaining = random.Next(540, 660); // 9-11 hours
        var onDutyRemaining = random.Next(720, 840);  // 12-14 hours
        return new DriverHosStatus
        {
            EmployeeId = driver.Id,
            CurrentDutyStatus = DutyStatus.OnDutyNotDriving,
            ProviderType = EldProviderType.Demo,
            StatusChangedAt = DateTime.UtcNow.AddMinutes(-random.Next(5, 60)),
            DrivingMinutesRemaining = drivingRemaining,
            OnDutyMinutesRemaining = onDutyRemaining,
            CycleMinutesRemaining = random.Next(2400, 4200), // 40-70 hours
            TimeUntilBreakRequired = TimeSpan.FromMinutes(random.Next(360, 480)),
            IsInViolation = false,
            LastUpdatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1, 15)),
            NextMandatoryBreakAt = DateTime.UtcNow.AddHours(random.Next(6, 8))
        };
    }

    private DriverHosStatus CreateMidShiftDriver(Employee driver)
    {
        var drivingRemaining = random.Next(240, 480); // 4-8 hours
        var onDutyRemaining = random.Next(360, 600);  // 6-10 hours
        return new DriverHosStatus
        {
            EmployeeId = driver.Id,
            CurrentDutyStatus = random.NextDouble() < 0.5 ? DutyStatus.Driving : DutyStatus.OnDutyNotDriving,
            ProviderType = EldProviderType.Demo,
            StatusChangedAt = DateTime.UtcNow.AddMinutes(-random.Next(30, 180)),
            DrivingMinutesRemaining = drivingRemaining,
            OnDutyMinutesRemaining = onDutyRemaining,
            CycleMinutesRemaining = random.Next(1800, 3600), // 30-60 hours
            TimeUntilBreakRequired = TimeSpan.FromMinutes(random.Next(60, 240)),
            IsInViolation = false,
            LastUpdatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1, 10)),
            NextMandatoryBreakAt = DateTime.UtcNow.AddHours(random.Next(1, 4))
        };
    }

    private DriverHosStatus CreateLowHoursDriver(Employee driver)
    {
        var drivingRemaining = random.Next(30, 120); // 0.5-2 hours
        var onDutyRemaining = random.Next(60, 180);  // 1-3 hours
        return new DriverHosStatus
        {
            EmployeeId = driver.Id,
            CurrentDutyStatus = DutyStatus.OnDutyNotDriving,
            ProviderType = EldProviderType.Demo,
            StatusChangedAt = DateTime.UtcNow.AddMinutes(-random.Next(5, 30)),
            DrivingMinutesRemaining = drivingRemaining,
            OnDutyMinutesRemaining = onDutyRemaining,
            CycleMinutesRemaining = random.Next(600, 1200), // 10-20 hours
            TimeUntilBreakRequired = TimeSpan.FromMinutes(random.Next(10, 60)),
            IsInViolation = false,
            LastUpdatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1, 5)),
            NextMandatoryBreakAt = DateTime.UtcNow.AddMinutes(random.Next(30, 120))
        };
    }

    private DriverHosStatus CreateOffDutyDriver(Employee driver)
    {
        return new DriverHosStatus
        {
            EmployeeId = driver.Id,
            CurrentDutyStatus = random.NextDouble() < 0.5 ? DutyStatus.OffDuty : DutyStatus.SleeperBerth,
            ProviderType = EldProviderType.Demo,
            StatusChangedAt = DateTime.UtcNow.AddHours(-random.Next(1, 8)),
            DrivingMinutesRemaining = 660, // Full 11 hours (resting)
            OnDutyMinutesRemaining = 840,  // Full 14 hours
            CycleMinutesRemaining = random.Next(3000, 4200),
            TimeUntilBreakRequired = null,
            IsInViolation = false,
            LastUpdatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1, 30))
        };
    }

    private DriverHosStatus CreateViolationDriver(Employee driver)
    {
        return new DriverHosStatus
        {
            EmployeeId = driver.Id,
            CurrentDutyStatus = DutyStatus.Driving,
            ProviderType = EldProviderType.Demo,
            StatusChangedAt = DateTime.UtcNow.AddMinutes(-random.Next(10, 60)),
            DrivingMinutesRemaining = 0,
            OnDutyMinutesRemaining = random.Next(0, 30),
            CycleMinutesRemaining = random.Next(0, 120),
            TimeUntilBreakRequired = TimeSpan.Zero,
            IsInViolation = true,
            LastUpdatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1, 5)),
            NextMandatoryBreakAt = DateTime.UtcNow.AddMinutes(-random.Next(5, 30))
        };
    }
}
