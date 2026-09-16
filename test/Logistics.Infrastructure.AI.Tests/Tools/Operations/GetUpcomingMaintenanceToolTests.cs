using Logistics.Infrastructure.AI.Tools.Operations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Operations.Maintenance.Queries;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Operations;

public class GetUpcomingMaintenanceToolTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly GetUpcomingMaintenanceTool _sut;

    public GetUpcomingMaintenanceToolTests()
    {
        _sut = new GetUpcomingMaintenanceTool(_mediator);
    }

    [Fact]
    public async Task Execute_DefaultsWindowTo30Days()
    {
        _mediator.Send(Arg.Any<GetUpcomingMaintenanceQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<List<MaintenanceScheduleDto>>.Ok([]));

        await _sut.ExecuteAsync(new JsonObject(), CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<GetUpcomingMaintenanceQuery>(q => q.DaysAhead == 30 && q.IncludeOverdue),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_ProjectsOverdueSchedules()
    {
        _mediator.Send(Arg.Any<GetUpcomingMaintenanceQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<List<MaintenanceScheduleDto>>.Ok(
                [new MaintenanceScheduleDto
                {
                    TruckId = Guid.NewGuid(),
                    TruckNumber = "TRK-101",
                    TypeDisplay = "Oil Change",
                    IsOverdue = true,
                    DaysUntilDue = -3
                }]));

        var result = await _sut.ExecuteAsync(
            new JsonObject { ["days_ahead"] = 14 }, CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        var schedule = Assert.Single(root.GetProperty("schedules").EnumerateArray());
        Assert.Equal("TRK-101", schedule.GetProperty("truck_number").GetString());
        Assert.True(schedule.GetProperty("is_overdue").GetBoolean());
    }
}
