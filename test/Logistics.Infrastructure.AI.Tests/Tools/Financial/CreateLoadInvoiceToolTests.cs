using Logistics.Infrastructure.AI.Tools.Financial;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Financial.Invoices.Commands;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Financial;

public class CreateLoadInvoiceToolTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CreateLoadInvoiceTool _sut;

    public CreateLoadInvoiceToolTests()
    {
        _sut = new CreateLoadInvoiceTool(_mediator);
    }

    private static CustomerDto SomeCustomer => new() { Id = Guid.NewGuid(), Name = "Acme Logistics" };

    /// <summary>Stubs the create command and the read-back the tool does for number and currency.</summary>
    private Guid SetupCreatedInvoice(long number = 7)
    {
        var invoiceId = Guid.NewGuid();
        _mediator.Send(Arg.Any<CreateLoadInvoiceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Ok(invoiceId));
        _mediator.Send(Arg.Any<GetInvoiceByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<InvoiceDto>.Ok(new InvoiceDto
            {
                Id = invoiceId,
                Number = number,
                Subtotal = CopilotToolTestData.Usd(1500m),
                TaxTotal = CopilotToolTestData.Usd(0m),
                Total = CopilotToolTestData.Usd(1500m)
            }));
        return invoiceId;
    }

    [Fact]
    public async Task Execute_MissingLoadId_ReturnsError()
    {
        var result = await _sut.ExecuteAsync(new JsonObject(), CancellationToken.None);

        Assert.Contains("\"error\"", result);
        await _mediator.DidNotReceiveWithAnyArgs().Send<Result>(default!, default);
    }

    [Fact]
    public async Task Execute_LoadWithoutCustomer_ReturnsError()
    {
        var load = CopilotToolTestData.CreateLoad(customer: null);
        _mediator.Send(Arg.Any<GetLoadByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadDto>.Ok(load));

        var result = await _sut.ExecuteAsync(
            new JsonObject { ["load_id"] = load.Id.ToString(), ["reasoning"] = "Load delivered" },
            CancellationToken.None);

        Assert.Contains("no customer", result);
        await _mediator.DidNotReceive().Send(Arg.Any<CreateLoadInvoiceCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_LoadAlreadyInvoiced_ReturnsErrorWithExistingInvoice()
    {
        var invoice = new InvoiceDto
        {
            Id = Guid.NewGuid(),
            Status = InvoiceStatus.Sent,
            Subtotal = CopilotToolTestData.Usd(1500m),
            TaxTotal = CopilotToolTestData.Usd(0m),
            Total = CopilotToolTestData.Usd(1500m)
        };
        var load = CopilotToolTestData.CreateLoad(customer: SomeCustomer, invoice: invoice);
        _mediator.Send(Arg.Any<GetLoadByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadDto>.Ok(load));

        var result = await _sut.ExecuteAsync(
            new JsonObject { ["load_id"] = load.Id.ToString(), ["reasoning"] = "Load delivered" },
            CancellationToken.None);

        Assert.Contains("already has an invoice", result);
        Assert.Contains(invoice.Id.ToString(), result);
        await _mediator.DidNotReceive().Send(Arg.Any<CreateLoadInvoiceCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_NoAmountGiven_DefaultsToLoadDeliveryCostWithoutRecordingPayment()
    {
        var customer = SomeCustomer;
        var load = CopilotToolTestData.CreateLoad(deliveryCost: 2350m, customer: customer);
        _mediator.Send(Arg.Any<GetLoadByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadDto>.Ok(load));
        var invoiceId = SetupCreatedInvoice();

        var result = await _sut.ExecuteAsync(
            new JsonObject { ["load_id"] = load.Id.ToString(), ["reasoning"] = "Load delivered" },
            CancellationToken.None);

        var root = JsonDocument.Parse(result).RootElement;
        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.Equal(2350m, root.GetProperty("amount").GetDecimal());
        Assert.Equal(invoiceId, root.GetProperty("invoice_id").GetGuid());

        // The copilot must never mark the invoice paid on creation.
        await _mediator.Received(1).Send(
            Arg.Is<CreateLoadInvoiceCommand>(c =>
                c.LoadId == load.Id &&
                c.CustomerId == customer.Id &&
                c.PaymentAmount == 2350m &&
                !c.RecordPayment),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_ExplicitAmount_OverridesDeliveryCost()
    {
        var load = CopilotToolTestData.CreateLoad(deliveryCost: 2350m, customer: SomeCustomer);
        _mediator.Send(Arg.Any<GetLoadByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<LoadDto>.Ok(load));
        SetupCreatedInvoice();

        await _sut.ExecuteAsync(
            new JsonObject
            {
                ["load_id"] = load.Id.ToString(),
                ["amount"] = 999m,
                ["reasoning"] = "Load delivered"
            },
            CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<CreateLoadInvoiceCommand>(c => c.PaymentAmount == 999m),
            Arg.Any<CancellationToken>());
    }
}
