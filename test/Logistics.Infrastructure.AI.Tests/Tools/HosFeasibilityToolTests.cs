using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Tools;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

/// <summary>
/// Pins the shared feasibility verdict AND the two tools' deliberately different payloads:
/// the single-shot tool emits <c>is_in_violation</c>, the batch tool emits <c>driver_id</c> /
/// <c>distance_km</c> and nullable remaining-minutes. Extracting the shared algorithm must not
/// converge the two shapes.
/// </summary>
public class HosFeasibilityToolTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<DriverHosStatus, Guid> hosRepo =
        Substitute.For<ITenantRepository<DriverHosStatus, Guid>>();

    public HosFeasibilityToolTests()
    {
        tenantUow.Repository<DriverHosStatus>().Returns(hosRepo);
    }

    private static DriverHosStatus Hos(
        Guid driverId, int drivingMinutes, int onDutyMinutes, bool inViolation = false) => new()
        {
            EmployeeId = driverId,
            CurrentDutyStatus = DutyStatus.OnDutyNotDriving,
            DrivingMinutesRemaining = drivingMinutes,
            OnDutyMinutesRemaining = onDutyMinutes,
            IsInViolation = inViolation
        };

    private void SingleReturns(DriverHosStatus? hos) =>
        hosRepo.GetAsync(Arg.Any<Expression<Func<DriverHosStatus, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(hos);

    private void BatchReturns(params DriverHosStatus[] statuses) =>
        hosRepo.GetListAsync(Arg.Any<Expression<Func<DriverHosStatus, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([.. statuses]);

    private Task<string> RunSingle(Guid driverId, double distanceKm) =>
        new CheckHosFeasibilityTool(tenantUow).ExecuteAsync(
            new JsonObject { ["driver_id"] = driverId.ToString(), ["distance_km"] = distanceKm },
            CancellationToken.None);

    private Task<string> RunBatch(params (Guid DriverId, double DistanceKm)[] checks) =>
        new BatchCheckHosFeasibilityTool(tenantUow).ExecuteAsync(
            new JsonObject
            {
                ["checks"] = new JsonArray([.. checks.Select(c => (JsonNode)new JsonObject
                {
                    ["driver_id"] = c.DriverId.ToString(),
                    ["distance_km"] = c.DistanceKm
                })])
            },
            CancellationToken.None);

    #region Shared verdict logic

    [Fact]
    public async Task BothTools_AmpleHours_AgreeOnFeasibleAndReason()
    {
        var driverId = Guid.NewGuid();
        // 400km at the assumed 80km/h = 300 minutes of driving.
        SingleReturns(Hos(driverId, 600, 660));
        BatchReturns(Hos(driverId, 600, 660));

        var single = JsonDocument.Parse(await RunSingle(driverId, 400)).RootElement;
        var batch = JsonDocument.Parse(await RunBatch((driverId, 400))).RootElement
            .GetProperty("results")[0];

        Assert.True(single.GetProperty("feasible").GetBoolean());
        Assert.True(batch.GetProperty("feasible").GetBoolean());
        Assert.Equal(300, single.GetProperty("estimated_driving_minutes").GetInt32());
        Assert.Equal(300, batch.GetProperty("estimated_driving_minutes").GetInt32());
        Assert.Equal(
            "Driver has sufficient hours to complete in one stretch",
            single.GetProperty("reason").GetString());
        Assert.Equal(
            single.GetProperty("reason").GetString(),
            batch.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task BothTools_InViolation_AgreeOnReason()
    {
        var driverId = Guid.NewGuid();
        SingleReturns(Hos(driverId, 600, 660, inViolation: true));
        BatchReturns(Hos(driverId, 600, 660, inViolation: true));

        var single = JsonDocument.Parse(await RunSingle(driverId, 400)).RootElement;
        var batch = JsonDocument.Parse(await RunBatch((driverId, 400))).RootElement
            .GetProperty("results")[0];

        Assert.False(single.GetProperty("feasible").GetBoolean());
        Assert.False(single.GetProperty("feasible_multi_day").GetBoolean());
        Assert.False(batch.GetProperty("feasible_multi_day").GetBoolean());
        Assert.Equal("Driver is currently in HOS violation", single.GetProperty("reason").GetString());
        Assert.Equal("Driver is currently in HOS violation", batch.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task BothTools_PartialHoursAbove2h_AgreeOnMultiDay()
    {
        var driverId = Guid.NewGuid();
        SingleReturns(Hos(driverId, 200, 300));
        BatchReturns(Hos(driverId, 200, 300));

        var single = JsonDocument.Parse(await RunSingle(driverId, 400)).RootElement;
        var batch = JsonDocument.Parse(await RunBatch((driverId, 400))).RootElement
            .GetProperty("results")[0];

        Assert.False(single.GetProperty("feasible").GetBoolean());
        Assert.True(single.GetProperty("feasible_multi_day").GetBoolean());
        Assert.True(batch.GetProperty("feasible_multi_day").GetBoolean());
        Assert.Contains("multi-day trip with rest stops", single.GetProperty("reason").GetString());
        Assert.Equal(
            single.GetProperty("reason").GetString(),
            batch.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task BothTools_BelowTwoHourFloor_AgreeOnInsufficient()
    {
        var driverId = Guid.NewGuid();
        SingleReturns(Hos(driverId, 90, 90));
        BatchReturns(Hos(driverId, 90, 90));

        var single = JsonDocument.Parse(await RunSingle(driverId, 400)).RootElement;
        var batch = JsonDocument.Parse(await RunBatch((driverId, 400))).RootElement
            .GetProperty("results")[0];

        Assert.False(single.GetProperty("feasible_multi_day").GetBoolean());
        Assert.False(batch.GetProperty("feasible_multi_day").GetBoolean());
        Assert.Contains("too low to make meaningful progress", single.GetProperty("reason").GetString());
        Assert.Equal(
            single.GetProperty("reason").GetString(),
            batch.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task BothTools_NoHosRow_AgreeOnReason()
    {
        var driverId = Guid.NewGuid();
        SingleReturns(null);
        BatchReturns();

        var single = JsonDocument.Parse(await RunSingle(driverId, 400)).RootElement;
        var batch = JsonDocument.Parse(await RunBatch((driverId, 400))).RootElement
            .GetProperty("results")[0];

        Assert.Equal("No HOS data available for this driver", single.GetProperty("reason").GetString());
        Assert.Equal("No HOS data available for this driver", batch.GetProperty("reason").GetString());
        Assert.False(single.GetProperty("feasible").GetBoolean());
        Assert.False(batch.GetProperty("feasible").GetBoolean());
    }

    #endregion

    #region Payload shapes must stay distinct

    [Fact]
    public async Task Single_EmitsIsInViolation_AndNoDriverIdOrDistance()
    {
        var driverId = Guid.NewGuid();
        SingleReturns(Hos(driverId, 600, 660, inViolation: true));

        var root = JsonDocument.Parse(await RunSingle(driverId, 400)).RootElement;

        Assert.True(root.GetProperty("is_in_violation").GetBoolean());
        Assert.False(root.TryGetProperty("driver_id", out _));
        Assert.False(root.TryGetProperty("distance_km", out _));
    }

    [Fact]
    public async Task Batch_EmitsDriverIdAndDistance_AndNoIsInViolation()
    {
        var driverId = Guid.NewGuid();
        BatchReturns(Hos(driverId, 600, 660, inViolation: true));

        var item = JsonDocument.Parse(await RunBatch((driverId, 400))).RootElement
            .GetProperty("results")[0];

        Assert.Equal(driverId.ToString(), item.GetProperty("driver_id").GetString());
        Assert.Equal(400, item.GetProperty("distance_km").GetDouble());
        Assert.False(item.TryGetProperty("is_in_violation", out _));
    }

    [Fact]
    public async Task Batch_MissingHosRow_EmitsNullRemainingMinutes()
    {
        var driverId = Guid.NewGuid();
        BatchReturns();

        var item = JsonDocument.Parse(await RunBatch((driverId, 400))).RootElement
            .GetProperty("results")[0];

        Assert.Equal(JsonValueKind.Null, item.GetProperty("driving_minutes_remaining").ValueKind);
        Assert.Equal(JsonValueKind.Null, item.GetProperty("on_duty_minutes_remaining").ValueKind);
    }

    [Fact]
    public async Task Batch_SeveralChecks_EmitsCountAndOneResultPerCheck()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        BatchReturns(Hos(first, 600, 660), Hos(second, 90, 90));

        var root = JsonDocument.Parse(await RunBatch((first, 400), (second, 400))).RootElement;

        Assert.Equal(2, root.GetProperty("count").GetInt32());
        Assert.Equal(2, root.GetProperty("results").GetArrayLength());
        Assert.True(root.GetProperty("results")[0].GetProperty("feasible").GetBoolean());
        Assert.False(root.GetProperty("results")[1].GetProperty("feasible").GetBoolean());
    }

    [Fact]
    public async Task Batch_QueriesHosOnceForAllDrivers()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        BatchReturns(Hos(first, 600, 660), Hos(second, 600, 660));

        await RunBatch((first, 400), (second, 400));

        await hosRepo.Received(1).GetListAsync(
            Arg.Any<Expression<Func<DriverHosStatus, bool>>>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Input guards

    [Fact]
    public async Task Single_MissingDriverId_EmitsError()
    {
        var result = await new CheckHosFeasibilityTool(tenantUow).ExecuteAsync(
            new JsonObject(), CancellationToken.None);

        Assert.Equal(
            "Invalid or missing driver_id",
            JsonDocument.Parse(result).RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Batch_EmptyChecks_EmitsError()
    {
        var result = await new BatchCheckHosFeasibilityTool(tenantUow).ExecuteAsync(
            new JsonObject { ["checks"] = new JsonArray() }, CancellationToken.None);

        Assert.Equal(
            "Missing or empty 'checks' array",
            JsonDocument.Parse(result).RootElement.GetProperty("error").GetString());
    }

    [Theory]
    [InlineData("400")]
    [InlineData("400.0")]
    public async Task BothTools_DistanceEmittedAsString_CoercesInsteadOfThrowing(string distance)
    {
        var driverId = Guid.NewGuid();
        SingleReturns(Hos(driverId, 600, 660));
        BatchReturns(Hos(driverId, 600, 660));

        var single = JsonDocument.Parse(await new CheckHosFeasibilityTool(tenantUow).ExecuteAsync(
            new JsonObject { ["driver_id"] = driverId.ToString(), ["distance_km"] = distance },
            CancellationToken.None)).RootElement;

        var batch = JsonDocument.Parse(await new BatchCheckHosFeasibilityTool(tenantUow).ExecuteAsync(
            new JsonObject
            {
                ["checks"] = new JsonArray(new JsonObject
                {
                    ["driver_id"] = driverId.ToString(),
                    ["distance_km"] = distance
                })
            },
            CancellationToken.None)).RootElement.GetProperty("results")[0];

        Assert.Equal(300, single.GetProperty("estimated_driving_minutes").GetInt32());
        Assert.Equal(300, batch.GetProperty("estimated_driving_minutes").GetInt32());
    }

    [Fact]
    public async Task Batch_AllDriverIdsUnparseable_EmitsNoValidChecks()
    {
        var result = await new BatchCheckHosFeasibilityTool(tenantUow).ExecuteAsync(
            new JsonObject
            {
                ["checks"] = new JsonArray(new JsonObject { ["driver_id"] = "not-a-guid" })
            },
            CancellationToken.None);

        Assert.Equal(
            "No valid checks provided",
            JsonDocument.Parse(result).RootElement.GetProperty("error").GetString());
    }

    #endregion

    [Fact]
    public void Names_AreSnakeCase()
    {
        Assert.Equal("check_hos_feasibility", new CheckHosFeasibilityTool(tenantUow).Name);
        Assert.Equal("batch_check_hos_feasibility", new BatchCheckHosFeasibilityTool(tenantUow).Name);
    }
}
