using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;
using Logistics.Application.Modules.Financial.Invoices.Queries;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class PreviewTaxCalculationToolTests
{
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly PreviewTaxCalculationTool sut;

    public PreviewTaxCalculationToolTests()
    {
        sut = new PreviewTaxCalculationTool(mediator);
    }

    [Fact]
    public void Name_IsSnakeCase()
    {
        Assert.Equal("preview_tax_calculation", sut.Name);
    }

    [Fact]
    public async Task Execute_MissingCustomerId_ReturnsError()
    {
        var input = new JsonObject
        {
            ["currency"] = "USD",
            ["line_items"] = new JsonArray(new JsonObject { ["description"] = "x", ["amount"] = 100 })
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);

        Assert.Contains("\"error\"", result);
        await mediator.DidNotReceiveWithAnyArgs().Send<Result<PreviewInvoiceTaxResponse>>(default!, default);
    }

    [Fact]
    public async Task Execute_InvalidCustomerIdGuid_ReturnsError()
    {
        var input = new JsonObject
        {
            ["customer_id"] = "not-a-guid",
            ["currency"] = "USD",
            ["line_items"] = new JsonArray(new JsonObject { ["description"] = "x", ["amount"] = 100 })
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);

        Assert.Contains("invalid customer_id", result);
    }

    [Fact]
    public async Task Execute_MissingCurrency_ReturnsError()
    {
        var input = new JsonObject
        {
            ["customer_id"] = Guid.NewGuid().ToString(),
            ["line_items"] = new JsonArray(new JsonObject { ["description"] = "x", ["amount"] = 100 })
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);

        Assert.Contains("Missing currency", result);
    }

    [Fact]
    public async Task Execute_EmptyLineItems_ReturnsError()
    {
        var input = new JsonObject
        {
            ["customer_id"] = Guid.NewGuid().ToString(),
            ["currency"] = "USD",
            ["line_items"] = new JsonArray()
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);

        Assert.Contains("line_items", result);
    }

    [Fact]
    public async Task Execute_ValidInput_PassesQueryToMediatorAndShapesResponse()
    {
        var customerId = Guid.NewGuid();
        var lineItemId = Guid.NewGuid();
        var input = new JsonObject
        {
            ["customer_id"] = customerId.ToString(),
            ["currency"] = "EUR",
            ["line_items"] = new JsonArray(
                new JsonObject
                {
                    ["description"] = "Freight",
                    ["type"] = "BaseRate",
                    ["amount"] = 100m,
                    ["quantity"] = 2
                })
        };

        var response = new PreviewInvoiceTaxResponse
        {
            TaxBehavior = TaxBehavior.Exclusive,
            Subtotal = new Money { Amount = 200m, Currency = "EUR" },
            TaxTotal = new Money { Amount = 38m, Currency = "EUR" },
            Total = new Money { Amount = 238m, Currency = "EUR" },
            Lines = [new PreviewLineItemTax
            {
                LineItemId = lineItemId, RatePercent = 19m, TaxAmount = 38m, TaxCode = null
            }],
            Breakdown = [new InvoiceTaxLineDto
            {
                RatePercent = 19m,
                BaseAmount = 200m,
                TaxAmount = 38m,
                Jurisdiction = new TaxJurisdictionDto { CountryCode = "DE" },
                Description = "VAT 19% — DE"
            }]
        };

        mediator.Send(Arg.Any<PreviewInvoiceTaxQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<PreviewInvoiceTaxResponse>.Ok(response));

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var json = JsonDocument.Parse(result);
        var root = json.RootElement;

        Assert.Equal("Exclusive", root.GetProperty("tax_behavior").GetString());
        Assert.Equal("EUR", root.GetProperty("currency").GetString());
        Assert.Equal(200m, root.GetProperty("subtotal").GetDecimal());
        Assert.Equal(38m, root.GetProperty("tax_total").GetDecimal());
        Assert.Equal(238m, root.GetProperty("total").GetDecimal());

        var lines = root.GetProperty("lines").EnumerateArray().ToList();
        Assert.Single(lines);
        Assert.Equal(19m, lines[0].GetProperty("rate_percent").GetDecimal());

        var breakdown = root.GetProperty("breakdown").EnumerateArray().ToList();
        Assert.Single(breakdown);
        Assert.Equal("DE", breakdown[0].GetProperty("jurisdiction").GetString());

        // Verify the query carried the parsed inputs.
        await mediator.Received(1).Send(
            Arg.Is<PreviewInvoiceTaxQuery>(q =>
                q.Request.CustomerId == customerId &&
                q.Request.Currency == "EUR" &&
                q.Request.LineItems.Count == 1 &&
                q.Request.LineItems[0].Amount == 100m &&
                q.Request.LineItems[0].Quantity == 2 &&
                q.Request.LineItems[0].Type == InvoiceLineItemType.BaseRate),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_ReverseChargeResponse_FlowsThroughTaxBehavior()
    {
        var input = new JsonObject
        {
            ["customer_id"] = Guid.NewGuid().ToString(),
            ["currency"] = "EUR",
            ["line_items"] = new JsonArray(new JsonObject
            {
                ["description"] = "Freight",
                ["amount"] = 100m
            })
        };

        var response = new PreviewInvoiceTaxResponse
        {
            TaxBehavior = TaxBehavior.ReverseCharge,
            Subtotal = new Money { Amount = 100m, Currency = "EUR" },
            TaxTotal = new Money { Amount = 0m, Currency = "EUR" },
            Total = new Money { Amount = 100m, Currency = "EUR" },
            Lines = [],
            Breakdown = [new InvoiceTaxLineDto
            {
                RatePercent = 0m,
                BaseAmount = 100m,
                TaxAmount = 0m,
                Jurisdiction = new TaxJurisdictionDto { CountryCode = "FR" },
                Description = "Reverse charge — VAT to be accounted by recipient"
            }]
        };

        mediator.Send(Arg.Any<PreviewInvoiceTaxQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<PreviewInvoiceTaxResponse>.Ok(response));

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var root = JsonDocument.Parse(result).RootElement;

        Assert.Equal("ReverseCharge", root.GetProperty("tax_behavior").GetString());
        Assert.Equal(0m, root.GetProperty("tax_total").GetDecimal());
    }

    [Fact]
    public async Task Execute_QueryFails_ReturnsErrorPayload()
    {
        var input = new JsonObject
        {
            ["customer_id"] = Guid.NewGuid().ToString(),
            ["currency"] = "USD",
            ["line_items"] = new JsonArray(new JsonObject { ["description"] = "x", ["amount"] = 100 })
        };

        mediator.Send(Arg.Any<PreviewInvoiceTaxQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<PreviewInvoiceTaxResponse>.Fail("Customer has no billing address"));

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var root = JsonDocument.Parse(result).RootElement;

        Assert.Equal("Customer has no billing address", root.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Execute_NonNumericLineAmount_ReturnsError()
    {
        var input = new JsonObject
        {
            ["customer_id"] = Guid.NewGuid().ToString(),
            ["currency"] = "USD",
            ["line_items"] = new JsonArray(new JsonObject
            {
                ["description"] = "x",
                ["amount"] = "not-a-number"
            })
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);

        Assert.Contains("numeric amount", result);
    }
}
