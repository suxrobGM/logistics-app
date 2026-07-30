using Logistics.Infrastructure.AI.Tools.Financial;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Financial.Expenses.Queries;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class GetExpenseStatsToolTests
{
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly GetExpenseStatsTool sut;

    public GetExpenseStatsToolTests()
    {
        sut = new GetExpenseStatsTool(mediator);
    }

    [Fact]
    public void Name_IsSnakeCase()
    {
        Assert.Equal("get_expense_stats", sut.Name);
    }

    [Fact]
    public async Task Execute_ReturnsCategoryRollups()
    {
        mediator.Send(Arg.Any<GetExpenseStatsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<ExpenseStatsDto>.Ok(new ExpenseStatsDto
            {
                TotalAmount = 5000m,
                TotalCount = 12,
                ByTruckCategory = [new ExpenseCategoryStatDto { Category = "Fuel", Amount = 3200m, Count = 8 }]
            }));

        var result = await sut.ExecuteAsync(
            new JsonObject { ["from_date"] = "2026-07-01", ["to_date"] = "2026-07-31" },
            CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        Assert.Equal(5000m, root.GetProperty("total_amount").GetDecimal());
        var category = Assert.Single(root.GetProperty("by_truck_category").EnumerateArray());
        Assert.Equal("Fuel", category.GetProperty("Category").GetString());

        await mediator.Received(1).Send(
            Arg.Is<GetExpenseStatsQuery>(q => q.FromDate != null && q.ToDate != null),
            Arg.Any<CancellationToken>());
    }
}
