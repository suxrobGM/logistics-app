using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Operations.Maintenance.Queries;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class GetUpcomingMaintenanceToolTests
{
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly GetUpcomingMaintenanceTool sut;

    public GetUpcomingMaintenanceToolTests()
    {
        sut = new GetUpcomingMaintenanceTool(mediator);
    }

    [Fact]
    public void Name_IsSnakeCase()
    {
        Assert.Equal("get_upcoming_maintenance", sut.Name);
    }

    [Fact]
    public async Task Execute_DefaultsWindowTo30Days()
    {
        mediator.Send(Arg.Any<GetUpcomingMaintenanceQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<List<MaintenanceScheduleDto>>.Ok([]));

        await sut.ExecuteAsync(new JsonObject(), CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<GetUpcomingMaintenanceQuery>(q => q.DaysAhead == 30 && q.IncludeOverdue),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_ProjectsOverdueSchedules()
    {
        mediator.Send(Arg.Any<GetUpcomingMaintenanceQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<List<MaintenanceScheduleDto>>.Ok(
                [new MaintenanceScheduleDto
                {
                    TruckId = Guid.NewGuid(),
                    TruckNumber = "TRK-101",
                    TypeDisplay = "Oil Change",
                    IsOverdue = true,
                    DaysUntilDue = -3
                }]));

        var result = await sut.ExecuteAsync(
            new JsonObject { ["days_ahead"] = 14 }, CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        var schedule = Assert.Single(root.GetProperty("schedules").EnumerateArray());
        Assert.Equal("TRK-101", schedule.GetProperty("truck_number").GetString());
        Assert.True(schedule.GetProperty("is_overdue").GetBoolean());
    }
}
