using Logistics.Infrastructure.AI.Tools.Dispatch;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.Dispatch;
using Logistics.Infrastructure.AI.Tools;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Dispatch;

public class CheckDispatchEligibilityToolTests
{
    private readonly IDispatchEligibilityService _eligibilityService =
        Substitute.For<IDispatchEligibilityService>();

    private readonly CheckDispatchEligibilityTool _sut;

    public CheckDispatchEligibilityToolTests()
    {
        _sut = new CheckDispatchEligibilityTool(_eligibilityService);
    }

    [Fact]
    public async Task Execute_HappyPath_PassesIdsToServiceAndShapesResponse()
    {
        var truckId = Guid.NewGuid();
        var loadId = Guid.NewGuid();
        var driverId = Guid.NewGuid();

        _eligibilityService
            .CheckAsync(truckId, loadId, driverId, Arg.Any<CancellationToken>())
            .Returns(EligibilityResult.Ok());

        var input = new JsonObject
        {
            ["truck_id"] = truckId.ToString(),
            ["load_id"] = loadId.ToString(),
            ["driver_id"] = driverId.ToString()
        };

        var result = await _sut.ExecuteAsync(input, CancellationToken.None);
        var root = JsonDocument.Parse(result).RootElement;

        Assert.True(root.GetProperty("is_eligible").GetBoolean());
        Assert.Empty(root.GetProperty("issues").EnumerateArray());

        await _eligibilityService.Received(1)
            .CheckAsync(truckId, loadId, driverId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_OmittedDriverId_DefersToTruckMainDriver()
    {
        var truckId = Guid.NewGuid();
        var loadId = Guid.NewGuid();

        _eligibilityService
            .CheckAsync(truckId, loadId, null, Arg.Any<CancellationToken>())
            .Returns(EligibilityResult.Ok());

        var input = new JsonObject
        {
            ["truck_id"] = truckId.ToString(),
            ["load_id"] = loadId.ToString()
        };

        var result = await _sut.ExecuteAsync(input, CancellationToken.None);

        Assert.Contains("\"is_eligible\":true", result);
        await _eligibilityService.Received(1)
            .CheckAsync(truckId, loadId, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_IneligibleResult_SerializesIssuesWithLowercaseSeverity()
    {
        var truckId = Guid.NewGuid();
        var loadId = Guid.NewGuid();

        _eligibilityService
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

        var result = await _sut.ExecuteAsync(input, CancellationToken.None);
        var root = JsonDocument.Parse(result).RootElement;

        Assert.False(root.GetProperty("is_eligible").GetBoolean());
        var issues = root.GetProperty("issues").EnumerateArray().ToList();
        Assert.Single(issues);
        Assert.Equal("MissingHazmatEndorsement", issues[0].GetProperty("code").GetString());
        Assert.Equal("error", issues[0].GetProperty("severity").GetString());
        Assert.Contains("Hazmat", issues[0].GetProperty("message").GetString());
    }
}
