using Logistics.Infrastructure.AI.Tools.Financial;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using Logistics.Mediator;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Financial;

public class GetInvoiceToolTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly GetInvoiceTool _sut;

    public GetInvoiceToolTests()
    {
        _sut = new GetInvoiceTool(_mediator);
    }

    [Fact]
    public async Task Execute_MissingInvoiceId_ReturnsError()
    {
        var result = await _sut.ExecuteAsync(new JsonObject(), CancellationToken.None);

        Assert.Contains("invoice_id", result);
        await _mediator.DidNotReceiveWithAnyArgs().Send<Result<InvoiceDto>>(default!, default);
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
        _mediator.Send(Arg.Any<GetInvoiceByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<InvoiceDto>.Ok(invoice));

        var result = await _sut.ExecuteAsync(
            new JsonObject { ["invoice_id"] = invoice.Id.ToString() }, CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        Assert.Equal(500m, root.GetProperty("amount_paid").GetDecimal());
        Assert.Equal(2, root.GetProperty("payment_count").GetInt32());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("factoring_status").ValueKind);
    }

    /// <summary>A funded factored invoice reads Paid; the status is what tells the copilot who paid.</summary>
    [Fact]
    public async Task Execute_FactoredCustomer_ReportsFactoringStatus()
    {
        var invoice = new InvoiceDto
        {
            Id = Guid.NewGuid(),
            Status = InvoiceStatus.Paid,
            Subtotal = CopilotToolTestData.Usd(1000m),
            TaxTotal = CopilotToolTestData.Usd(0m),
            Total = CopilotToolTestData.Usd(1000m),
            Customer = new CustomerDto { Name = "Big Broker", FactoringCompanyId = Guid.NewGuid() },
            FactoringStatus = FactoringStatus.Funded
        };
        _mediator.Send(Arg.Any<GetInvoiceByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<InvoiceDto>.Ok(invoice));

        var result = await _sut.ExecuteAsync(
            new JsonObject { ["invoice_id"] = invoice.Id.ToString() }, CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        Assert.Equal("Funded", root.GetProperty("factoring_status").GetString());
    }
}
