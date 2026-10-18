using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.AI.Tools.Operations;
using Logistics.Mediator;
using Logistics.Shared.Models;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Operations;

public class GetDetentionToolTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<TripStop, Guid> _stopRepo = Substitute.For<ITenantRepository<TripStop, Guid>>();
    private readonly GetDetentionTool _sut;

    public GetDetentionToolTests()
    {
        _tenantUow.Repository<TripStop>().Returns(_stopRepo);
        _tenantUow.GetCurrentTenant().Returns(new Tenant
        {
            Name = "Test Tenant",
            ConnectionString = "test-connection",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "123 Test St", City = "Test", State = "TX", ZipCode = "12345", Country = "US" }
        });
        _sut = new GetDetentionTool(_mediator, _tenantUow);
    }

    [Fact]
    public async Task Execute_WithLoadId_ReturnsThatLoadsStops()
    {
        var loadId = Guid.NewGuid();
        _mediator.Send(Arg.Any<GetLoadDetentionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<List<StopDetentionDto>>.Ok(
            [
                new StopDetentionDto
                {
                    StopType = TripStopType.DropOff,
                    ArrivedAt = DateTime.UtcNow.AddHours(-4),
                    DepartedAt = DateTime.UtcNow,
                    DwellMinutes = 240,
                    FreeMinutes = 120,
                    OverMinutes = 120,
                    Charge = 150m,
                    IsBilled = true
                }
            ]));

        var result = await _sut.ExecuteAsync(new JsonObject { ["load_id"] = loadId.ToString() }, CancellationToken.None);

        var stop = Assert.Single(JsonDocument.Parse(result).RootElement.GetProperty("stops").EnumerateArray());
        Assert.Equal(120, stop.GetProperty("over_minutes").GetInt32());
        Assert.True(stop.GetProperty("is_billed").GetBoolean());
        await _mediator.Received(1).Send(
            Arg.Is<GetLoadDetentionQuery>(q => q.LoadId == loadId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WithoutLoadId_ListsWaitingTrucksMostOverdueFirst()
    {
        var shortWait = Stop("T-1", arrivedHoursAgo: 1);
        var longWait = Stop("T-2", arrivedHoursAgo: 5);
        _stopRepo.Query().Returns(new List<TripStop> { shortWait, longWait }.BuildMock());

        var result = await _sut.ExecuteAsync(new JsonObject(), CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        var waiting = root.GetProperty("waiting").EnumerateArray().ToList();
        Assert.Equal(2, waiting.Count);
        Assert.Equal("T-2", waiting[0].GetProperty("truck_number").GetString());
        Assert.True(waiting[0].GetProperty("over_minutes").GetInt32() > 0);
        Assert.Equal(0, waiting[1].GetProperty("over_minutes").GetInt32());
        await _mediator.DidNotReceiveWithAnyArgs().Send<Result<List<StopDetentionDto>>>(default!, default);
    }

    private static TripStop Stop(string truckNumber, double arrivedHoursAgo)
    {
        var address = new Address { Line1 = "1 Dock", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US" };
        var load = new Load
        {
            Name = $"Load for {truckNumber}",
            Type = LoadType.GeneralFreight,
            Customer = null!,
            OriginAddress = address,
            OriginLocation = new GeoPoint(0, 0),
            DestinationAddress = address,
            DestinationLocation = new GeoPoint(0, 0),
            DeliveryCost = Money.Zero("USD")
        };
        var trip = new Trip { Name = "Trip", Truck = new Truck { Number = truckNumber, Type = TruckType.FreightTruck } };
        return new TripStop
        {
            Type = TripStopType.PickUp,
            Trip = trip,
            Load = load,
            LoadId = load.Id,
            Address = address,
            Location = new GeoPoint(0, 0),
            ArrivedAt = DateTime.UtcNow.AddHours(-arrivedHoursAgo)
        };
    }
}
