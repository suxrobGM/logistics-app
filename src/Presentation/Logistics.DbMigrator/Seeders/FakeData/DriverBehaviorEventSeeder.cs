using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Utils;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Identity.Roles;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds sample driver behavior events (harsh braking, speeding, etc.) for testing.
/// </summary>
internal class DriverBehaviorEventSeeder(ILogger<DriverBehaviorEventSeeder> logger) : SeederBase(logger)
{
    private static readonly string[] reviewNotes =
    [
        "Reviewed dashcam footage, event confirmed.",
        "Spoke with driver, coaching completed.",
        "Driver explained circumstances, excused due to road conditions.",
        "Event dismissed - false positive from sensor.",
        "Warning issued, added to driver file.",
        "Scheduled safety refresher training.",
        "Driver acknowledged, no further action needed."
    ];

    private static readonly string[] locationDescriptions =
    [
        "Interstate highway near exit ramp",
        "City street intersection",
        "Highway merge zone",
        "Construction zone",
        "Residential area",
        "Commercial district",
        "Rural highway",
        "Parking lot entrance"
    ];

    public override string Name => nameof(DriverBehaviorEventSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 159;
    public override IReadOnlyList<string> DependsOn =>
        [nameof(TruckSeeder), nameof(EmployeeSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<DriverBehaviorEvent>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var eventRepository = context.TenantUnitOfWork.Repository<DriverBehaviorEvent>();
        var employeeRepository = context.TenantUnitOfWork.Repository<Employee>();
        var truckRepository = context.TenantUnitOfWork.Repository<Truck>();

        // Get drivers from context or load from database
        var drivers = context.CreatedEmployees?.Drivers
            ?? await employeeRepository.GetListAsync(e => e.Role != null && e.Role.Name == TenantRoles.Driver, ct: cancellationToken);

        if (drivers.Count == 0)
        {
            logger.LogWarning("No drivers available for driver behavior event seeding");
            LogCompleted(0);
            return;
        }

        // Get trucks
        var trucks = context.CreatedTrucks
            ?? await truckRepository.GetListAsync(ct: cancellationToken);

        var count = 0;

        // Create events for the past 30 days
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        // Each driver should have some events (most drivers will have few, some will have more)
        foreach (var driver in drivers)
        {
            // 70% of drivers have events, varying from 2-15 events
            if (random.NextDouble() > 0.7) continue;

            var eventCount = random.Next(2, 16);
            var truck = trucks.Count > 0 ? random.Pick(trucks) : null;

            for (var i = 0; i < eventCount; i++)
            {
                var eventDate = random.UtcDate(startDate, endDate);
                var location = random.Pick(RoutePoints.Points);
                var eventType = PickEventType();

                var behaviorEvent = CreateBehaviorEvent(driver, truck, location, eventDate, eventType);
                await eventRepository.AddAsync(behaviorEvent, cancellationToken);
                count++;
            }
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(count);
    }

    private DriverBehaviorEvent CreateBehaviorEvent(
        Employee driver,
        Truck? truck,
        (Domain.Primitives.ValueObjects.Address Address, double Longitude, double Latitude) location,
        DateTime eventDate,
        DriverBehaviorEventType eventType)
    {
        var isReviewed = random.NextDouble() < 0.6; // 60% are reviewed
        var isDismissed = isReviewed && random.NextDouble() < 0.2; // 20% of reviewed are dismissed

        var behaviorEvent = new DriverBehaviorEvent
        {
            EmployeeId = driver.Id,
            TruckId = truck?.Id,
            EventType = eventType,
            OccurredAt = eventDate,
            ProviderType = random.Pick(new[] { EldProviderType.Samsara, EldProviderType.Motive, EldProviderType.Demo }),
            Latitude = location.Latitude + (random.NextDouble() - 0.5) * 0.05,
            Longitude = location.Longitude + (random.NextDouble() - 0.5) * 0.05,
            Location = $"{random.Pick(locationDescriptions)} near {location.Address.City}, {location.Address.State}",
            ExternalEventId = $"ELD-{Guid.NewGuid():N}".ToUpperInvariant()[..20],
            IsReviewed = isReviewed,
            IsDismissed = isDismissed
        };

        // Set event-specific data
        SetEventSpecificData(behaviorEvent, eventType);

        // Add review info if reviewed
        if (isReviewed)
        {
            behaviorEvent.ReviewedAt = eventDate.AddHours(random.Next(1, 72));
            behaviorEvent.ReviewNotes = random.Pick(reviewNotes);
        }

        return behaviorEvent;
    }

    private void SetEventSpecificData(DriverBehaviorEvent evt, DriverBehaviorEventType eventType)
    {
        switch (eventType)
        {
            case DriverBehaviorEventType.Speeding:
                evt.SpeedMph = random.Next(70, 95);
                evt.SpeedLimitMph = random.Pick(new[] { 55, 60, 65, 70 });
                evt.DurationSeconds = random.Next(10, 300);
                break;

            case DriverBehaviorEventType.HarshBraking:
                evt.GForce = Math.Round(random.NextDouble() * 0.4 + 0.3, 2); // 0.3 - 0.7 G
                evt.DurationSeconds = random.Next(1, 5);
                break;

            case DriverBehaviorEventType.HarshAcceleration:
                evt.GForce = Math.Round(random.NextDouble() * 0.3 + 0.25, 2); // 0.25 - 0.55 G
                evt.DurationSeconds = random.Next(2, 8);
                break;

            case DriverBehaviorEventType.HarshCornering:
                evt.GForce = Math.Round(random.NextDouble() * 0.3 + 0.2, 2); // 0.2 - 0.5 G
                evt.SpeedMph = random.Next(25, 55);
                break;

            case DriverBehaviorEventType.Tailgating:
                evt.DurationSeconds = random.Next(5, 60);
                evt.SpeedMph = random.Next(45, 75);
                break;

            case DriverBehaviorEventType.DistractedDriving:
            case DriverBehaviorEventType.CellPhoneUse:
                evt.DurationSeconds = random.Next(3, 30);
                evt.SpeedMph = random.Next(30, 65);
                break;

            case DriverBehaviorEventType.Drowsiness:
                evt.DurationSeconds = random.Next(2, 10);
                break;

            case DriverBehaviorEventType.SeatbeltViolation:
                evt.DurationSeconds = random.Next(60, 600);
                break;

            case DriverBehaviorEventType.RollingStop:
                evt.SpeedMph = random.Next(3, 10);
                break;

            case DriverBehaviorEventType.ForwardCollisionWarning:
            case DriverBehaviorEventType.LaneDepartureWarning:
                evt.SpeedMph = random.Next(50, 75);
                break;

            case DriverBehaviorEventType.CameraObstruction:
                evt.DurationSeconds = random.Next(30, 3600);
                break;
        }
    }

    private DriverBehaviorEventType PickEventType()
    {
        // Weighted distribution - some events more common than others
        var roll = random.NextDouble();
        return roll switch
        {
            < 0.25 => DriverBehaviorEventType.HarshBraking,
            < 0.40 => DriverBehaviorEventType.Speeding,
            < 0.50 => DriverBehaviorEventType.HarshAcceleration,
            < 0.58 => DriverBehaviorEventType.HarshCornering,
            < 0.65 => DriverBehaviorEventType.DistractedDriving,
            < 0.72 => DriverBehaviorEventType.Tailgating,
            < 0.78 => DriverBehaviorEventType.ForwardCollisionWarning,
            < 0.84 => DriverBehaviorEventType.LaneDepartureWarning,
            < 0.88 => DriverBehaviorEventType.RollingStop,
            < 0.92 => DriverBehaviorEventType.CellPhoneUse,
            < 0.95 => DriverBehaviorEventType.SeatbeltViolation,
            < 0.97 => DriverBehaviorEventType.Drowsiness,
            _ => DriverBehaviorEventType.CameraObstruction
        };
    }
}
