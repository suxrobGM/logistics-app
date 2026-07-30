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

public class GetInvoiceToolTests
{
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly GetInvoiceTool sut;

    public GetInvoiceToolTests()
    {
        sut = new GetInvoiceTool(mediator);
    }

    [Fact]
    public void Name_IsSnakeCase()
    {
        Assert.Equal("get_invoice", sut.Name);
    }

    [Fact]
    public async Task Execute_MissingInvoiceId_ReturnsError()
    {
        var result = await sut.ExecuteAsync(new JsonObject(), CancellationToken.None);

        Assert.Contains("invoice_id", result);
        await mediator.DidNotReceiveWithAnyArgs().Send<Result<InvoiceDto>>(default!, default);
    }

    [Fact]
    public async Task Execute_ValidInput_SumsPayments()
    {
        var invoice = new InvoiceDto
        {
            Id = Guid.NewGuid(),
            Number = 12,
            Status = InvoiceStatus.PartiallyPaid,
            Subtotal = CopilotToolTestData.Usd(1000m),
            TaxTotal = CopilotToolTestData.Usd(0m),
            Total = CopilotToolTestData.Usd(1000m),
            Payments =
            [
                new PaymentDto { Amount = CopilotToolTestData.Usd(300m) },
                new PaymentDto { Amount = CopilotToolTestData.Usd(200m) }
            ]
        };
        mediator.Send(Arg.Any<GetInvoiceByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<InvoiceDto>.Ok(invoice));

        var result = await sut.ExecuteAsync(
            new JsonObject { ["invoice_id"] = invoice.Id.ToString() }, CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        Assert.Equal(500m, root.GetProperty("amount_paid").GetDecimal());
        Assert.Equal(2, root.GetProperty("payment_count").GetInt32());
    }
}
