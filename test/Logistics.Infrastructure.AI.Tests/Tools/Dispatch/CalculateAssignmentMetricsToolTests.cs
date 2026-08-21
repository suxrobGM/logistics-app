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

public class CalculateAssignmentMetricsToolTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Load, Guid> loadRepo =
        Substitute.For<ITenantRepository<Load, Guid>>();
    private readonly ITenantRepository<Truck, Guid> truckRepo =
        Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly CalculateAssignmentMetricsTool sut;

    public CalculateAssignmentMetricsToolTests()
    {
        tenantUow.Repository<Load>().Returns(loadRepo);
        tenantUow.Repository<Truck>().Returns(truckRepo);
        sut = new CalculateAssignmentMetricsTool(tenantUow);
    }

    private static Address SomeAddress => new()
    {
        Line1 = "1 Depot", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US"
    };

    /// <param name="distanceMeters">Load.Distance is stored in metres.</param>
    private static Load CreateLoad(Guid id, string name, double distanceMeters, decimal deliveryCost)
    {
        return new Load
        {
            Id = id,
            Name = name,
            Type = LoadType.GeneralFreight,
            Customer = null!,
            OriginAddress = SomeAddress,
            // Same point as the truck below, so deadhead is zero unless a test moves it.
            OriginLocation = new GeoPoint(-96.8, 32.78),
            DestinationAddress = SomeAddress,
            DestinationLocation = new GeoPoint(-95.36, 29.76),
            Distance = distanceMeters,
            DeliveryCost = new Money { Amount = deliveryCost, Currency = "USD" }
        };
    }

    private static Truck CreateTruck(Guid id, string number) => new()
    {
        Id = id,
        Number = number,
        Type = TruckType.Flatbed,
        CurrentLocation = new GeoPoint(-96.8, 32.78)
    };

    private void Setup(List<Load> loads, List<Truck> trucks)
    {
        loadRepo.GetListAsync(Arg.Any<Expression<Func<Load, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(loads);
        truckRepo.GetListAsync(Arg.Any<Expression<Func<Truck, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(trucks);
    }

    private static JsonObject Candidate(Guid loadId, Guid truckId) => new()
    {
        ["load_id"] = loadId.ToString(),
        ["truck_id"] = truckId.ToString()
    };

    private async Task<JsonElement> Run(params JsonNode[] candidates)
    {
        var result = await sut.ExecuteAsync(
            new JsonObject { ["candidates"] = new JsonArray(candidates) }, CancellationToken.None);
        return JsonDocument.Parse(result).RootElement;
    }

    [Fact]
    public async Task Execute_NoCandidates_ReturnsError()
    {
        var result = await sut.ExecuteAsync(
            new JsonObject { ["candidates"] = new JsonArray() }, CancellationToken.None);

        Assert.Equal(
            "Missing or empty candidates array",
            JsonDocument.Parse(result).RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Execute_SingleCandidate_EmitsSnakeCaseMetrics()
    {
        var loadId = Guid.NewGuid();
        var truckId = Guid.NewGuid();
        // 100km loaded, no deadhead => 62.1 miles, $1242.74 revenue => ~$20.00/mile
        Setup([CreateLoad(loadId, "Dallas run", 100_000, 1242.74m)], [CreateTruck(truckId, "T-1")]);

        var root = await Run(Candidate(loadId, truckId));
        var candidate = root.GetProperty("candidates")[0];

        Assert.Equal(1, root.GetProperty("count").GetInt32());
        Assert.Equal(loadId, candidate.GetProperty("load_id").GetGuid());
        Assert.Equal(truckId, candidate.GetProperty("truck_id").GetGuid());
        Assert.Equal("Dallas run", candidate.GetProperty("load_name").GetString());
        Assert.Equal("T-1", candidate.GetProperty("truck_number").GetString());
        Assert.Equal(0, candidate.GetProperty("deadhead_miles").GetDouble());
        Assert.Equal(62.1, candidate.GetProperty("loaded_miles").GetDouble(), 1);
        Assert.Equal(62.1, candidate.GetProperty("total_miles").GetDouble(), 1);
        Assert.Equal(1242.74, candidate.GetProperty("delivery_cost").GetDouble(), 2);
        Assert.Equal(20.0, candidate.GetProperty("revenue_per_mile").GetDouble(), 1);
        Assert.Equal(0, candidate.GetProperty("deadhead_ratio").GetDouble());
    }

    [Fact]
    public async Task Execute_SeveralCandidates_SortsByRevenuePerMileDescending()
    {
        var cheapLoad = Guid.NewGuid();
        var richLoad = Guid.NewGuid();
        var truckId = Guid.NewGuid();
        Setup(
            [CreateLoad(cheapLoad, "Cheap", 100_000, 500m), CreateLoad(richLoad, "Rich", 100_000, 5000m)],
            [CreateTruck(truckId, "T-1")]);

        var root = await Run(Candidate(cheapLoad, truckId), Candidate(richLoad, truckId));
        var candidates = root.GetProperty("candidates");

        Assert.Equal("Rich", candidates[0].GetProperty("load_name").GetString());
        Assert.Equal("Cheap", candidates[1].GetProperty("load_name").GetString());
    }

    [Fact]
    public async Task Execute_UnparseableId_FailsTheCallNamingTheProperty()
    {
        // One bad id fails the whole call: the agent picks a winner from these numbers, and a
        // silently dropped candidate is one it never compares.
        var result = await sut.ExecuteAsync(
            new JsonObject
            {
                ["candidates"] = new JsonArray(
                    new JsonObject { ["load_id"] = "not-a-guid", ["truck_id"] = "also-bad" })
            },
            CancellationToken.None);

        var error = JsonDocument.Parse(result).RootElement.GetProperty("error").GetString();
        Assert.Contains("load_id", error);
    }

    [Fact]
    public async Task Execute_MissingEntities_ReportsWhichOneIsMissing()
    {
        var loadId = Guid.NewGuid();
        var truckId = Guid.NewGuid();
        Setup([], [CreateTruck(truckId, "T-1")]);

        var root = await Run(Candidate(loadId, truckId));

        Assert.Equal("Load not found", root.GetProperty("candidates")[0].GetProperty("error").GetString());
    }

    [Fact]
    public async Task Execute_ManyCandidates_BatchesLoadsAndTrucksIntoOneQueryEach()
    {
        var truckId = Guid.NewGuid();
        var loadIds = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();
        Setup(
            [.. loadIds.Select((id, i) => CreateLoad(id, $"L-{i}", 100_000, 1000m))],
            [CreateTruck(truckId, "T-1")]);

        await Run([.. loadIds.Select(id => (JsonNode)Candidate(id, truckId))]);

        // The prompt asks the agent to score every competing pairing at once, so the previous
        // per-candidate GetByIdAsync pair was 20 sequential round trips for this input.
        await loadRepo.Received(1).GetListAsync(
            Arg.Any<Expression<Func<Load, bool>>>(), Arg.Any<CancellationToken>());
        await truckRepo.Received(1).GetListAsync(
            Arg.Any<Expression<Func<Truck, bool>>>(), Arg.Any<CancellationToken>());
        await loadRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await truckRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
