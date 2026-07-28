using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Financial.Expenses.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class SearchExpensesToolTests
{
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly SearchExpensesTool sut;

    public SearchExpensesToolTests()
    {
        sut = new SearchExpensesTool(mediator);
    }

    [Fact]
    public void Name_IsSnakeCase()
    {
        Assert.Equal("search_expenses", sut.Name);
    }

    [Fact]
    public async Task Execute_ParsesFiltersAndProjectsCategory()
    {
        mediator.Send(Arg.Any<GetExpensesQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<ExpenseDto>.Ok(
                [new ExpenseDto
                {
                    Id = Guid.NewGuid(),
                    Type = ExpenseType.Truck,
                    Status = ExpenseStatus.Approved,
                    Amount = CopilotToolTestData.Usd(450m),
                    TruckCategory = TruckExpenseCategory.Tires
                }], 1, 20));

        var result = await sut.ExecuteAsync(new JsonObject
        {
            ["type"] = "truck",
            ["status"] = "Approved",
            ["from_date"] = "2026-07-01"
        }, CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        var expense = Assert.Single(root.GetProperty("expenses").EnumerateArray());
        Assert.Equal("Tires", expense.GetProperty("category").GetString());

        await mediator.Received(1).Send(
            Arg.Is<GetExpensesQuery>(q =>
                q.Type == ExpenseType.Truck &&
                q.Status == ExpenseStatus.Approved &&
                q.FromDate == new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc)),
            Arg.Any<CancellationToken>());
    }
}
