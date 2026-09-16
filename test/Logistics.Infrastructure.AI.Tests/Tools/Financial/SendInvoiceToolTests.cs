using Logistics.Infrastructure.AI.Tools.Financial;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Financial.Invoices.Commands;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using Logistics.Mediator;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Financial;

public class SendInvoiceToolTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly SendInvoiceTool _sut;

    public SendInvoiceToolTests()
    {
        _sut = new SendInvoiceTool(_mediator);
    }

    [Fact]
    public async Task Execute_MissingInvoiceId_ReturnsError()
    {
        var result = await _sut.ExecuteAsync(
            new JsonObject { ["recipient_email"] = "a@b.com", ["reasoning"] = "why" },
            CancellationToken.None);

        Assert.Contains("invoice_id", result);
        await _mediator.DidNotReceiveWithAnyArgs().Send<Result>(default!, default);
    }

    [Fact]
    public async Task Execute_MissingRecipientEmail_ReturnsError()
    {
        var result = await _sut.ExecuteAsync(
            new JsonObject { ["invoice_id"] = Guid.NewGuid().ToString(), ["reasoning"] = "why" },
            CancellationToken.None);

        Assert.Contains("recipient_email", result);
    }

    [Fact]
    public async Task Execute_ValidInput_SendsCommand()
    {
        var invoiceId = Guid.NewGuid();
        _mediator.Send(Arg.Any<SendInvoiceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok());

        var result = await _sut.ExecuteAsync(new JsonObject
        {
            ["invoice_id"] = invoiceId.ToString(),
            ["recipient_email"] = "billing@acme.com",
            ["personal_message"] = "Thanks for your business",
            ["reasoning"] = "Load delivered last week"
        }, CancellationToken.None);

        Assert.Contains("\"success\":true", result);
        await _mediator.Received(1).Send(
            Arg.Is<SendInvoiceCommand>(c =>
                c.InvoiceId == invoiceId &&
                c.RecipientEmail == "billing@acme.com" &&
                c.PersonalMessage == "Thanks for your business"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_CommandFails_ReturnsError()
    {
        _mediator.Send(Arg.Any<SendInvoiceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Invoice is cancelled"));

        var result = await _sut.ExecuteAsync(new JsonObject
        {
            ["invoice_id"] = Guid.NewGuid().ToString(),
            ["recipient_email"] = "billing@acme.com",
            ["reasoning"] = "Load delivered last week"
        }, CancellationToken.None);

        Assert.Contains("Invoice is cancelled", result);
    }
}
