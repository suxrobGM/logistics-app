using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Financial.Expenses.Queries;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Application.Modules.IdentityAccess.Customers.Queries;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

/// <summary>
/// The four paged tools each hand-roll the same <c>{ items, count, total, truncated }</c> envelope
/// and their own <c>MaxResults</c>. That envelope is about to be extracted into one helper, so pin
/// it - including the per-tool collection key, which must NOT be unified.
/// </summary>
public class PagedToolEnvelopeTests
{
    private const int ExpectedPageSize = 20;

    private readonly IMediator mediator = Substitute.For<IMediator>();

    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

    private static void AssertEnvelope(JsonElement root, string collectionKey, int count, int total)
    {
        Assert.True(
            root.TryGetProperty(collectionKey, out var items),
            $"expected the collection to be named '{collectionKey}'");
        Assert.Equal(count, items.GetArrayLength());
        Assert.Equal(count, root.GetProperty("count").GetInt32());
        Assert.Equal(total, root.GetProperty("total").GetInt32());
        Assert.Equal(total > count, root.GetProperty("truncated").GetBoolean());
    }

    [Fact]
    public async Task SearchLoads_EmptyPage_EmitsEnvelopeUnderLoads()
    {
        mediator.Send(Arg.Any<GetLoadsQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<LoadDto>.Ok([], 0, ExpectedPageSize));

        var root = Parse(await new SearchLoadsTool(mediator)
            .ExecuteAsync(new JsonObject(), CancellationToken.None));

        AssertEnvelope(root, "loads", count: 0, total: 0);
    }

    [Fact]
    public async Task SearchLoads_MoreThanOnePage_MarksTruncated()
    {
        mediator.Send(Arg.Any<GetLoadsQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<LoadDto>.Ok([CopilotToolTestData.CreateLoad()], 91, ExpectedPageSize));

        var root = Parse(await new SearchLoadsTool(mediator)
            .ExecuteAsync(new JsonObject(), CancellationToken.None));

        AssertEnvelope(root, "loads", count: 1, total: 91);
    }

    [Fact]
    public async Task SearchCustomers_EmitsEnvelopeUnderCustomers_AndAlwaysRequestsFirstPage()
    {
        mediator.Send(Arg.Any<GetCustomersQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<CustomerDto>.Ok([], 0, ExpectedPageSize));

        var root = Parse(await new SearchCustomersTool(mediator)
            .ExecuteAsync(new JsonObject { ["page"] = 3 }, CancellationToken.None));

        AssertEnvelope(root, "customers", count: 0, total: 0);

        // search_customers has no page parameter - it pins Page = 1 regardless of input.
        await mediator.Received(1).Send(
            Arg.Is<GetCustomersQuery>(q => q.Page == 1 && q.PageSize == ExpectedPageSize),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchExpenses_EmitsEnvelopeUnderExpenses()
    {
        mediator.Send(Arg.Any<GetExpensesQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<ExpenseDto>.Ok([], 7, ExpectedPageSize));

        var root = Parse(await new SearchExpensesTool(mediator)
            .ExecuteAsync(new JsonObject(), CancellationToken.None));

        AssertEnvelope(root, "expenses", count: 0, total: 7);
    }

    [Fact]
    public async Task GetInvoices_EmitsEnvelopeUnderInvoices()
    {
        mediator.Send(Arg.Any<GetInvoicesQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<InvoiceDto>.Ok([], 0, ExpectedPageSize));

        var root = Parse(await new GetInvoicesTool(mediator)
            .ExecuteAsync(new JsonObject(), CancellationToken.None));

        AssertEnvelope(root, "invoices", count: 0, total: 0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    public async Task PageableTools_PassPageThroughWithSharedPageSize(int page)
    {
        mediator.Send(Arg.Any<GetLoadsQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<LoadDto>.Ok([], 0, ExpectedPageSize));
        mediator.Send(Arg.Any<GetExpensesQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<ExpenseDto>.Ok([], 0, ExpectedPageSize));
        mediator.Send(Arg.Any<GetInvoicesQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<InvoiceDto>.Ok([], 0, ExpectedPageSize));

        var input = new JsonObject { ["page"] = page };
        await new SearchLoadsTool(mediator).ExecuteAsync(input, CancellationToken.None);
        await new SearchExpensesTool(mediator).ExecuteAsync(input, CancellationToken.None);
        await new GetInvoicesTool(mediator).ExecuteAsync(input, CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<GetLoadsQuery>(q => q.Page == page && q.PageSize == ExpectedPageSize),
            Arg.Any<CancellationToken>());
        await mediator.Received(1).Send(
            Arg.Is<GetExpensesQuery>(q => q.Page == page && q.PageSize == ExpectedPageSize),
            Arg.Any<CancellationToken>());
        await mediator.Received(1).Send(
            Arg.Is<GetInvoicesQuery>(q => q.Page == page && q.PageSize == ExpectedPageSize),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PagedTools_QueryFails_EmitBareErrorWithNoEnvelope()
    {
        mediator.Send(Arg.Any<GetLoadsQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<LoadDto>.Fail("database unavailable"));

        var root = Parse(await new SearchLoadsTool(mediator)
            .ExecuteAsync(new JsonObject(), CancellationToken.None));

        Assert.Equal("database unavailable", root.GetProperty("error").GetString());
        Assert.False(root.TryGetProperty("loads", out _));
        Assert.False(root.TryGetProperty("truncated", out _));
    }
}
