using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.Dispatch;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.AI.Tools.Dispatch;
using Logistics.Mediator;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Dispatch;

public class GetDispatchCandidatesToolTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IDispatchEligibilityService _eligibility = Substitute.For<IDispatchEligibilityService>();
    private readonly ITenantRepository<Truck, Guid> _truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly ITenantRepository<DriverHosStatus, Guid> _hosRepo =
        Substitute.For<ITenantRepository<DriverHosStatus, Guid>>();
    private readonly ITenantRepository<Employee, Guid> _employeeRepo =
        Substitute.For<ITenantRepository<Employee, Guid>>();
    private readonly GetDispatchCandidatesTool _sut;

    /// <summary>Dallas - every load here picks up from it.</summary>
    private static readonly GeoPoint Pickup = new(-96.8, 32.78);

    public GetDispatchCandidatesToolTests()
    {
        _tenantUow.Repository<Truck>().Returns(_truckRepo);
        _tenantUow.Repository<DriverHosStatus>().Returns(_hosRepo);
        _tenantUow.Repository<Employee>().Returns(_employeeRepo);
        _tenantUow.GetCurrentTenant().Returns(TestTenants.Create());
        _sut = new GetDispatchCandidatesTool(_mediator, _tenantUow, _eligibility);
    }

    private static LoadDto Load(LoadType type = LoadType.GeneralFreight, double distanceMeters = 400_000)
    {
        var load = CopilotToolTestData.CreateLoad(deliveryCost: 1000m);
        load.Type = type;
        load.OriginLocation = Pickup;
        load.Distance = distanceMeters;
        return load;
    }

    private static (Truck Truck, Employee Driver, DriverHosStatus Hos) Truck(
        string number, TruckType type, GeoPoint at, int drivingMinutesLeft = 600)
    {
        var driver = new Employee { Id = Guid.NewGuid(), Email = $"{number}@test.com", FirstName = "Dana", LastName = number, Salary = Money.Zero("USD") };
        var truck = new Truck
        {
            Id = Guid.NewGuid(),
            Number = number,
            Type = type,
            Status = TruckStatus.Available,
            MainDriverId = driver.Id,
            CurrentLocation = at
        };
        var hos = new DriverHosStatus
        {
            EmployeeId = driver.Id,
            CurrentDutyStatus = DutyStatus.OnDutyNotDriving,
            DrivingMinutesRemaining = drivingMinutesLeft,
            OnDutyMinutesRemaining = 700,
            CycleMinutesRemaining = 3000
        };
        return (truck, driver, hos);
    }

    private void Setup(LoadDto[] loads, params (Truck Truck, Employee Driver, DriverHosStatus Hos)[] fleet)
    {
        _mediator.Send(Arg.Any<GetUnassignedLoadsQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<LoadDto>.Ok(loads, loads.Length, loads.Length));
        _truckRepo.GetListAsync(Arg.Any<Expression<Func<Truck, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([.. fleet.Select(f => f.Truck)]);
        _employeeRepo.GetListAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([.. fleet.Select(f => f.Driver)]);
        _hosRepo.GetListAsync(Arg.Any<Expression<Func<DriverHosStatus, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([.. fleet.Select(f => f.Hos)]);
    }

    private async Task<JsonElement> Run(JsonObject? input = null) =>
        JsonDocument.Parse(await _sut.ExecuteAsync(input ?? [], CancellationToken.None)).RootElement;

    [Fact]
    public async Task Execute_RanksCompatibleTrucksByDeadheadInTheTenantsUnit()
    {
        var near = Truck("T-near", TruckType.BoxTruck, new GeoPoint(-96.9, 32.8));
        var far = Truck("T-far", TruckType.BoxTruck, new GeoPoint(-97.7, 30.3));
        Setup([Load()], far, near);

        var candidates = (await Run()).GetProperty("loads")[0].GetProperty("candidates");

        Assert.Equal("T-near", candidates[0].GetProperty("truck_number").GetString());
        Assert.Equal("T-far", candidates[1].GetProperty("truck_number").GetString());
        Assert.True(candidates[0].GetProperty("deadhead_mi").GetDouble() < candidates[1].GetProperty("deadhead_mi").GetDouble());
        Assert.Equal("match", candidates[0].GetProperty("equipment").GetString());
        Assert.Equal("10h", candidates[0].GetProperty("driving_time_left").GetString());
    }

    [Fact]
    public async Task Execute_IncompatibleTypeAndShortHours_AreExcludedAndCounted()
    {
        // The two hard rules run in code, so the model never has to remember them.
        var carHauler = Truck("T-car", TruckType.CarHauler, Pickup);
        var tired = Truck("T-tired", TruckType.BoxTruck, Pickup, drivingMinutesLeft: 30);
        var ok = Truck("T-ok", TruckType.BoxTruck, Pickup);
        Setup([Load()], carHauler, tired, ok);

        var load = (await Run()).GetProperty("loads")[0];

        Assert.Equal("T-ok", Assert.Single(load.GetProperty("candidates").EnumerateArray())
            .GetProperty("truck_number").GetString());
        Assert.Equal(1, load.GetProperty("excluded").GetProperty("type_incompatible").GetInt32());
        Assert.Equal(1, load.GetProperty("excluded").GetProperty("hos_infeasible").GetInt32());
    }

    [Fact]
    public async Task Execute_HazmatLoad_RunsEligibilityAndPutsBlockedCandidatesLast()
    {
        var blocked = Truck("T-blocked", TruckType.BoxTruck, Pickup);
        var clear = Truck("T-clear", TruckType.BoxTruck, new GeoPoint(-97.0, 32.9));
        var load = Load();
        load.HazmatClass = HazmatClass.Class3;
        Setup([load], blocked, clear);
        _eligibility.CheckAsync(blocked.Truck.Id, load.Id, null, Arg.Any<CancellationToken>())
            .Returns(EligibilityResult.Block(new EligibilityIssue(
                EligibilityIssueCode.MissingHazmatEndorsement, EligibilitySeverity.Error, "Driver lacks a hazmat endorsement")));
        _eligibility.CheckAsync(clear.Truck.Id, load.Id, null, Arg.Any<CancellationToken>())
            .Returns(EligibilityResult.Ok());

        var candidates = (await Run()).GetProperty("loads")[0].GetProperty("candidates");

        Assert.Equal("T-clear", candidates[0].GetProperty("truck_number").GetString());
        Assert.False(candidates[0].GetProperty("blocked").GetBoolean());
        Assert.True(candidates[1].GetProperty("blocked").GetBoolean());
        Assert.Equal("Driver lacks a hazmat endorsement", candidates[1].GetProperty("issues")[0].GetString());
    }

    [Fact]
    public async Task Execute_OrdinaryLoad_SkipsTheEligibilityCheck()
    {
        Setup([Load()], Truck("T-1", TruckType.BoxTruck, Pickup));

        await Run();

        await _eligibility.DidNotReceiveWithAnyArgs().CheckAsync(default, default, default, default);
    }

    [Fact]
    public async Task Execute_LoadIds_PassesThemToTheQuery()
    {
        // Filtered in the query, so a requested load past the first page is still planned.
        var wanted = Guid.NewGuid();
        Setup([]);

        await Run(new JsonObject { ["load_ids"] = new JsonArray(wanted.ToString()) });

        await _mediator.Received(1).Send(
            Arg.Is<GetUnassignedLoadsQuery>(q => q.LoadIds!.SequenceEqual(new[] { wanted })), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_AsksForMoreThanTheDefaultPageOfLoads()
    {
        // The query pages at 10 by default, which used to leave every load past the tenth unplanned.
        Setup([]);

        await Run();

        await _mediator.Received(1).Send(
            Arg.Is<GetUnassignedLoadsQuery>(q => q.PageSize > 10), Arg.Any<CancellationToken>());
    }
}
