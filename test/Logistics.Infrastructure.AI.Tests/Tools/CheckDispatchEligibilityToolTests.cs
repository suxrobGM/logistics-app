using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.Dispatch;
using Logistics.Infrastructure.AI.Tools;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class CheckDispatchEligibilityToolTests
{
    private readonly IDispatchEligibilityService eligibilityService =
        Substitute.For<IDispatchEligibilityService>();

    private readonly CheckDispatchEligibilityTool sut;

    public CheckDispatchEligibilityToolTests()
    {
        sut = new CheckDispatchEligibilityTool(eligibilityService);
    }

    [Fact]
    public void Name_IsSnakeCase()
    {
        Assert.Equal("check_dispatch_eligibility", sut.Name);
    }

    [Fact]
    public async Task Execute_MissingTruckId_ReturnsError()
    {
        var input = new JsonObject
        {
            ["load_id"] = Guid.NewGuid().ToString()
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        Assert.Contains("truck_id", root.GetProperty("error").GetString());
        await eligibilityService.DidNotReceiveWithAnyArgs()
            .CheckAsync(default, default, default, default);
    }

    [Fact]
    public async Task Execute_InvalidTruckId_ReturnsError()
    {
        var input = new JsonObject
        {
            ["truck_id"] = "not-a-guid",
            ["load_id"] = Guid.NewGuid().ToString()
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);

        Assert.Contains("truck_id", result);
    }

    [Fact]
    public async Task Execute_MissingLoadId_ReturnsError()
    {
        var input = new JsonObject
        {
            ["truck_id"] = Guid.NewGuid().ToString()
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);

        Assert.Contains("load_id", result);
    }

    [Fact]
    public async Task Execute_InvalidDriverId_ReturnsError()
    {
        var input = new JsonObject
        {
            ["truck_id"] = Guid.NewGuid().ToString(),
            ["load_id"] = Guid.NewGuid().ToString(),
            ["driver_id"] = "garbage"
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);

        Assert.Contains("driver_id", result);
    }

    [Fact]
    public async Task Execute_HappyPath_PassesIdsToServiceAndShapesResponse()
    {
        var truckId = Guid.NewGuid();
        var loadId = Guid.NewGuid();
        var driverId = Guid.NewGuid();

        eligibilityService
            .CheckAsync(truckId, loadId, driverId, Arg.Any<CancellationToken>())
            .Returns(EligibilityResult.Ok());

        var input = new JsonObject
        {
            ["truck_id"] = truckId.ToString(),
            ["load_id"] = loadId.ToString(),
            ["driver_id"] = driverId.ToString()
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var root = JsonDocument.Parse(result).RootElement;

        Assert.True(root.GetProperty("is_eligible").GetBoolean());
        Assert.Empty(root.GetProperty("issues").EnumerateArray());

        await eligibilityService.Received(1)
            .CheckAsync(truckId, loadId, driverId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_OmittedDriverId_DefersToTruckMainDriver()
    {
        var truckId = Guid.NewGuid();
        var loadId = Guid.NewGuid();

        eligibilityService
            .CheckAsync(truckId, loadId, null, Arg.Any<CancellationToken>())
            .Returns(EligibilityResult.Ok());

        var input = new JsonObject
        {
            ["truck_id"] = truckId.ToString(),
            ["load_id"] = loadId.ToString()
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);

        Assert.Contains("\"is_eligible\":true", result);
        await eligibilityService.Received(1)
            .CheckAsync(truckId, loadId, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_IneligibleResult_SerializesIssuesWithLowercaseSeverity()
    {
        var truckId = Guid.NewGuid();
        var loadId = Guid.NewGuid();

        eligibilityService
            .CheckAsync(truckId, loadId, null, Arg.Any<CancellationToken>())
            .Returns(new EligibilityResult(false, [
                new EligibilityIssue(
                    EligibilityIssueCode.MissingHazmatEndorsement,
                    EligibilitySeverity.Error,
                    "US driver requires Hazmat (H) endorsement for this load.")
            ]));

        var input = new JsonObject
        {
            ["truck_id"] = truckId.ToString(),
            ["load_id"] = loadId.ToString()
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var root = JsonDocument.Parse(result).RootElement;

        Assert.False(root.GetProperty("is_eligible").GetBoolean());
        var issues = root.GetProperty("issues").EnumerateArray().ToList();
        Assert.Single(issues);
        Assert.Equal("MissingHazmatEndorsement", issues[0].GetProperty("code").GetString());
        Assert.Equal("error", issues[0].GetProperty("severity").GetString());
        Assert.Contains("Hazmat", issues[0].GetProperty("message").GetString());
    }
}
