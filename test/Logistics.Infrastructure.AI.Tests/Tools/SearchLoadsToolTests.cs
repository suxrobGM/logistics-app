using Logistics.Infrastructure.AI.Tools.Operations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class SearchLoadsToolTests
{
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly SearchLoadsTool sut;

    public SearchLoadsToolTests()
    {
        sut = new SearchLoadsTool(mediator);
    }

    [Fact]
    public void Name_IsSnakeCase()
    {
        Assert.Equal("search_loads", sut.Name);
    }

    [Fact]
    public async Task Execute_ParsesFiltersIntoQuery()
    {
        var customerId = Guid.NewGuid();
        mediator.Send(Arg.Any<GetLoadsQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<LoadDto>.Ok([], 0, 20));

        await sut.ExecuteAsync(new JsonObject
        {
            ["statuses"] = new JsonArray("Delivered", "delivered_typo_ignored"),
            ["customer_id"] = customerId.ToString(),
            ["start_date"] = "2026-07-20",
            ["page"] = 2
        }, CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<GetLoadsQuery>(q =>
                q.Statuses!.Single() == LoadStatus.Delivered &&
                q.CustomerId == customerId &&
                q.StartDate == new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc) &&
                q.Page == 2 &&
                q.PageSize == 20),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_MoreResultsThanPage_SetsTruncated()
    {
        mediator.Send(Arg.Any<GetLoadsQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<LoadDto>.Ok([CopilotToolTestData.CreateLoad()], 55, 20));

        var result = await sut.ExecuteAsync(new JsonObject(), CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        Assert.True(root.GetProperty("truncated").GetBoolean());
        Assert.Equal(55, root.GetProperty("total").GetInt32());
    }
}
