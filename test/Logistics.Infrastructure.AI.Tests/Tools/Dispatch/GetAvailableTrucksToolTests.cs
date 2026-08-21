using Logistics.Infrastructure.AI.Tools.Dispatch;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.AI.Tools;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Dispatch;

/// <summary>
/// Pins the payload of the tool the system prompt tells the agent to call first on every run.
/// The driver lookup is being moved off the lazy-loaded <c>Truck.MainDriver</c> navigation onto a
/// batched query, so the emitted shape - including the null arms - has to be nailed down first.
/// </summary>
public class GetAvailableTrucksToolTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Truck, Guid> truckRepo =
        Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly ITenantRepository<Trip, Guid> tripRepo =
        Substitute.For<ITenantRepository<Trip, Guid>>();
    private readonly ITenantRepository<DriverHosStatus, Guid> hosRepo =
        Substitute.For<ITenantRepository<DriverHosStatus, Guid>>();
    private readonly ITenantRepository<Employee, Guid> employeeRepo =
        Substitute.For<ITenantRepository<Employee, Guid>>();
    private readonly GetAvailableTrucksTool sut;

    public GetAvailableTrucksToolTests()
    {
        tenantUow.Repository<Truck>().Returns(truckRepo);
        tenantUow.Repository<Trip>().Returns(tripRepo);
        tenantUow.Repository<DriverHosStatus>().Returns(hosRepo);
        tenantUow.Repository<Employee>().Returns(employeeRepo);
        sut = new GetAvailableTrucksTool(tenantUow);
    }

    private static Employee Driver(Guid id) => new()
    {
        Id = id,
        Email = "driver@test.com",
        FirstName = "Dana",
        LastName = "Reyes"
    };

    private static Truck AvailableTruck(string number, Employee? driver) => new()
    {
        Id = Guid.NewGuid(),
        Number = number,
        Type = TruckType.Flatbed,
        Status = TruckStatus.Available,
        MainDriver = driver,
        MainDriverId = driver?.Id,
        CurrentLocation = new GeoPoint(-96.8, 32.78),
        CurrentAddress = new Address
        {
            Line1 = "500 Depot Rd",
            City = "Dallas",
            State = "TX",
            ZipCode = "75201",
            Country = "US"
        }
    };

    private static DriverHosStatus Hos(Guid driverId, bool inViolation = false) => new()
    {
        EmployeeId = driverId,
        CurrentDutyStatus = DutyStatus.OnDutyNotDriving,
        DrivingMinutesRemaining = 420,
        OnDutyMinutesRemaining = 500,
        CycleMinutesRemaining = 2400,
        IsInViolation = inViolation
    };

    private void Setup(List<Truck> trucks, List<DriverHosStatus> hos, int totalTrucks = 10, int activeTrips = 3)
    {
        truckRepo.GetListAsync(Arg.Any<Expression<Func<Truck, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(trucks);
        truckRepo.CountAsync(null, Arg.Any<CancellationToken>()).Returns(totalTrucks);
        tripRepo.CountAsync(Arg.Any<Expression<Func<Trip, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(activeTrips);
        hosRepo.GetListAsync(Arg.Any<Expression<Func<DriverHosStatus, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(hos);
        employeeRepo.GetListAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([.. trucks.Where(t => t.MainDriver is not null).Select(t => t.MainDriver!)]);
    }

    private async Task<JsonElement> Run()
    {
        var result = await sut.ExecuteAsync(new JsonObject(), CancellationToken.None);
        return JsonDocument.Parse(result).RootElement;
    }

    [Fact]
    public async Task Execute_TruckWithDriverAndHos_ShapesNestedDriverAndHos()
    {
        var driver = Driver(Guid.NewGuid());
        var truck = AvailableTruck("T-100", driver);
        Setup([truck], [Hos(driver.Id)]);

        var root = await Run();
        var truckJson = root.GetProperty("trucks")[0];

        Assert.Equal(truck.Id, truckJson.GetProperty("id").GetGuid());
        Assert.Equal("T-100", truckJson.GetProperty("number").GetString());
        Assert.Equal("Flatbed", truckJson.GetProperty("type").GetString());
        Assert.Equal(32.78, truckJson.GetProperty("current_lat").GetDouble(), 3);
        Assert.Equal(-96.8, truckJson.GetProperty("current_lng").GetDouble(), 3);
        Assert.Contains("Dallas", truckJson.GetProperty("current_address").GetString());

        var driverJson = truckJson.GetProperty("main_driver");
        Assert.Equal(driver.Id, driverJson.GetProperty("id").GetGuid());
        Assert.Equal("Dana Reyes", driverJson.GetProperty("name").GetString());

        var hosJson = driverJson.GetProperty("hos");
        Assert.Equal(420, hosJson.GetProperty("driving_minutes_remaining").GetInt32());
        Assert.Equal(500, hosJson.GetProperty("on_duty_minutes_remaining").GetInt32());
        Assert.Equal(2400, hosJson.GetProperty("cycle_minutes_remaining").GetInt32());
        Assert.False(hosJson.GetProperty("is_in_violation").GetBoolean());
        Assert.True(hosJson.GetProperty("is_available").GetBoolean());
    }

    [Fact]
    public async Task Execute_DriverWithoutHosRow_EmitsNullHos()
    {
        var driver = Driver(Guid.NewGuid());
        Setup([AvailableTruck("T-200", driver)], []);

        var root = await Run();
        var driverJson = root.GetProperty("trucks")[0].GetProperty("main_driver");

        Assert.Equal("Dana Reyes", driverJson.GetProperty("name").GetString());
        Assert.Equal(JsonValueKind.Null, driverJson.GetProperty("hos").ValueKind);
    }

    [Fact]
    public async Task Execute_TruckWithoutDriver_EmitsNullMainDriver()
    {
        Setup([AvailableTruck("T-300", null)], []);

        var root = await Run();

        Assert.Equal(
            JsonValueKind.Null,
            root.GetProperty("trucks")[0].GetProperty("main_driver").ValueKind);
    }

    [Fact]
    public async Task Execute_EmitsFleetSummary()
    {
        var inViolation = Driver(Guid.NewGuid());
        var healthy = Driver(Guid.NewGuid());
        Setup(
            [AvailableTruck("T-1", inViolation), AvailableTruck("T-2", healthy)],
            [Hos(inViolation.Id, inViolation: true), Hos(healthy.Id)],
            totalTrucks: 12,
            activeTrips: 5);

        var root = await Run();
        var summary = root.GetProperty("fleet_summary");

        Assert.Equal(2, root.GetProperty("count").GetInt32());
        Assert.Equal(12, summary.GetProperty("total_trucks").GetInt32());
        Assert.Equal(2, summary.GetProperty("available_trucks").GetInt32());
        Assert.Equal(5, summary.GetProperty("active_trips").GetInt32());
        Assert.Equal(1, summary.GetProperty("drivers_in_violation").GetInt32());
    }

    [Fact]
    public async Task Execute_NoAvailableTrucks_EmitsEmptyListAndZeroCount()
    {
        Setup([], [], totalTrucks: 4, activeTrips: 4);

        var root = await Run();

        Assert.Equal(0, root.GetProperty("trucks").GetArrayLength());
        Assert.Equal(0, root.GetProperty("count").GetInt32());
        Assert.Equal(0, root.GetProperty("fleet_summary").GetProperty("available_trucks").GetInt32());
    }

    [Fact]
    public async Task Execute_ManyTrucks_BatchesHosAndDriversIntoOneQueryEach()
    {
        var drivers = Enumerable.Range(0, 5).Select(_ => Driver(Guid.NewGuid())).ToList();
        Setup(
            [.. drivers.Select((d, i) => AvailableTruck($"T-{i}", d))],
            [.. drivers.Select(d => Hos(d.Id))]);

        await Run();

        await hosRepo.Received(1).GetListAsync(
            Arg.Any<Expression<Func<DriverHosStatus, bool>>>(), Arg.Any<CancellationToken>());

        // Truck.MainDriver is a lazy navigation - reading it per truck would be five extra
        // SELECTs here and one per truck in a real fleet.
        await employeeRepo.Received(1).GetListAsync(
            Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_NoTrucksHaveDrivers_SkipsTheDriverQueryEntirely()
    {
        Setup([AvailableTruck("T-1", null), AvailableTruck("T-2", null)], []);

        await Run();

        await employeeRepo.DidNotReceive().GetListAsync(
            Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>());
    }
}
