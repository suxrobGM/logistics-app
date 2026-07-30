using Logistics.Infrastructure.AI.Tools.Financial;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class GetInvoicesToolTests
{
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly GetInvoicesTool sut;

    public GetInvoicesToolTests()
    {
        sut = new GetInvoicesTool(mediator);
    }

    [Fact]
    public void Name_IsSnakeCase()
    {
        Assert.Equal("get_invoices", sut.Name);
    }

    [Fact]
    public async Task Execute_AlwaysScopesToLoadInvoices()
    {
        mediator.Send(Arg.Any<GetInvoicesQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<InvoiceDto>.Ok([], 0, 20));

        await sut.ExecuteAsync(
            new JsonObject { ["status"] = "sent" }, CancellationToken.None);

        // Payroll/subscription invoices must never leak into copilot answers.
        await mediator.Received(1).Send(
            Arg.Is<GetInvoicesQuery>(q =>
                q.InvoiceType == InvoiceType.Load &&
                q.Status == InvoiceStatus.Sent),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_ReturnsCompactInvoiceList()
    {
        mediator.Send(Arg.Any<GetInvoicesQuery>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<InvoiceDto>.Ok(
                [new InvoiceDto
                {
                    Id = Guid.NewGuid(),
                    Number = 12,
                    Status = InvoiceStatus.Sent,
                    Subtotal = CopilotToolTestData.Usd(1000m),
                    TaxTotal = CopilotToolTestData.Usd(80m),
                    Total = CopilotToolTestData.Usd(1080m)
                }], 1, 20));

        var result = await sut.ExecuteAsync(new JsonObject(), CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        var invoice = Assert.Single(root.GetProperty("invoices").EnumerateArray());
        Assert.Equal(1080m, invoice.GetProperty("total").GetDecimal());
        Assert.Equal("Sent", invoice.GetProperty("status").GetString());
    }
}
