using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Compliance.Inspections.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Tools.Compliance;
using Logistics.Mediator;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Compliance;

public class GetConditionReportsToolTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly GetConditionReportsTool _sut;

    public GetConditionReportsToolTests()
    {
        _sut = new GetConditionReportsTool(_mediator);
    }

    [Fact]
    public async Task Execute_MissingLoadId_ReturnsError()
    {
        var result = await _sut.ExecuteAsync(new JsonObject(), CancellationToken.None);

        Assert.Contains("\"error\"", result);
        await _mediator.DidNotReceiveWithAnyArgs().Send<Result<List<ConditionReportDto>>>(default!, default);
    }

    [Fact]
    public async Task Execute_VehicleReport_DescribesVehicleAndDefectsWithBolCodes()
    {
        var loadId = Guid.NewGuid();
        _mediator.Send(Arg.Any<GetConditionReportsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<List<ConditionReportDto>>.Ok(
            [
                new ConditionReportDto
                {
                    LoadId = loadId,
                    Type = InspectionType.Delivery,
                    Vin = "1HGCM82633A004352",
                    VehicleYear = 2021,
                    VehicleMake = "Toyota",
                    VehicleModel = "Camry",
                    HasSignature = true,
                    InspectorName = "Dana Reyes",
                    InspectedAt = new DateTime(2026, 10, 17, 14, 30, 0, DateTimeKind.Utc),
                    Defects =
                    [
                        new ConditionDefectDto
                        {
                            PartCategoryDisplay = "Body",
                            ZoneDisplay = "Front bumper",
                            DamageCode = "D",
                            SeverityDisplay = "Minor",
                            Description = "3 inch dent"
                        }
                    ]
                }
            ]));

        var result = await _sut.ExecuteAsync(new JsonObject { ["load_id"] = loadId.ToString() }, CancellationToken.None);

        var report = Assert.Single(JsonDocument.Parse(result).RootElement.GetProperty("reports").EnumerateArray());
        Assert.Equal("Delivery", report.GetProperty("type").GetString());
        Assert.Equal("2021 Toyota Camry VIN 1HGCM82633A004352", report.GetProperty("vehicle").GetString());
        Assert.Equal("Front bumper: D, Minor - 3 inch dent", report.GetProperty("defects")[0].GetString());
        Assert.True(report.GetProperty("signed").GetBoolean());
        await _mediator.Received(1).Send(
            Arg.Is<GetConditionReportsQuery>(q => q.LoadId == loadId), Arg.Any<CancellationToken>());
    }
}
